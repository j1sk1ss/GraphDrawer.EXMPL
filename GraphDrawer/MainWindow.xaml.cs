using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using ExpressionConverter = GraphDrawer.Functions.ExpressionConverter;

namespace GraphDrawer {
    public partial class MainWindow {
        public MainWindow() {
            InitializeComponent();
        }
        
        private string _equation = " ";
        
        private void UserPressButton(object sender, RoutedEventArgs e) {
            var symbol = (sender as Button)!.Content.ToString();
            _equation += (symbol == "ПРОБЕЛ" ? " " : symbol);
            UserEquation.Content = _equation;
        }

        private void DeleteSymbol(object sender, RoutedEventArgs e) {
            if (_equation.Length == 0) return;
            
            _equation = _equation[..^1];
            UserEquation.Content = _equation;
        }

        [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
        private void ResolveEquation(object sender, RoutedEventArgs e) {
            try {
                GraphBody.Children.Clear();
            
                var prevX = .1d;
                var prevY = ExpressionConverter.ConvertString(_equation.Replace("x", $"{prevX}"));
                for (var i = .1d; i < 15; i += .1d) {
                    var answer = ExpressionConverter.ConvertString(_equation.Replace("x", $"{i}"));

                    GraphBody.Children.Add(new Line {
                        X1 = prevX * 10,
                        Y1 = -prevY * 10,
                        X2 = i * 10,
                        Y2 = -answer * 10,
                        Stroke = Brushes.Black
                    });
                
                    prevX = i;
                    prevY = answer;
                }
            }
            catch (Exception exception) {
                MessageBox.Show($"{exception}");
                throw;
            }
        }

        private void ExportImage(object sender, RoutedEventArgs e) {
            try {
                var bounds = VisualTreeHelper.GetDescendantBounds(Graph);
                const double dpi = 96d;
                var rtb = new RenderTargetBitmap((int)bounds.Width, (int)bounds.Height, dpi, dpi, PixelFormats.Default);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen()) {
                    var vb = new VisualBrush(Graph);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                var file = new SaveFileDialog();
                if (file.ShowDialog() != true) return;
                
                var png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                using (Stream stm = File.Create(file.FileName)) {
                    png.Save(stm);
                }
            }
            catch (Exception exception) {
                MessageBox.Show($"{exception}");
                throw;
            }
        }
    }
}