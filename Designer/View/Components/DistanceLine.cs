using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Models;
using Line = System.Windows.Shapes.Line;

namespace Designer.View.Components {
    public class DistanceLine {
        private Line _line;
        private Line _line2;
        private TextBlock _textBlock;
        private string _prefix;
        private Position _p1;
        private Position _p2;

        public Position P1 {
            get => _p1;
            set {
                _p1 = value;
                UpdatePositions();
            }
        }

        public Position P2 {
            get => _p2;
            set {
                _p2 = value;
                UpdatePositions();
            }
        }

        public bool Shows { get; private set; }

        public DistanceLine(Position p1, Position p2, string prefix = "") {
            _p1 = p1;
            _p2 = p2;
            _line = new Line();
            _line2 = new Line();
            _textBlock = new TextBlock();
            _prefix = prefix;


            _line.Stroke = Brushes.White;
            _line.StrokeThickness = 3;
            _line2.Stroke = Brushes.Black;
            _line2.StrokeThickness = 1;
            _textBlock.Foreground = new SolidColorBrush(Colors.Black);
            _textBlock.Background = new SolidColorBrush(Colors.White);
        }

        /**
         * Renders the line on the canvas
         */
        public void Add(Canvas editor) {
            Shows = true;
            editor.Children.Add(_line);
            editor.Children.Add(_line2);
            editor.Children.Add(_textBlock);
            Render();
        }

        /**
         * Set stuff that needs the canvas to be used
         */
        private void Render() {
            Panel.SetZIndex(_line, 100);
            Panel.SetZIndex(_line2, 101);
            Panel.SetZIndex(_textBlock, 102);

            UpdatePositions();
        }

        /**
         * Updates the lines so that they reflect P1 and P2
         */
        private void UpdatePositions() {
            if (P1 == null || P2 == null) return;
            _line.X1 = P1.X;
            _line.Y1 = P1.Y;
            _line.X2 = P2.X;
            _line.Y2 = P2.Y;

            _line2.X1 = P1.X;
            _line2.Y1 = P1.Y;
            _line2.X2 = P2.X;
            _line2.Y2 = P2.Y;

            Position center = P1.Center(P2);
            _textBlock.Text = _prefix + FormatText(P1.Distance(P2));
            Size size = MeasureString();

            double dx = size.Width / 2;

            //Calculate the rotation by use of atan2
            double radians = Math.Atan2(P2.Y - P1.Y, P2.X - P1.X);
            double degrees = ConvertRadiansToDegrees(radians);
            _textBlock.RenderTransform = new RotateTransform(degrees, dx, 0);

            Canvas.SetLeft(_textBlock, center.X - dx);
            Canvas.SetTop(_textBlock, center.Y - 0);
        }

        /**
         * Measaures the length of the text, this is needed to properly rotate the text block
         */
        private Size MeasureString() {
            var formattedText = new FormattedText(
                _textBlock.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(
                    _textBlock.FontFamily, _textBlock.FontStyle, _textBlock.FontWeight, _textBlock.FontStretch
                ),
                _textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                1
            );

            return new Size(formattedText.Width, formattedText.Height);
        }

        /**
         * Generate the text is the correct format
         */
        private static string FormatText(double distance) {
            if (distance < 100) {
                return distance.ToString("F0") + " cm";
            }

            return (distance / 100).ToString("F2") + " m";
        }

        /**
         * Removes the lines and text from the canvas
         */
        public void Remove(Canvas editor) {
            Shows = false;
            editor.Children.Remove(_line);
            editor.Children.Remove(_line2);
            editor.Children.Remove(_textBlock);
        }

        /**
         * Converts radians to degrees
         */
        private static double ConvertRadiansToDegrees(double radians) {
            double degrees = 180 / Math.PI * radians;
            if (degrees > 90) return degrees + 180;
            if (degrees < -90) return degrees + 180;
            return degrees;
        }
    }
}