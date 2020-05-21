using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        private readonly BackgroundWorker worker = new BackgroundWorker();  // used for game logic
        private readonly Cell[,] Cells = new Cell[46, 46];
        private readonly bool[,] States = new bool[46, 46];
        private readonly bool[,] NextIterStates = new bool[46, 46];

        public MainWindow()
        {
            void SetCellAlive(int x, int y)
            {
                States[x, y] = true;
                Cells[x, y].SetAlive();
            }

            void SetCellDead(int x, int y)
            {
                States[x, y] = false;
                Cells[x, y].SetDead();
            }

            InitializeComponent();

            window.ResizeMode = ResizeMode.NoResize; // prevent the window being resized

            // Draw cells

            for (var x = 0; x < 46; x++)
            {
                for (var y = 0; y < 46; y++)
                {

                    Cells[x, y] = new Cell();
                    Cells[x, y].Rect = new Rectangle()
                    {
                        Fill = Brushes.White,
                        Height = 8,
                        Width = 8,
                        StrokeThickness = 0
                    };

                    gameCanvas.Children.Add(Cells[x, y].Rect);
                    Canvas.SetTop(Cells[x, y].Rect, y * 10);
                    Canvas.SetLeft(Cells[x, y].Rect, x * 10);

                }
            }

            // Draw grid

            for (var i = 1; i < 46; i++)
            {
                Line line = new Line()
                {
                    StrokeThickness = 1,
                    Stroke = Brushes.Black,
                    X1 = 10 * i - 1,
                    X2 = 10 * i - 1,
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
                    Y1 = 10 * i - 1,
                    Y2 = 10 * i - 1
                };

                gameCanvas.Children.Add(line);
            }

            void CalculateNextIteration(object sender, DoWorkEventArgs e)
            {
                // Copy array
                Array.Copy(States, NextIterStates, States.Length);

                // Iter over every cell
                for (var x = 0; x < 46; x++)
                {
                    for (var y = 0; y < 46; y++)
                    {
                        // Calculate number of neighbours
                        var positions = new int[8, 2]
                        {
                            { 0, 1 },
                            { 1, 1 },
                            { 1, 0 },
                            { 1, -1 },
                            { 0, -1 },
                            { -1, -1 },
                            { -1, 0 },
                            { -1, -1 }
                        };

                        var numNeighbours = 0;
                        
                        for (var i = 0; i < positions.GetLength(0); i++)
                        {
                            var testXPos = x + positions[i, 0];
                            var testYPos = y + positions[i, 1];

                            if (!(testXPos < 0 || testXPos >= 46) && !(testYPos < 0 || testYPos >= 46))
                            {
                                if (States[testXPos, testYPos])
                                {
                                    numNeighbours++;
                                }
                            }
                        }

                        // Make decisions about if the cell should live or die

                        if (States[x, y] && numNeighbours < 2)
                        {
                            NextIterStates[x, y] = false;
                        }
                        else if (!States[x, y] && numNeighbours == 3)
                        {
                            NextIterStates[x, y] = true;
                        }
                        else
                        {
                            NextIterStates[x, y] = false;
                        }
                    }
                }

                // Modify cell state

            }

            void UpdateUI(object sender, RunWorkerCompletedEventArgs e)
            {
                for (var x = 0; x < 45; x++)
                {
                    for (var y = 0; y < 45; y++)
                    {
                        if (States[x, y] == NextIterStates[x, y]) { }
                        else if (NextIterStates[x, y])
                        {
                            SetCellAlive(x, y);
                        }
                        else
                        {
                            SetCellDead(x, y);
                        }
                    }
                }
            }


            // TODO: Set inital state
            SetCellAlive(15, 15);
            SetCellAlive(16, 15);
            SetCellAlive(17, 15);

            // Add functions to worker to be run asynchronously
            worker.DoWork += CalculateNextIteration;
            worker.RunWorkerCompleted += UpdateUI;

            void tickAction(object sender, object e)
            {
                worker.RunWorkerAsync();
            }
            
            /*var timer = new System.Threading.Timer(
                e => worker.RunWorkerAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));*/
            
            // https://www.pmichaels.net/2015/11/27/using-a-timer-to-update-the-ui-thread/
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += tickAction;
            timer.Start();
            
        }

        private class Cell
        {
            private bool IsAlive = false;
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
}