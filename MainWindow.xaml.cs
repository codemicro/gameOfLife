using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
 {
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow
     {
         private readonly Cell[,] cells = new Cell[46,46];

         public MainWindow()
         {
             InitializeComponent();
             
             window.ResizeMode = ResizeMode.NoResize;  // prevent the window being resized
             
             // Draw cells

             for (var x = 0; x < 46; x++)
             {
                 for (var y = 0; y < 46; y++)
                 {

                     cells[x, y] = new Cell();
                     cells[x, y].Rect = new Rectangle()
                     {
                        Fill = Brushes.White,
                        Height = 8,
                        Width = 8,
                        StrokeThickness = 0
                     };

                     gameCanvas.Children.Add(cells[x, y].Rect);
                     Canvas.SetTop(cells[x, y].Rect, y*10);
                     Canvas.SetLeft(cells[x, y].Rect, x*10);
                     
                 }
             }
             
             // Draw grid
             
             for (var i = 1; i < 46; i++)
             {
                 Line line = new Line()
                 {
                     StrokeThickness = 1,
                     Stroke = Brushes.Black,
                     X1 = 10*i - 1,
                     X2 = 10*i - 1,
                     Y1 = 0,
                     Y2 = 459
                 };
                 
                 gameCanvas.Children.Add(line);
                 
                 line = new Line()
                 {
                     StrokeThickness = 1,
                     Stroke = Brushes.Black,
                     X1 = 0,
                     X2 = 459,
                     Y1 = 10*i - 1,
                     Y2 = 10*i - 1
                 };

                 gameCanvas.Children.Add(line);
             }
             
             cells[15, 15].SetAlive();
             cells[16, 15].SetAlive();
             cells[17, 15].SetAlive();
         }
     }

     public class Cell
     {
         public bool IsAlive = false;
         public Rectangle Rect { get; set; }
         
         public void SetAlive()
         {
             IsAlive = true;
             Rect.Fill = Brushes.Black;
         }

         public void SetDead()
         {
             IsAlive = false;
             Rect.Fill = Brushes.White;
         }
     }
 }