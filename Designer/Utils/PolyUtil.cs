using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Models.Utils {
    public static class PolyUtil {
        /**
         * Checks center distance if this is smaller than 5 m returns <see cref="MinDistance"/>
         */
        public static (Position p1, Position p2)? IsSafe(this Polygon poly1, Polygon poly2) {
            double dis = poly1.Center().Distance(poly2.Center());
            if (dis < 150) {
                double smallest = -1;
                Position best1 = null;
                Position best2 = null;
                
                foreach (Position p1 in poly1) {
                    foreach (Position p2 in poly2) {
                        double d = p1.Distance(p2);
                        if (smallest >= 0 && d >= smallest) continue;
                        smallest = d;
                        best1 = p1;
                        best2 = p2;
                    }
                }

                return (best1, best2);
            }

            if (dis < 500) return MinDistanceOneDirection(poly1, poly2);
            return null;
        }
        
        public static (Position p1, Position p2) MinDistance(this Polygon poly1, Polygon poly2) {
            //Probeert bijde variates omdat het uitmaakt welke volgorde je ze in MinDistanceOneDirection stopt
            (Position p1, Position p2) best1 = MinDistanceOneDirection(poly1, poly2);
            (Position p1, Position p2) best2 = MinDistanceOneDirection(poly2, poly1);

            //Returned de beste variant
            return best1.p1.Distance(best1.p2) > best2.p1.Distance(best2.p2) ? best2 : best1;
        }

        private static (Position p1, Position p2) MinDistanceOneDirection(Polygon poly1, Polygon poly2) {
            double smallest = -1;
            Position best1 = null;
            Position best2 = null;

            //Loopt door alle zijdes heen van de polygon
            foreach ((Position p1, Position p2) in poly1.GetLines()) {
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
        public static bool DoesCollide(this Polygon poly1, Polygon poly2) {
            return poly1.DoesOutsideCollide(poly2) || poly1.InsideNoOutside(poly2) || poly2.InsideNoOutside(poly1);
        }

        private static bool DoesOutsideCollide(this Polygon poly1, Polygon poly2) {
            foreach ((Position p1, Position p2) line1 in poly1.GetLines())
            foreach ((Position p1, Position p2) line2 in poly2.GetLines()) {
                PointF[] array = Intersector.Intersection(
                    line1.p1.Point(), line1.p2.Point(),
                    line2.p1.Point(), line2.p2.Point()
                );

                if (array.Any()) return true;
            }

            return false;
        }

        /**
         * Checks if poly2 is inside poly1
         */
        public static bool Inside(this Polygon poly1, Polygon poly2) {
            return poly2.InsideNoOutside(poly1);// && poly1.DoesOutsideCollide(poly2);
        }

        private static bool InsideNoOutside(this Polygon poly1, Polygon poly2) {
            return poly2.All(poly1.Inside);
        }

        public static bool Inside(this Polygon poly, Position position) {
            int j = poly.Count - 1;

            bool result = false;

            // Loopt door alle punten in de polygon
            for (int i = 0; i < poly.Count(); i++) {
                // Kijkt of de gegeven point in de polygon ligt qua coordinaten
                if (poly[i].Y < position.Y && poly[j].Y >= position.Y ||
                    poly[j].Y < position.Y && poly[i].Y >= position.Y) {
                    if (poly[i].X + ((double) position.Y - poly[i].Y) / (poly[j].Y - (double) poly[i].Y) *
                        (poly[j].X - (double) poly[i].X) < position.X) {
                        result = !result;
                    }
                }

                j = i;
            }

            return result;
        }

        private static PointF Point(this Position position) { return new PointF(position.X, position.Y); }
    }

    // port of this JavaScript code with some changes:
    //   http://www.kevlindev.com/gui/math/intersection/Intersection.js
    // found here:
    //   http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect/563240#563240

    public class Intersector {
        static double MyEpsilon = 0.00001;

        private static float[] OverlapIntervals(float ub1, float ub2) {
            float l = Math.Min(ub1, ub2);
            float r = Math.Max(ub1, ub2);
            float A = Math.Max(0, l);
            float B = Math.Min(1, r);
            if (A > B) // no intersection
                return new float[] { };
            else if (A == B) return new float[] {A};
            else // if (A < B)
                return new float[] {A, B};
        }

        // IMPORTANT: a1 and a2 cannot be the same, e.g. a1--a2 is a true segment, not a point
        // b1/b2 may be the same (b1--b2 is a point)
        private static PointF[] OneD_Intersection(PointF a1, PointF a2, PointF b1, PointF b2) {
            //float ua1 = 0.0f; // by definition
            //float ua2 = 1.0f; // by definition
            float ub1, ub2;

            float denomx = a2.X - a1.X;
            float denomy = a2.Y - a1.Y;

            if (Math.Abs(denomx) > Math.Abs(denomy)) {
                ub1 = (b1.X - a1.X) / denomx;
                ub2 = (b2.X - a1.X) / denomx;
            } else {
                ub1 = (b1.Y - a1.Y) / denomy;
                ub2 = (b2.Y - a1.Y) / denomy;
            }

            List<PointF> ret = new List<PointF>();
            float[] interval = OverlapIntervals(ub1, ub2);
            foreach (float f in interval) {
                float x = a2.X * f + a1.X * (1.0f - f);
                float y = a2.Y * f + a1.Y * (1.0f - f);
                PointF p = new PointF(x, y);
                ret.Add(p);
            }

            return ret.ToArray();
        }

        private static bool PointOnLine(PointF p, PointF a1, PointF a2) {
            float dummyU = 0.0f;
            double d = DistFromSeg(p, a1, a2, MyEpsilon, ref dummyU);
            return d < MyEpsilon;
        }

        private static double DistFromSeg(PointF p, PointF q0, PointF q1, double radius, ref float u) {
            // formula here:
            //http://mathworld.wolfram.com/Point-LineDistance2-Dimensional.html
            // where x0,y0 = p
            //       x1,y1 = q0
            //       x2,y2 = q1
            double dx21 = q1.X - q0.X;
            double dy21 = q1.Y - q0.Y;
            double dx10 = q0.X - p.X;
            double dy10 = q0.Y - p.Y;
            double segLength = Math.Sqrt(dx21 * dx21 + dy21 * dy21);
            if (segLength < MyEpsilon) throw new Exception("Expected line segment, not point.");
            double num = Math.Abs(dx21 * dy10 - dx10 * dy21);
            double d = num / segLength;
            return d;
        }

        // this is the general case. Really really general
        public static PointF[] Intersection(PointF a1, PointF a2, PointF b1, PointF b2) {
            if (a1.Equals(a2) && b1.Equals(b2)) {
                // both "segments" are points, return either point
                if (a1.Equals(b1)) return new PointF[] {a1};
                return new PointF[] { };
            }

            if (b1.Equals(b2)) // b is a point, a is a segment
            {
                if (PointOnLine(b1, a1, a2)) return new PointF[] {b1};
                return new PointF[] { };
            } else if (a1.Equals(a2)) // a is a point, b is a segment
            {
                if (PointOnLine(a1, b1, b2)) return new PointF[] {a1};
                else return new PointF[] { };
            }

            // at this point we know both a and b are actual segments

            float ua_t = (b2.X - b1.X) * (a1.Y - b1.Y) - (b2.Y - b1.Y) * (a1.X - b1.X);
            float ub_t = (a2.X - a1.X) * (a1.Y - b1.Y) - (a2.Y - a1.Y) * (a1.X - b1.X);
            float u_b = (b2.Y - b1.Y) * (a2.X - a1.X) - (b2.X - b1.X) * (a2.Y - a1.Y);

            // Infinite lines intersect somewhere
            if (!(-MyEpsilon < u_b && u_b < MyEpsilon)) // e.g. u_b != 0.0
            {
                float ua = ua_t / u_b;
                float ub = ub_t / u_b;
                if (0.0f <= ua && ua <= 1.0f && 0.0f <= ub && ub <= 1.0f) {
                    // Intersection
                    return new PointF[] {
                        new PointF(
                            a1.X + ua * (a2.X - a1.X),
                            a1.Y + ua * (a2.Y - a1.Y)
                        )
                    };
                } else {
                    // No Intersection
                    return new PointF[] { };
                }
            } else // lines (not just segments) are parallel or the same line
            {
                // Coincident
                // find the common overlapping section of the lines
                // first find the distance (squared) from one point (a1) to each point
                if ((-MyEpsilon < ua_t && ua_t < MyEpsilon)
                    || (-MyEpsilon < ub_t && ub_t < MyEpsilon)) {
                    if (a1.Equals(a2)) // danger!
                        return OneD_Intersection(b1, b2, a1, a2);
                    else // safe
                        return OneD_Intersection(a1, a2, b1, b2);
                } else {
                    // Parallel
                    return new PointF[] { };
                }
            }
        }
    }
}