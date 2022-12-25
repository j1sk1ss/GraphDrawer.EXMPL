using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            _equation += (symbol == "ПРОБЕЛ" ? " " : symbol) + " ";
            UserEquation.Content = _equation;
        }

        private void DeleteSymbol(object sender, RoutedEventArgs e) {
            if (_equation.Length == 0) return;
            
            _equation = _equation[..^1];
            while (_equation[^1] != ' ') {
                _equation = _equation[..^1];
            }
            
            UserEquation.Content = _equation;
        }

        [SuppressMessage("ReSharper.DPA", "DPA0000: DPA issues")]
        private void ResolveEquation(object sender, RoutedEventArgs e) {
            Values.Content =
                "-15 -14 -13 -12 -11 -10 -9 -8 -7 -6 -5 -4 -3 -2 -1 0  1  2  3  4  5  6  7  8  9  10  11  12" +
                "13  14  15";
            try {
                GraphBody.Children.Clear();
                var prevX = -15d;
                var prevY = ExpressionConverter.ConvertString(_equation.Replace("x", $"{prevX}"));
                for (var i = -15d; i < 15d; i += .1d) {
                    var answer = ExpressionConverter.ConvertString(_equation.Replace("x", $"{i}"))
                         >= 15 ? 15 : ExpressionConverter.ConvertString(_equation.Replace("x", $"{i}"));

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
                MessageBox.Show($"{exception}", "Ошибка записи");
            }
        }

        private void ExportImage(object sender, RoutedEventArgs e) {
            try {
                var bounds = VisualTreeHelper.GetDescendantBounds(Graph);

                const double dpi = 96d;
                var rtb = new RenderTargetBitmap(300, 300, 
                    dpi, dpi, PixelFormats.Default);

                var dv = new DrawingVisual();
                using (var dc = dv.RenderOpen()) {
                    var vb = new VisualBrush(Graph);
                    dc.DrawRectangle(vb, null, new Rect(new Point(), bounds.Size));
                }

                rtb.Render(dv);

                var file = new SaveFileDialog {
                    Filter = "PNG files | *.png"
                };
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

        private readonly Dictionary<string, string> _operations = new() {
            {"multiply", "*"},
            {"divide","/"},
            {"subtract","-"},
            {"add","+"}
        };

        private void UserType(object sender, KeyEventArgs e) {
            var temp = e.Key.ToString().ToLower();
            
            if (temp == "back") {
                DeleteSymbol(null, null);
                return;
            }

            if (_operations.ContainsKey(temp)) temp = _operations[temp];
            
            if (temp.Length > 1) {
                temp = temp[1..];
            }
            
            _equation += temp == "space" ? " " : temp;
            UserEquation.Content = _equation;
        }
    }
}
