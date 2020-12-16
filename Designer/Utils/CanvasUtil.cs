﻿using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;
using Models;
using System.Windows;
using System.Windows.Media.Imaging;

namespace Designer.Utils
{
    public static class CanvasUtil
    {
        public static Canvas CreateRoomCanvas(Room room)
        {
            int maxX = 0;
            int maxY = 0;
            Canvas canvas = new Canvas();
            canvas.Background = Brushes.White;
            List<Position> coordinates = Room.ToList(room.Positions);

            PointCollection points = new PointCollection();
            for (int i = 0; i < coordinates.Count; i++)
            {
                maxX = coordinates[i].X > maxX ? coordinates[i].X : maxX;
                maxY = coordinates[i].Y > maxY ? coordinates[i].Y : maxY;
                points.Add(new Point(coordinates[i].X, coordinates[i].Y));
            }
            
            System.Windows.Shapes.Polygon roomPolygon = new System.Windows.Shapes.Polygon
            {
                Stroke = Brushes.Black,
                Fill = Brushes.LightGray,
                StrokeThickness = 1,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Points = points
            };

            // Zet de dimensies van de ruimte polygon
            roomPolygon.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            canvas.Children.Add(roomPolygon);
            var scaleX = 270.00 / maxX;
            var scaleY = 150.00 / maxY;
            double scale = scaleX < scaleY ? scaleX : scaleY;
            canvas.RenderTransform = new ScaleTransform(scale, scale);

            return canvas;
        }

        public static Canvas FillCanvas(Design design, Canvas roomCanvas)
        {
            foreach (var placement in design.ProductPlacements)
            {
                DrawProduct(roomCanvas, placement);
            }
            return roomCanvas;
        }

        public static void DrawProduct(Canvas canvas, ProductPlacement placement)
        {
            //Haal de bestandsnaam van de foto op of gebruik de default
            Product product = placement.Product;
            int x = placement.X;
            int y = placement.Y;

            var photo = product.Photo ?? "placeholder.png";
            var actualWidth = placement.Rotation % 180 == 0 ? product.Width : product.Length;
            var actualLength = placement.Rotation % 180 == 0 ? product.Length : product.Width;
            // Veranderd de rotatie van het product
            TransformedBitmap tempBitmap = new TransformedBitmap();

            tempBitmap.BeginInit();
            var source = new BitmapImage(new Uri(Environment.CurrentDirectory + $"/Resources/Images/{photo}"));
            tempBitmap.Source = source;
            RotateTransform transform = new RotateTransform(placement.Rotation, source.Width / 2, source.Height / 2);
            tempBitmap.Transform = transform;
            tempBitmap.EndInit();

            var image = new Image {
                Source = tempBitmap,
                Height = actualLength,
                Width = actualWidth
            };

            Canvas.SetTop(image, y);
            Canvas.SetLeft(image, x);
            // Voeg product toe aan canvas
            canvas.Children.Add(image);
        }
    }
}