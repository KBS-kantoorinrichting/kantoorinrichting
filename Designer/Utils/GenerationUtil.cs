using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Designer.View.Components;
using Designer.ViewModel;
using Models;
using Models.Utils;

namespace Designer.Utils {
    public static class GenerationUtils {
        public static void GeneratePlexi(this DesignEditor designer) {
            // genereerdt plexiglas
            foreach (DistanceLine distanceLine in designer._lines.Values
                .SelectMany(d => d.Values)
                .Distinct()) {
                Line line = new Line(distanceLine.P1, distanceLine.P2);
                line = line.RightAngleLine();
                if (line == null) continue;
                designer.PlexiLines.Add(new Polygon(line.AsList()));
            }

            designer.UpdateDbPlexiglass();
            designer.RenderPolyPlexi();
        }

        public static void GenerateWalkRoute(this DesignEditor designer) {
            // genereer loop route
            int distance = 50;

            List<Line> lines = designer.Design.Room.GetPoly().GetLines().ToList();
            List<Line> correct = lines.Select(l => (Line) null).ToList();
            //Gaat door alle hoeken (lijn paren) heen om te kijken waar maar 1 mogelijk is, om hiervandaan te starten
            int start = -1;
            for (int i = 0; i < lines.Count; i++) {
                Line l1 = lines[i];
                Line l2 = lines[(i + 1) % lines.Count];

                Line foundL1 = null;
                Line foundL2 = null;
                for (int r = 0; r < 4; r++) {
                    Line tempL1 = l1.OffsetPerpendicular(distance, r % 2 == 0);
                    Line tempL2 = l2.OffsetPerpendicular(distance, r / 2 == 0);

                    Position inter = tempL1.Intersection(tempL2);
                    if (inter == null || !designer.Design.Room.GetPoly().Inside(inter)) continue;
                    //Als die een tweede punt vind dan is dit geen geldige hoek
                    if (foundL1 != null) {
                        foundL1 = null;
                        foundL2 = null;
                        break;
                    }

                    foundL1 = tempL1;
                    foundL2 = tempL2;
                }

                if (foundL1 == null) continue;
                correct[i] = foundL1;
                start = (i + 1) % lines.Count;
                correct[start] = foundL2;
                break;
            }

            //Start bij de eerste hoek waar maar 1 mogelijk punt is en pakt vervolgens altijd de verste afstand hiervan voor de volgende lijn
            for (int i = start; i != start - 1; i = (i + 1) % lines.Count) {
                int j = (i + 1) % lines.Count;
                if (correct[j] != null) break;
                Line before = correct[i];
                Line toTest = lines[j];

                Line l1 = toTest.OffsetPerpendicular(distance);
                Line l2 = toTest.OffsetPerpendicular(distance, false);

                Position inter1 = before.Intersection(l1);
                if (inter1 == null || !designer.Design.Room.GetPoly().Inside(inter1)) {
                    correct[j] = l2;
                    continue;
                }

                Position inter2 = before.Intersection(l2);
                if (inter2 == null || !designer.Design.Room.GetPoly().Inside(inter2)) {
                    correct[j] = l1;
                    continue;
                }

                double d1 = before.P1.Distance(inter1);
                double d2 = before.P1.Distance(inter2);

                correct[j] = d1 > d2 ? l1 : l2;
            }

            //Zoekt voor alle lijn de snijpunten om de route te maken
            List<Position> positions = new List<Position>();
            for (int i = 0; i < correct.Count; i++) {
                Line l1 = correct[i];
                Line l2 = correct[(i + 1) % lines.Count];
                positions.Add(l1.Intersection(l2));
            }

            designer._route = new Polygon(positions);
            designer.RenderRoute();
        }
        
        /**
         * Loopt alle mogelijke plaatsingen door om vervolgens de kamer zo vol mogelijk te krijgen
         */
        public static void GenerateLayout(this DesignEditor designer)
        {
            Models.Polygon room = designer.Design.Room.GetPoly();
            //TODO Pakt momententeel nog het eerste product om te plaatsen
            Product product = designer.Products.First();

            //Pakt de minimale en maximale punten
            Position min = room.Min();
            Position max = room.Max();

            //Berekend de afstand per stap door middel van de afstand tussen de 2 boudning hoeken punten,
            //als dit te groot wordt pakt die de product lengte of hoogte ligt er aan welke kleiner is 
            int accuracy = Math.Min((int)min.Distance(max) / 200, Math.Min(product.Length, product.Width));

            //Maakt een nieuwe thread aan waar de dingen gecontrolleerd worden zodat je live de producten ziet plaatsen
            new Thread(
                () =>
                {
                    //Loopt door alle coordinaten binnen de ruimte boudning box heen met stappen van accuracy
                    for (int y = min.Y + 1; y < max.Y; y += accuracy)
                    {
                        for (int x = min.X + 1; x < max.X; x += accuracy)
                        {
                            Position position = new Position(x, y);

                            //Als het punt binnen de ruimte zit controlleerd die of deze genoeg afstand heeft van alles
                            if (room.Inside(product.GetPoly().Offset(position)))
                            {
                                ProductPlacement placement = new ProductPlacement(position, product, null);
                                bool success = true;
                                //Loopt alle plaatsingen langs om te kijken of die veilig is om te plaatsen
                                for (int i = 0; i < designer.ProductPlacements.Count; i++)
                                {
                                    ProductPlacement place = designer.ProductPlacements[i];
                                    if (place.GetPoly().IsSafe(placement.GetPoly())) continue;

                                    success = false;
                                    break;
                                }

                                //Kijkt of die ver genoeg van de lijn is
                                if (success && designer._route != null && designer._route.Count >= 2)
                                {
                                    (Position p1, Position p2) = placement.GetPoly().MinDistance(designer._route);
                                    if (p1.Distance(p2) < 150) success = false;
                                }

                                //Als dit allemaal klopt voegd die het product toe;
                                if (success)
                                {
                                    designer.ProductPlacements.Add(placement);

                                    designer.AddToOverview(placement.Product);

                                    designer.Editor.Dispatcher.Invoke(
                                        () =>
                                        {
                                            designer.DrawProduct(placement, designer.ProductPlacements.IndexOf(placement));
                                        }
                                    );
                                }
                            }
                        }
                    }

                    designer.OnPropertyChanged();
                }
            ).Start();
        }
    }
}