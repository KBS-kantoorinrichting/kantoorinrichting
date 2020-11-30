using System.Collections.Generic;
using Models;

namespace Designer.Utils {
    public static class PolyUtil {
        public static (Position p1, Position p2) MinDistance(List<Position> poly1, List<Position> poly2) {
            //Probeert bijde variates omdat het uitmaakt welke volgorde je ze in MinDistanceOneDirection stopt
            (Position p1, Position p2) best1 = MinDistanceOneDirection(poly1, poly2);
            (Position p1, Position p2) best2 = MinDistanceOneDirection(poly2, poly1);

            //Returned de beste variant
            return best1.p1.Distance(best1.p2) > best2.p1.Distance(best2.p2) ? best2 : best1;
        }

        private static (Position p1, Position p2) MinDistanceOneDirection(List<Position> poly1, List<Position> poly2) {
            double smallest = -1;
            Position best1 = null;
            Position best2 = null;

            //Loopt door alle zijdes heen van de polygon
            for (int i = 0; i < poly1.Count; i++) {
                Position p1 = poly1[i];
                Position p2 = poly1[(i + 1) % poly1.Count];

                //Vergelijkt deze vervolgens met de hoeken van de 2 polygon
                foreach (Position to in poly2) {
                    Position from;
                    if (Equals(p1, p2)) from = p1;
                    else {
                        //Berekend waar het punt zicht bevind relatief to de lijn
                        double dX = p2.X - p1.X;
                        double dY = p2.Y - p1.Y;
                        double lenSq = dX * dX + dY * dY;
                        double param = lenSq == 0 ? -1 : ((to.X - p1.X) * dX + (to.Y - p1.Y) * dY) / lenSq;

                        if (param < 0) {
                            from = p1;
                        } else if (param > 1) {
                            from = p2;
                        } else {
                            //Berkend het punt met een haakse lijn van de lijn to het punt
                            from = new Position((int) (p1.X + param * dX), (int) (p1.Y + param * dY));
                        }
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
        public static bool DoesCollide(List<Position> poly1, List<Position> poly2) {
            for (int i = 0; i < poly1.Count; i++) {
                (Position p1, Position p2) line1 = (poly1[i], poly1[(i + 1) % poly1.Count]);
                
                for (int j = 0; j < poly2.Count; j++) {
                    (Position p1, Position p2) line2 = (poly1[j], poly1[(j + 1) % poly2.Count]);
                
                    if (FindIntersection(line1, line2) != null) return true;
                }                
            }
            
            return false;
        }

        /**
         * <returns>null wanneer de line intersection niet binnen het lijn segment ligt</returns>
         * <seealso cref="FindLineIntersection"/>
         */
        public static Position FindIntersection((Position p1, Position p2) line1, (Position p1, Position p2) line2) {
            Position i = FindLineIntersection(line1, line2);
            (Position p1, Position p2) = line1;
            
            if (i.X < p1.X || i.X > p1.X || i.Y < p1.Y || i.Y > p1.Y) {
                return null;
            }

            return i;
        }

        /**
         * Berekend het punt waar twee lijnen met elkaar snijden
         * Door middel van:
         * <code>
         * y = ax + b
         * y = Ax + B
         * ax + b = Ax + B
         * ax + Ax = b + B
         * (a + A)x = b + B
         * x = (b + B) / (a + A)
         * 
         * a = dy / dx
         * b = y - ax
         * </code>
         */
        public static Position FindLineIntersection(
            (Position p1, Position p2) line1,
            (Position p1, Position p2) line2
        ) {
            (double a1, double b1) = Unbind(line1);
            (double a2, double b2) = Unbind(line1);

            double x = ((b1 + b2) / (a1 + a2));
            double y = x * a1 + b1;

            return new Position((int) x, (int) y);
        }

        private static (double a, double b) Unbind((Position p1, Position p2) line) {
            (Position p1, Position p2) = line;

            int dx = (p1?.X ?? 0) - (p2?.X ?? 0);
            int dy = (p1?.Y ?? 0) - (p2?.Y ?? 0);
            double a = (double) dy / dx;
            double b = (p1?.Y ?? 0) - a * (p1?.X ?? 0);

            return (a, b);
        }
    }
}