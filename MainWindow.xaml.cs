using ChessLogic;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DrawingPoint = System.Drawing.Point;
using System.Windows.Documents;
using System.Linq;

namespace ChessUI
{
    public partial class MainWindow : Window
    {
        private Board board;
        private GameState gameState;
        private Image selectedPieceImage = null;
        private DrawingPoint selectedPosition;
        private ChessPiece selectedPiece = null;
        private ChessAI AI;

        public MainWindow()
        {
            InitializeComponent();
            board = new Board();
            board.CreateInitialBoard();
            gameState = new GameState(board);
            board.GameState = gameState;
            AI = new ChessAI(board, gameState);
            InitializeBoardUI();
            ChessBoard.MouseDown += ChessBoard_MouseDown;
        }

        private void InitializeBoardUI()
        {
            // Clear existing pieces
            ChessBoard.Children.Clear();

            // Initialize UI based on actual game board state
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var piece = board.board[x, y];
                    if (piece != null)
                    {
                        PlacePieceUI(piece);
                    }
                }
            }
        }

        private void PlacePieceUI(ChessPiece piece)
        {
            string colourPrefix = piece.Colour == PieceColour.White ? "White" : "Black";
            string pieceType = piece.Type.ToString();
            string imageName = $"{colourPrefix}{pieceType}.png";

            Image image = new Image
            {
                Source = new BitmapImage(new Uri($"Pieces/{imageName}", UriKind.Relative)),
                Stretch = Stretch.Uniform,  // Changed from UniformToFill to Uniform
                VerticalAlignment = VerticalAlignment.Center,  // Center vertically
                HorizontalAlignment = HorizontalAlignment.Center,  // Center horizontally
                Margin = new Thickness(2)  // Small margin to prevent edge touching
            };

            Grid.SetRow(image, piece.Y);
            Grid.SetColumn(image, piece.X);
            ChessBoard.Children.Add(image);
        }

        private DrawingPoint GetChessBoardPosition(Point screenPosition)
        {
            double squareSize = ChessBoard.ActualWidth / 8;
            return new DrawingPoint(
                (int)(screenPosition.X / squareSize),
                (int)(screenPosition.Y / squareSize)
            );
        }

        private void ChessBoard_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gameState == null || gameState.IsGameOver) return;

            DrawingPoint position = GetChessBoardPosition(e.GetPosition(ChessBoard));
            if (selectedPiece != null)
            {
                // If clicking on the same piece (deselect) and then click again, if no game freezes
                if (position.X == selectedPosition.X && position.Y == selectedPosition.Y)
                {

                    if (selectedPieceImage != null)
                    {
                        selectedPieceImage.Opacity = 1.0;
                    }
                    selectedPiece = null;
                    selectedPieceImage = null;
                    return;
                }

                // Check if clicking on another friendly piece (switch selection)
                ChessPiece newPiece = board.GetPieceAt(position);
                if (newPiece != null && newPiece.Colour == gameState.CurrentTurn)
                {
                    // Deselect old piece
                    if (selectedPieceImage != null)
                    {
                        selectedPieceImage.Opacity = 1.0;
                    }

                    // Select new piece
                    SelectPiece(position);
                    return;
                }


                TryMakeMove(position);
            }
            else
            {
                SelectPiece(position);
            }
        }

        private void SelectPiece(DrawingPoint position)
        {
            ChessPiece piece = board.GetPieceAt(position);
            if (piece != null && piece.Colour == gameState.CurrentTurn)
            {
                selectedPiece = piece;
                selectedPosition = position;


                foreach (UIElement element in ChessBoard.Children)
                {
                    if (element is Image img &&
                        Grid.GetRow(img) == position.Y &&
                        Grid.GetColumn(img) == position.X)
                    {
                        selectedPieceImage = img;
                        img.Opacity = 0.7; // make piece transparent
                        break;
                    }
                }
            }
        }

        private void TryMakeMove(DrawingPoint targetPosition)
        {
            bool moveSuccessful = gameState.TryMakeMove(selectedPosition, targetPosition);

            if (selectedPieceImage != null)
            {
                selectedPieceImage.Opacity = 1.0;
            }

            if (moveSuccessful)
            {
                UpdateBoardUI(selectedPosition, targetPosition);
                CheckGameState();

                // Check if pawn reached promotion rank
                ChessPiece piece = board.GetPieceAt(targetPosition);
                if (piece != null && piece.Type == PieceType.Pawn &&
                    ((piece.Colour == PieceColour.White && targetPosition.Y == 0) ||
                     (piece.Colour == PieceColour.Black && targetPosition.Y == 7)))
                {
                    HandlePawnPromotion(targetPosition);
                }


            }


            // If game is not over and it's AI's turn, make AI move
            if (!gameState.IsGameOver && gameState.CurrentTurn == PieceColour.Black)
            {
                try
                {
                    // Make AI move
                    bool aiMoved = AI.MakeMove();

                    if (aiMoved)
                    {
                        // Refresh the board UI to reflect AI's move
                        InitializeBoardUI();
                        CheckGameState();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"AI error: {ex.Message}");
                }

                selectedPiece = null;
                selectedPieceImage = null;
            }

        }

        private void UpdateBoardUI(DrawingPoint from, DrawingPoint to)
        {
            // Remove captured piece images at the target position
            foreach (UIElement element in ChessBoard.Children.OfType<Image>().ToList())
            {
                if (Grid.GetRow(element) == to.Y && Grid.GetColumn(element) == to.X && element != selectedPieceImage)
                {
                    ChessBoard.Children.Remove(element);
                }
            }

            if (selectedPieceImage != null)
            {
                Grid.SetRow(selectedPieceImage, to.Y);
                Grid.SetColumn(selectedPieceImage, to.X);
            }

            if (selectedPiece.Type == PieceType.King && Math.Abs(to.X - from.X) == 2)
            {
                HandleCastlingVisuals(from, to);
            }
        }

        private void HandleCastlingVisuals(DrawingPoint from, DrawingPoint to)
        {
            int direction = to.X > from.X ? 1 : -1;
            int rookFromX = direction == 1 ? 7 : 0;
            int rookToX = direction == 1 ? from.X + 1 : from.X - 1;

            foreach (UIElement element in ChessBoard.Children.OfType<Image>().ToList())
            {
                if (Grid.GetRow(element) == from.Y && Grid.GetColumn(element) == rookFromX)
                {
                    Grid.SetColumn(element, rookToX);
                    break;
                }
            }
        }

        private void CheckGameState()
        {
            PieceColour opponentColour = gameState.CurrentTurn;
            if (gameState.IsInCheckmate(opponentColour))
            {
                PieceColour winner = opponentColour == PieceColour.White ? PieceColour.Black : PieceColour.White;
                ShowGameOverScreen(winner, GameOverReason.Checkmate);
            }
            else if (gameState.IsInCheck(opponentColour))
            {
                MessageBox.Show($"{opponentColour} is in check!");
            }
            else if (gameState.IsInStalemate(opponentColour))
            {
                ShowGameOverScreen(null, GameOverReason.Stalemate);
            }
        }





        private void ShowGameOverScreen(PieceColour? winner, GameOverReason reason)
        {
            string title = "Game Over";
            string message = "";

            switch (reason)
            {
                case GameOverReason.Checkmate:
                    title = $"{winner} Wins!";
                    message = $"Checkmate! {winner} has won the game.";
                    break;
                case GameOverReason.Stalemate:
                    title = "Draw";
                    message = "Stalemate! The game ends in a draw.";
                    break;
                case GameOverReason.Resignation:
                    title = $"{winner} Wins!";
                    message = $"{(winner == PieceColour.White ? "Black" : "White")} has resigned. {winner} wins the game.";
                    break;
            }

            // Update the game over screen text
            GameOverTitle.Text = title;
            GameOverMessage.Text = message;

            // Show the game over screen
            GameOverScreen.Visibility = Visibility.Visible;

            // Set game state to over
            if (!gameState.IsGameOver)
            {
                gameState.EndGameWithWinner(winner ?? PieceColour.White);
            }
        }


        private void PlayAgainButton_Click(object sender, RoutedEventArgs e)
        {
            RestartGame();
            GameOverScreen.Visibility = Visibility.Collapsed;
        }


        private void QuitButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the application
        }


        private void ResignButton_Click(object sender, RoutedEventArgs e)
        {
            if (gameState.IsGameOver) return;

            // The current player resigns, so the opponent wins
            PieceColour winner = gameState.CurrentTurn == PieceColour.White ? PieceColour.Black : PieceColour.White;
            ShowGameOverScreen(winner, GameOverReason.Resignation);
        }

        private void RestartGame()
        {
            board = new Board();
            board.CreateInitialBoard();
            gameState = new GameState(board);
            board.GameState = gameState;
            AI = new ChessAI(board, gameState);
            InitializeBoardUI();
            selectedPiece = null;
            selectedPieceImage = null;
        }

        private ChessPiece CreateNewPiece(PieceType type, PieceColour colour, int x, int y)
        {
            return type switch
            {
                PieceType.Queen => new Queen(PieceType.Queen, colour, x, y),
                PieceType.Rook => new Rook(PieceType.Rook, colour, x, y),
                PieceType.Bishop => new Bishop(PieceType.Bishop, colour, x, y),
                PieceType.Knight => new Knight(PieceType.Knight, colour, x, y),
                _ => new Queen(PieceType.Queen, colour, x, y)
            };
        }

        private void UpdatePieceUI(int x, int y, PieceType type, PieceColour colour)
        {
            foreach (UIElement element in ChessBoard.Children.OfType<Image>().ToList())
            {
                if (Grid.GetColumn(element) == x && Grid.GetRow(element) == y)
                {
                    ChessBoard.Children.Remove(element);
                }
            }

            string colourPrefix = colour == PieceColour.White ? "White" : "Black";
            Image newPiece = new Image
            {
                Source = new BitmapImage(new Uri($"Pieces/{colourPrefix}{type}.png", UriKind.Relative)),
                Stretch = Stretch.Uniform,
                VerticalAlignment = VerticalAlignment.Center,  // Center vertically
                HorizontalAlignment = HorizontalAlignment.Center,  // Center horizontally
                Margin = new Thickness(2)  // Small margin to prevent edge touching
            };
            Grid.SetRow(newPiece, y);
            Grid.SetColumn(newPiece, x);
            ChessBoard.Children.Add(newPiece);
        }

        private void PromotePawnTo(DrawingPoint position, PieceType type, PieceColour colour)
        {
            int x = position.X;
            int y = position.Y;

            board.board[x, y] = CreateNewPiece(type, colour, x, y);

            // Update UI
            UpdatePieceUI(x, y, type, colour);
        }

        private void HandlePawnPromotion(DrawingPoint position)
        {
            ChessPiece pawn = board.GetPieceAt(position);
            if (pawn == null || pawn.Type != PieceType.Pawn) return;

            Window promotionWindow = new Window
            {
                Title = "Promote Pawn",
                Width = 300,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            StackPanel panel = new StackPanel { Orientation = Orientation.Horizontal };

            // Create buttons for each piece type
            foreach (var pieceType in new[] { PieceType.Queen, PieceType.Rook, PieceType.Bishop, PieceType.Knight })
            {
                Button button = new Button
                {
                    Content = pieceType.ToString(),
                    Width = 70,
                    Height = 50,
                    Margin = new Thickness(5)
                };

                button.Click += (sender, e) =>
                {
                    PromotePawnTo(position, pieceType, pawn.Colour);
                    promotionWindow.Close();
                };

                panel.Children.Add(button);
            }

            promotionWindow.Content = panel;
            promotionWindow.ShowDialog();
        }
    }
}