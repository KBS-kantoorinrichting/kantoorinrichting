﻿using System.Collections.Generic;
using Models;
namespace Designer.Utils {
    public static class PolyUtil {
        public static (Position p1, Position p2) MinDistance(List<Position> poly1, List<Position> poly2) {
            (Position p1, Position p2) best1 = MinDistanceOneDirection(poly1, poly2);
            (Position p1, Position p2) best2 = MinDistanceOneDirection(poly2, poly1);
            return best1.p1.Distance(best1.p2) > best2.p1.Distance(best2.p2) ? best2 : best1;
        }

        private static (Position p1, Position p2) MinDistanceOneDirection(List<Position> poly1, List<Position> poly2) {
            double smallest = -1;
            Position best1 = null;
            Position best2 = null;

            for (int i = 0; i < poly1.Count; i++) {
                Position p1 = poly1[i];
                Position p2 = poly1[(i + 1) % poly1.Count];
                
                foreach (Position to in poly2) {
                    Position from;
                    if (Equals(p1, p2)) from = p1;
                    else {
                        double dX = p2.X - p1.X;
                        double dY = p2.Y - p1.Y;
                        double lenSq = dX * dX + dY * dY;
                        double param = lenSq == 0 ? -1 : ((to.X - p1.X) * dX + (to.Y - p1.Y) * dY) / lenSq;
                        if (param < 0) {
                            from = p1;
                        } else if (param > 1) {
                            from = p2;
                        } else {
                            from = new Position((int) (p1.X + param * dX), (int) (p1.Y + param * dY));
                        }
                    }
                    
                    double dis = from.Distance(to);
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