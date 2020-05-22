using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Win32;

namespace GameOfLife
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        
        private readonly BackgroundWorker worker = new BackgroundWorker();  // used for running game logic
        private readonly DispatcherTimer Timer = new DispatcherTimer();
        private readonly Cell[,] Cells = new Cell[46, 46];
        private readonly bool[,] NextIterStates = new bool[46, 46];
        
        private bool[,] States = new bool[46, 46];
        private readonly bool[,] StockCells = new bool[46, 46];  // when the game resets, it restores to this array
        
        public bool IsRunning = false;
        public int CurrentIteration = 0;

        public MainWindow()
        {

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
                for (var y = 0; y < 46; y++)
                {
                    for (var x = 0; x < 46; x++)
                    {
                        // Calculate number of neighbours

                        var numNeighbours = 0;
                        
                        var positions = new int[8, 2]
                        {
                            { 0, 1 },
                            { 1, 1 },
                            { 1, 0 },
                            { 1, -1 },
                            { 0, -1 },
                            { -1, -1 },
                            { -1, 0 },
                            { -1, 1 }
                        };
                        
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

                        // Make decisions about the fate of each cell

                        if (States[x, y] && (numNeighbours == 2 || numNeighbours == 3))
                        {
                            NextIterStates[x, y] = true;
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

            }

            void UpdateUi(object sender, RunWorkerCompletedEventArgs e)
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

                IncrementIteration();
            }
            
            InitialiseCells();
            
            // Add functions to worker to be run asynchronously
            worker.DoWork += CalculateNextIteration;
            worker.RunWorkerCompleted += UpdateUi;

            void tickAction(object sender, object e)
            {
                try
                {
                    worker.RunWorkerAsync();
                } catch (InvalidOperationException) { }  // will be thrown if a new tick tries to occur when the previous has not finished, in that case, miss the tick
                
            }
            
            /*var timer = new System.Threading.Timer(
                e => worker.RunWorkerAsync(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));*/
            
            // https://www.pmichaels.net/2015/11/27/using-a-timer-to-update-the-ui-thread/

            tickSpeedSlider.Value = 250;
            
            Timer.Interval = TimeSpan.FromMilliseconds(250);
            Timer.Tick += tickAction;

        }
        
        // Various functions

        private void IncrementIteration()
        {
            CurrentIteration++;
            iterationTextBlock.Text = "Iteration: " + CurrentIteration;
        }

        private void ResetIteration()
        {
            CurrentIteration = 0;
            iterationTextBlock.Text = "Iteration: " + CurrentIteration;
        }
        
        private void InitialiseCells()
        {
            // TODO: Better functionality to set initial state
            
            // Create a blinker
            SetCellAlive(30, 5);
            SetCellAlive(31, 5);
            SetCellAlive(32, 5);
            
            // Create a glider
            SetCellAlive(0, 2);
            SetCellAlive(1, 3);
            SetCellAlive(2, 3);
            SetCellAlive(2, 2);
            SetCellAlive(2, 1);
            
            Array.Copy(States, StockCells, States.Length);
        }
        
        private void SetCellAlive(int x, int y)
        {
            States[x, y] = true;
            Cells[x, y].SetAlive();
        }

        private void SetCellDead(int x, int y)
        {
            States[x, y] = false;
            Cells[x, y].SetDead();
        }
        
        // Classes
        
        private class Cell
        {
            // Represents a cell in the master array
            
            public Rectangle Rect { get; set; }

            public void SetAlive()
            {
                Rect.Fill = Brushes.Black;
            }

            public void SetDead()
            {
                Rect.Fill = Brushes.White;
            }
        }
        
        
        // Functions triggered by UI interaction

        private void Start_button(object sender, RoutedEventArgs e)
        {
            Timer.Start();
            IsRunning = true;
            statusTextBlock.Text = "Currently: Running";
        }

        private void Stop_button(object sender, RoutedEventArgs e)
        {
            Timer.Stop();
            IsRunning = false;
            statusTextBlock.Text = "Currently: Not running";
        }

        private void Reset_button(object sender, RoutedEventArgs e)
        {
            for (var x = 0; x < 45; x++)
            {
                for (var y = 0; y < 45; y++)
                {
                    if (StockCells[x, y] == States[x, y]) { }
                    else if (StockCells[x, y])
                    {
                        SetCellAlive(x, y);
                    }
                    else
                    {
                        SetCellDead(x, y);
                    }
                }
            }
            
            Array.Copy(StockCells, States, States.Length);

            ResetIteration();
        }

        private void LoadFromFile_button(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt";

            if (openFileDialog.ShowDialog() == true)
            {

                Timer.Stop();
                IsRunning = false;
                statusTextBlock.Text = "Currently: Not running";
                
                var newArray = new bool[46, 46];
                var filetext = File.ReadAllText(openFileDialog.FileName).Split('\n');

                if (filetext.Length != 46)
                {
                    MessageBox.Show("Incorrect number of lines! Must be 46");
                    return;
                }
                
                for (var lineNumber = 0; lineNumber < filetext.Length; lineNumber++)
                {
                    
                    if (filetext[lineNumber].Trim().Length != 46)
                    {
                        MessageBox.Show("Incorrect number of characters on line " + (lineNumber + 1) + "! Must be 46");
                        return;
                    }
                    
                    for (var charNumber = 0; charNumber < filetext[lineNumber].Trim().Length; charNumber++)
                    {
                        var c = filetext[lineNumber].Trim()[charNumber];
                        
                        bool val;
                        if (c == '0')
                        {
                            val = false;
                        } 
                        else if (c == '1')
                        {
                            val = true;
                        }
                        else
                        {
                            MessageBox.Show("Unknown character '" + c + "'" + " at " + lineNumber + "," + charNumber);
                            return;
                        }

                        newArray[charNumber, lineNumber] = val;

                    }
                }
                
                Array.Copy(newArray, States, States.Length);
                Array.Copy(newArray, StockCells, StockCells.Length);
                
                for (var x = 0; x < 45; x++)
                {
                    for (var y = 0; y < 45; y++)
                    {
                        if (newArray[x, y])
                        {
                            SetCellAlive(x, y);
                        }
                        else
                        {
                            SetCellDead(x, y);
                        }
                    }
                }

                ResetIteration();
                loadedFileTextBlock.Text = "Loaded file: " + openFileDialog.FileName;

            }
        }

        private void TickSpeedChanged_slider(object sender, RoutedEventArgs e)
        {
            Timer.Interval = TimeSpan.FromMilliseconds(tickSpeedSlider.Value);
        }
    }
}