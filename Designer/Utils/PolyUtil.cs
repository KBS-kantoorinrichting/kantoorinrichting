using System;
using System.Linq;

namespace Models.Utils {
    public static class PolyUtil {
        /**
         * Checks center distance if this is smaller than 5 m and bigger then 1.5m returns
         * <see cref="MinDistance" />
         */
        public static bool IsSafe(this Polygon poly1, Polygon poly2) {
            double dis = poly1.Center().Distance(poly2.Center());
            //TODO use bounds to check this
            if (dis <= 150) return false;
            if (dis >= 500) return true;
            (Position p1, Position p2) = MinDistance(poly1, poly2);
            return p1.Distance(p2) > 150;
        }

        public static (bool needed, bool safe) PreciseNeeded(this Polygon poly1, Polygon poly2, double distance) {
            Position c1 = poly1.Center();
            Position c2 = poly2.Center();

            double dis = c1.DistanceManhattan(c2);
            if (dis <= 150) return (false, true);
            if (dis >= 800) return (false, false);

            dis = c1.Distance(c2);
            if (dis <= 150) return (false, false);
            if (dis >= 500) return (false, true);
            return (true, false);
        }

        public static (Position p1, Position p2) MinDistance(this Polygon poly1, Polygon poly2) {
            //Probeert bijde variates omdat het uitmaakt welke volgorde je ze in MinDistanceOneDirection stopt
            (Position p1, Position p2) best1 = MinDistanceOneDirection(poly1, poly2);
            (Position p1, Position p2) best2 = MinDistanceOneDirection(poly2, poly1);

            if (best1.p1 == null) return best2;
            if (best2.p1 == null) return best1;

            //Returned de beste variant
            return best1.p1.Distance(best1.p2) > best2.p1.Distance(best2.p2) ? best2 : best1;
        }

        private static (Position p1, Position p2) MinDistanceOneDirection(Polygon poly1, Polygon poly2) {
            double smallest = -1;
            Position best1 = null;
            Position best2 = null;

            //Loopt door alle zijdes heen van de polygon
            foreach (Line line in poly1.GetLines()) {
                (Position p1, Position p2) = line.AsTuple;
                //Vergelijkt deze vervolgens met de hoeken van de 2 polygon
                foreach (Position to in poly2) {
                    Position from;
                    if (Equals(p1, p2)) {
                        @from = p1;
                    } else {
                        //Berekend waar het punt zicht bevind relatief to de lijn
                        double dX = p2.X - p1.X;
                        double dY = p2.Y - p1.Y;
                        double lenSq = dX * dX + dY * dY;
                        double param = lenSq == 0 ? -1 : ((to.X - p1.X) * dX + (to.Y - p1.Y) * dY) / lenSq;

                        if (param < 0) @from = p1;
                        else if (param > 1) @from = p2;
                        else //Berkend het punt met een haakse lijn van de lijn to het punt
                            @from = new Position((int) (p1.X + param * dX), (int) (p1.Y + param * dY));
                    }

                    //Houdt de kleinste afstanden bij
                    double dis = from.Distance(to);
                    if (smallest >= 0 && dis >= smallest) continue;
                    smallest = dis;
                    best1 = to;
                    best2 = from;
                }
            }

            return (best1, best2);
        }

        /**
         * Vergelijkt alle lijn delen met elkaar en kijkt of deze snijden
         */
        public static bool DoesCollide(this Polygon poly1, Polygon poly2) {
            return poly1.DoesOutsideCollide(poly2) || poly1.InsideNoOutside(poly2) || poly2.InsideNoOutside(poly1);
        }

        private static bool DoesOutsideCollide(this Polygon poly1, Polygon poly2) {
            foreach (Line line1 in poly1.GetLines())
            foreach (Line line2 in poly2.GetLines()) {
                Position intersection = line1.IntersectionLineSegment(line2);
                if (intersection != null) return true;
            }

            return false;
        }

        /**
         * Checks if poly2 is inside poly1
         */
        public static bool Inside(this Polygon poly1, Polygon poly2) {
            return poly1.InsideNoOutside(poly2) && !poly2.DoesOutsideCollide(poly1);
        }

        /**
         * Checked of alle coordinaten van poly 2 in poly 1 zitten
         */
        private static bool InsideNoOutside(this Polygon poly1, Polygon poly2) { return poly2.All(poly1.Inside); }

        public static bool Inside(this Polygon poly, Position position) {
            int j = poly.Count - 1;

            bool result = false;

            // Loopt door alle punten in de polygon
            for (int i = 0; i < poly.Count(); i++) {
                // Kijkt of de gegeven point in de polygon ligt qua coordinaten
                if (poly[i].Y < position.Y && poly[j].Y >= position.Y ||
                    poly[j].Y < position.Y && poly[i].Y >= position.Y)
                    if (poly[i].X + ((double) position.Y - poly[i].Y) / (poly[j].Y - (double) poly[i].Y) *
                        (poly[j].X - (double) poly[i].X) < position.X)
                        result = !result;

                j = i;
            }

            return result;
        }
    }

    public static class LineUtil {
        /**
         * Berekend het verschil in X. Kan negatief zijn
         */
        private static int DeltaX(this Line line) { return line.P1.X - line.P2.X; }

        /**
         * Berekend het verschil in y. Kan negatief zijn
         */
        private static int DeltaY(this Line line) { return line.P1.Y - line.P2.Y; }

        /**
         * Ontbind de lijn na de vorm y = ax + b
         */
        public static (double a, double b)? AsFormulaX(this Line line) {
            int dx = line.DeltaX();
            int dy = line.DeltaY();

            //Als er geen verschil is op de x as dan is het een verticale lijne en kan die niet ontbonden worden naar y = ax + b
            if (dx == 0) return null;

            double a = (double) dy / dx;
            double b = line.P1.Y - a * line.P1.X;

            return (a, b);
        }

        /**
         * Ontbind de lijn na de vorm x = ay + b
         */
        public static (double a, double b)? AsFormulaY(this Line line) {
            int dx = line.DeltaX();
            int dy = line.DeltaY();

            //Als er geen verschil is op de y as dan is het een horizontale lijne en kan die niet ontbonden worden naar x = ay + b
            if (dy == 0) return null;

            double a = (double) dx / dy;
            double b = line.P1.X - a * line.P1.Y;

            return (a, b);
        }

        /**
         * Berekend de lijn die evenredig is aan line met een afstand van distance
         */
        public static Line OffsetPerpendicular(this Line line, int distance, bool up = true) {
            //Ontbind de lijn op het x,y coordinaten systeem of y,x coordinaten systeem
            (double a, double b)? f = line.AsFormulaX();
            bool x = f.HasValue;
            if (!x) f = line.AsFormulaY();
            if (!f.HasValue) return null;

            (double a, double _) = f.Value;

            //Berekend de afstand met de bepaalde hoek
            double r = distance * a;
            int dB = (int) Math.Sqrt(r * r + distance * distance) * (up ? 1 : -1);

            //Geeft de juiste offset op basis van het coordinaten systeem
            return x ? line.Offset(y: dB) : line.Offset(dB);
        }

        /**
         * Berekend de snijpunt van twee lijnen die op het lijndeel zelf licht
         */
        public static Position IntersectionLineSegment(this Line line1, Line line2) {
            Position i = line1.Intersection(line2);
            if (i == null) return null;

            (Position p1, Position p2) = line1.AsTuple;

            //Als het punt niet binnen het gebied licht van de lijn geeft die geen punt terug 
            if (p1.X > p2.X && (p1.X < i.X || i.X < p2.X)) return null;
            if (p2.X > p1.X && (p2.X < i.X || i.X < p1.X)) return null;
            if (p1.Y > p2.Y && (p1.Y < i.Y || i.Y < p2.Y)) return null;
            if (p2.Y > p1.Y && (p2.Y < i.Y || i.Y < p1.Y)) return null;

            (p1, p2) = line2.AsTuple;

            if (p1.X > p2.X && (p1.X < i.X || i.X < p2.X)) return null;
            if (p2.X > p1.X && (p2.X < i.X || i.X < p1.X)) return null;
            if (p1.Y > p2.Y && (p1.Y < i.Y || i.Y < p2.Y)) return null;
            if (p2.Y > p1.Y && (p2.Y < i.Y || i.Y < p1.Y)) return null;

            return i;
        }

        /**
         * Berekend het snijpunt van 2 lijnen, deze hoefd niet op het lijndeel te liggen
         */
        public static Position Intersection(this Line line1, Line line2) {
            (double a, double b)? l1Fx = line1.AsFormulaX();
            (double a, double b)? l2Fx = line2.AsFormulaX();

            //Als beide lijnen een valide ontbinding hebben op het x,y coordinaten systeem
            if (l1Fx.HasValue && l2Fx.HasValue) {
                (double a1, double b1) = l1Fx.Value;
                (double a2, double b2) = l2Fx.Value;
                //Als de lijnen evenredig zijn is er geen snijpunt
                if (a1 - a2 == 0) return null;

                // Berekend het snijpunt door middel van:
                // y = a1x + b1, y = a2x + b2
                // a1x + b1 = a2x + b2
                // a1x - a2x = b2 - b1
                // (a1 - a2)x = b2 - b1
                // x = (b2 - b1) / (a1 - a2)
                double x = (b2 - b1) / (a1 - a2);

                //Vult de x in bij de eerste formule om y te berekenen
                double y = x * a1 + b1;

                return new Position((int) x, (int) y);
            }

            (double a, double b)? l1Fy = line1.AsFormulaY();
            (double a, double b)? l2Fy = line2.AsFormulaY();

            //Als beide lijnen een valide ontbinding hebben op het y,x coordinaten systeem
            if (l1Fy.HasValue && l2Fy.HasValue) {
                (double a1, double b1) = l1Fy.Value;
                (double a2, double b2) = l2Fy.Value;
                //Als de lijnen evenredig zijn is er geen snijpunt
                if (a1 - a2 == 0) return null;

                // Berekend het snijpunt door middel van:
                // x = a1y + b1, x = a2y + b2
                // a1y + b1 = a2y + b2
                // a1y - a2y = b2 - b1
                // (a1 - a2)y = b2 - b1
                // y = (b2 - b1) / (a1 - a2)
                double y = (b2 - b1) / (a1 - a2);

                //Vult de y in bij de eerste formule om x te berekenen
                double x = y * a1 + b1;

                return new Position((int) x, (int) y);
            }

            //Als lijn1 op het x,y systeem zit en lijn2 op het y,x systeem
            //Dit kan alleen gebruiken als lijn1 een horizontale lijn is en lijn2 een verticale
            if (l1Fx.HasValue && l2Fy.HasValue) {
                (_, double b1) = l1Fx.Value;
                (_, double b2) = l2Fy.Value;
                return new Position((int) b2, (int) b1);
            }

            //Als lijn1 op het y,x systeem zit en lijn2 op het x,y systeem
            //Dit kan alleen gebruiken als lijn1 een verticale lijn is en lijn2 een horizontale
            if (l1Fy.HasValue && l2Fx.HasValue) {
                (_, double b1) = l1Fy.Value;
                (_, double b2) = l2Fx.Value;
                return new Position((int) b1, (int) b2);
            }

            //Alle andere mogelijkheden zouden geen snijpunt hebben
            //bijv: een lijn die uit 2x het zelfde coordinaat bestaat
            return null;
        }

        /**
         * Berekend de haakse lijn op een lijn met een bepaalde afstand vanaf het midden
         */
        public static Line RightAngleLine(this Line line, int dis = 50) {
            if (line.P1 == null || line.P2 == null) return null;
            (double a, double b)? f = line.AsFormulaX();
            bool x = f.HasValue;
            if (!x) f = line.AsFormulaY();
            if (!f.HasValue) return null;

            (double a, double b) = f.Value;
            (int cX, int cY) = line.Center.AsTuple;

            Position p1, p2;

            switch (x) {
                //Als de lijn horizontaal is
                case true when a == 0:
                    p1 = new Position(cX, (int) b - dis);
                    p2 = new Position(cX, (int) b + dis);
                    break;
                //Als de lijn verticaal is
                case false:
                    p1 = new Position((int) b - dis, cY);
                    p2 = new Position((int) b + dis, cY);
                    break;
                //Als de lijn niet recht is
                default:
                    a = -1 / a;
                    b = cY - cX * a;

                    double d = Math.Sqrt(1 + a * a);
                    double dx = dis / d;

                    p1 = new Position((int) (cX - dx), (int) ((cX - dx) * a + b));
                    p2 = new Position((int) (cX + dx), (int) ((cX + dx) * a + b));
                    break;
            }

            return new Line(p1, p2);
        }
    }
}