using System;
using System.Collections.Generic;
using Models;

namespace Designer.Utils {
    /*
function sqr(x) { return x * x }
function dist2(v, w) { return sqr(v.x - w.x) + sqr(v.y - w.y) }
function distToSegmentSquared(p, v, w) {
  var l2 = dist2(v, w);
  if (l2 == 0) return dist2(p, v);
  var t = ((p.x - v.x) * (w.x - v.x) + (p.y - v.y) * (w.y - v.y)) / l2;
  t = Math.max(0, Math.min(1, t));
  return dist2(p, { x: v.x + t * (w.x - v.x),
                    y: v.y + t * (w.y - v.y) });
}
function distToSegment(p, v, w) { return Math.sqrt(distToSegmentSquared(p, v, w)); }
     */
    
    public static class PolyUtil {
        public static void MinDistance(List<Position> poly1, List<Position> poly2) {
            List<double> distances = new List<double>();
            
            for (int i = 1; i < poly1.Count; i++) {
                Position p1 = poly1[i - 1];
                Position p2 = poly1[i];

                double distance = p1.Distance(p2);
                foreach (Position to in poly2) {
                    if (distance == 0) distances.Add(p1.Distance(to));
                    else {
                        
                    }
                }
            }
            
            Console.WriteLine(poly1);
            Console.WriteLine(poly2);
        }
    }
}