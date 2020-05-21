﻿using System;
using System.ComponentModel;
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
        private readonly Cell[,] Cells = new Cell[46, 46];
        private readonly bool[,] States = new bool[46, 46];

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

            void Update()
            {
                // Copy array
                var nextIterStates = new bool[46, 46];
                Array.Copy(States, nextIterStates, States.Length);

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
                            nextIterStates[x, y] = false;
                        }
                        else if (!States[x, y] && numNeighbours == 3)
                        {
                            nextIterStates[x, y] = true;
                        }
                        else
                        {
                            nextIterStates[x, y] = false;
                        }
                    }
                }

                // Modify cell state
                
                for (var x = 0; x < 45; x++)
                {
                    for (var y = 0; y < 45; y++)
                    {
                        if (States[x, y] != nextIterStates[x, y])
                        {
                            
                            Action action;
                            
                            if (nextIterStates[x, y])
                            {
                                action = delegate() { SetCellAlive(x, y); };
                            }
                            else
                            {
                                action = delegate { SetCellDead(x, y); };
                            }
                            
                            Application.Current.Dispatcher.BeginInvoke(action);
                        }
                    }
                }
                
            }


            // TODO: Set inital state
            SetCellAlive(15, 15);
            SetCellAlive(16, 15);
            SetCellAlive(17, 15);

            var timer = new System.Threading.Timer(
                e => Update(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(5));
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
}