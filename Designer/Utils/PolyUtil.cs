using System;
using System.Collections.Generic;
using System.Linq;
using Models;

namespace Designer.Utils {
    public static class PolyUtil {
        public static (Position p1, Position p2) MinDistance(List<Position> poly1, List<Position> poly2) {
            double smallest = -1;
            Position best1 = null;
            Position best2 = null;

            for (int i = 1; i < poly1.Count; i++) {
                Position p1 = poly1[i - 1];
                Position p2 = poly1[i];

                double distance = p1.Distance(p2);
                foreach (Position to in poly2) {
                    Position from;
                    if (distance == 0) from = p1;
                    else {
                        double t = ((to.X - p1.X) * (p2.X - p1.X) + (to.Y - p1.Y) * (p2.Y - p1.Y)) / distance;
                        t = Math.Max(0, Math.Min(1, t));
                        from = new Position((int) (p1.X + t * (p2.X - p1.X)), (int) (p1.Y + t * (p2.Y - p1.Y)));
                    }
                    
                    double dis = to.Distance(to);
                    if (smallest >= 0 && dis >= smallest) continue;
                    smallest = dis;
                    best1 = to;
                    best2 = from;
                }
            }
            
            return (best1, best2);
        }
    }
}