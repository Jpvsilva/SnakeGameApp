using SnakeGameApp.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnakeGameApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Random rnd;
        private bool wallCollisions = true;

        const int SnakeSquareSize = 20;
        const int SnakeStartLength = 3;
        const int SnakeStartSpeed = 400;
        const int SnakeSpeedThreshold = 100;

        private int snakeLength;

        private UIElement snakeFood = null;
        private SolidColorBrush foodBrush = Brushes.Red;

        private readonly SolidColorBrush snakeBodyBrush = Brushes.Green;
        private readonly SolidColorBrush snakeHeadBrush = Brushes.YellowGreen;
        private readonly List<SnakePiece> snakePieces = new List<SnakePiece>();

        public enum SnakeDirection { Left, Right, Up, Down };
        private SnakeDirection snakeDirection = SnakeDirection.Right;

        private System.Windows.Threading.DispatcherTimer gameTickTimer = new System.Windows.Threading.DispatcherTimer();

        public int CurrentScore { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            rnd = new Random();
            gameTickTimer.Tick += GameTickTimer_Tick;
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            DrawGameArea();
            StartNewGame();
        }

        private void GameTickTimer_Tick(object sender, EventArgs e)
        {
            MoveSnake();
        }

        private void DrawGameArea()
        {
            bool doneDrawingBackground = false;
            int nextX = 0, nextY = 0;
            int rowCounter = 0;

            while (doneDrawingBackground == false)
            {
                Rectangle rect = new Rectangle
                {
                    Width = SnakeSquareSize,
                    Height = SnakeSquareSize,
                    Fill = Brushes.Gray
                };
                Board.Children.Add(rect);
                Canvas.SetTop(rect, nextY);
                Canvas.SetLeft(rect, nextX);

                nextX += SnakeSquareSize;
                if (nextX >= Board.ActualWidth)
                {
                    nextX = 0;
                    nextY += SnakeSquareSize;
                    rowCounter++;
                }

                if (nextY >= Board.ActualHeight)
                    doneDrawingBackground = true;
            }
        }

        private void DrawSnake()
        {
            foreach (SnakePiece snakePart in snakePieces)
            {
                if (snakePart.UiElement == null)
                {
                    snakePart.UiElement = new Rectangle()
                    {
                        Width = SnakeSquareSize,
                        Height = SnakeSquareSize,
                        Fill = (snakePart.IsHead ? snakeHeadBrush : snakeBodyBrush)
                    };
                    Board.Children.Add(snakePart.UiElement);
                    Canvas.SetTop(snakePart.UiElement, snakePart.Position.Y);
                    Canvas.SetLeft(snakePart.UiElement, snakePart.Position.X);
                }
            }
        }

        private void DrawSnakeFood()
        {
            Point foodPosition = GetNextFoodPosition();
            snakeFood = new Ellipse()
            {
                Width = SnakeSquareSize,
                Height = SnakeSquareSize,
                Fill = foodBrush
            };
            Board.Children.Add(snakeFood);
            Canvas.SetTop(snakeFood, foodPosition.Y);
            Canvas.SetLeft(snakeFood, foodPosition.X);
        }

        private void StartNewGame()
        {
            scoreLabel.Content = ("Score: 0").ToString();
            snakeLength = SnakeStartLength;
            snakeDirection = SnakeDirection.Right;
            snakePieces.Add(new SnakePiece() { Position = new Point(SnakeSquareSize * 5, SnakeSquareSize * 5) });
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(SnakeStartSpeed);

            // Draw the snake  
            DrawSnake();
            // Draw the snake food
            DrawSnakeFood();

            // Enable timed movement          
            gameTickTimer.IsEnabled = true;
        }

        private void MoveSnake()
        {
            // Remove the last part of the snake, in preparation of the new part added below  
            while (snakePieces.Count >= snakeLength)
            {
                Board.Children.Remove(snakePieces[0].UiElement);
                snakePieces.RemoveAt(0);
            }
            // Next up, we'll add a new element to the snake, which will be the (new) head  
            foreach (SnakePiece snakePart in snakePieces)
            {
                // Therefore, we mark all existing parts as non-head (body) elements and then  
                // we make sure that they use the body brush  
                (snakePart.UiElement as Rectangle).Fill = snakeBodyBrush;
                snakePart.IsHead = false;
            }

            // Determine in which direction to expand the snake, based on the current direction  
            SnakePiece snakeHead = snakePieces[snakePieces.Count - 1];
            double nextX = snakeHead.Position.X;
            double nextY = snakeHead.Position.Y;
            switch (snakeDirection)
            {
                case SnakeDirection.Left:
                    nextX -= SnakeSquareSize;
                    break;
                case SnakeDirection.Right:
                    nextX += SnakeSquareSize;
                    break;
                case SnakeDirection.Up:
                    nextY -= SnakeSquareSize;
                    break;
                case SnakeDirection.Down:
                    nextY += SnakeSquareSize;
                    break;
            }

            // Now add the new head part to our list of snake parts...  
            snakePieces.Add(new SnakePiece()
            {
                Position = new Point(nextX, nextY),
                IsHead = true
            });
            // And then have it drawn!  
            DrawSnake();

            DoCollisionCheck();          
        }

        private Point GetNextFoodPosition()
        {
            int maxX = (int)(Board.ActualWidth / SnakeSquareSize);
            int maxY = (int)(Board.ActualHeight / SnakeSquareSize);
            int foodX = rnd.Next(0, maxX) * SnakeSquareSize;
            int foodY = rnd.Next(0, maxY) * SnakeSquareSize;

            foreach (SnakePiece snakePart in snakePieces)
            {
                if ((snakePart.Position.X == foodX) && (snakePart.Position.Y == foodY))
                    return GetNextFoodPosition();
            }

            return new Point(foodX, foodY);

        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            SnakeDirection originalSnakeDirection = snakeDirection;
            switch (e.Key)
            {
                case Key.Up:
                    if (snakeDirection != SnakeDirection.Down)
                        snakeDirection = SnakeDirection.Up;
                    break;
                case Key.Down:
                    if (snakeDirection != SnakeDirection.Up)
                        snakeDirection = SnakeDirection.Down;
                    break;
                case Key.Left:
                    if (snakeDirection != SnakeDirection.Right)
                        snakeDirection = SnakeDirection.Left;
                    break;
                case Key.Right:
                    if (snakeDirection != SnakeDirection.Left)
                        snakeDirection = SnakeDirection.Right;
                    break;
                case Key.Space:
                    StartNewGame();
                    break;
            }
            if (snakeDirection != originalSnakeDirection)
                MoveSnake();
        }

        private void EatSnakeFood()
        {
            snakeLength++;
            CurrentScore++;
            int timerInterval = Math.Max(SnakeSpeedThreshold, (int)gameTickTimer.Interval.TotalMilliseconds - (CurrentScore * 2));
            gameTickTimer.Interval = TimeSpan.FromMilliseconds(timerInterval);
            Board.Children.Remove(snakeFood);
            DrawSnakeFood();
            UpdateGameStatus();
        }

        private void UpdateGameStatus()
        {
             scoreLabel.Content = ("Score: " + CurrentScore + " - Game speed: " + gameTickTimer.Interval.TotalMilliseconds).ToString();
        }

        private void EndGame()
        {
            gameTickTimer.IsEnabled = false;
            MessageBox.Show("Oooops, you died!\n\nTo start a new game, just press the Space bar...", "SnakeGameApp");
        }

        private void DoCollisionCheck()
        {
            SnakePiece snakeHead = snakePieces[snakePieces.Count - 1];

            if ((snakeHead.Position.X == Canvas.GetLeft(snakeFood)) && (snakeHead.Position.Y == Canvas.GetTop(snakeFood)))
            {
                EatSnakeFood();
                return;
            }

            if(wallCollisions == true)
            {
                if ((snakeHead.Position.Y < 0) || (snakeHead.Position.Y >= Board.ActualHeight) ||
                    (snakeHead.Position.X < 0) || (snakeHead.Position.X >= Board.ActualWidth))
                {
                    EndGame();
                }
            }
            //else
            //{
                //PassToOtherSide();
            //}           

            foreach (SnakePiece snakeBodyPart in snakePieces.Take(snakePieces.Count - 1))
            {
                if ((snakeHead.Position.X == snakeBodyPart.Position.X) && (snakeHead.Position.Y == snakeBodyPart.Position.Y))
                    EndGame();
            }
        }
        /*
        private void PassToOtherSide()
        {
            SnakePiece snakeHead = snakePieces[snakePieces.Count - 1];

            if (snakeHead.Position.X >= Board.ActualWidth)
                snakeHead.Position.X = 0;
            else if (snakeHead.Position.X < 0)
                snakeHead.Position.X = Board.ActualWidth - 1;

            if (snakeHead.Position.Y >= Board.ActualHeight) 
                snakeHead.Position.Y = 0;
            else if (snakeHead.Position.Y < 0) 
                snakeHead.Position.Y = Board.ActualHeight - 1;
        }*/
    }
}
