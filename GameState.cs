using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ChessLogic;
using static ChessLogic.Pawn;

namespace ChessLogic
{
    public class GameState
    {
        private Board board;
        private PieceColour currentTurn;
        private bool gameOver;
        private GameResult result;
        private Dictionary<string, bool> castlingRights = new Dictionary<string, bool>
        // Dictionary to track available castling options for both players
        {
            {"WhiteKingside", true},
            {"WhiteQueenside", true},
            {"BlackKingside", true},
            {"BlackQueenside", true}
        };
        public Dictionary<string, bool> CastlingRights => castlingRights;

        // Add LastMovedPiece property
        public ChessPiece LastMovedPiece { get; private set; }

        public GameState(Board board)
        {
            this.board = board;
            currentTurn = PieceColour.White; // White starts first
            gameOver = false;
            result = GameResult.InProgress;
        }

        public PieceColour CurrentTurn
        {
            get { return currentTurn; }
        }

        public bool IsGameOver
        {
            get { return gameOver; }
        }

        public GameResult Result
        {
            get { return result; }
        }
        public void EndGameWithWinner(PieceColour winner)
        {
            gameOver = true;// checks for game ending conditions
            result = winner == PieceColour.White ? GameResult.WhiteWins : GameResult.BlackWins;
        }

        // Changes the turn after a move is made
        public void ChangeTurn()
        {
            currentTurn = (currentTurn == PieceColour.White) ? PieceColour.Black : PieceColour.White;
            UpdateGameState();
        }

        // Try to make a move and update game state
        public bool TryMakeMove(Point from, Point to)
        {
            ChessPiece piece = board.GetPieceAt(from);

            if (piece == null)
            {
                return false;
            }


            if (piece.Colour != currentTurn)
            {
                return false;
            }


            bool isCastling = piece.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;

            // Validate castling move conditions
            if (isCastling)
            {
                if (!CanCastle(from, to)) return false;
            }
            else if (!piece.IsValidMove(board, from, to))
            {
                return false;
            }

            // Check for en passant capture
            if (piece.Type == PieceType.Pawn)
            {
                // Check for en passant capture
                if (from.X != to.X && board.IsEmptySquare(to))
                {
                    // This is an en passant capture
                    Point capturedPawnPosition = new Point(to.X, from.Y);
                    board.board[capturedPawnPosition.X, capturedPawnPosition.Y] = null;
                }
            }

            // Temporarily store the captured piece (if any)
            ChessPiece capturedPiece = board.GetPieceAt(to);
            Point capturedPosition = new Point(to.X, to.Y);

            // Make the move
            bool moveMade = board.MovePiece(from, to);

            if (!moveMade)
            {
                return false;
            }

            // Set the LastMovedPiece property
            LastMovedPiece = piece;


            // Check if the move leaves or puts the king in check
            if (IsInCheck(currentTurn))
            {
                // Undo the move
                UndoMove(from, to, capturedPiece, capturedPosition, isCastling);
                return false;
            }

            // Move was successful and legal
            ChangeTurn();
            return true;
        }
        public bool CanCastle(Point from, Point to)  // checking if a king can castle according to chess rules
        {
            ChessPiece king = board.GetPieceAt(from);
            if (king == null || king.Type != PieceType.King || (king as King).HasMoved)
            {
                return false;
            }

            int direction = to.X > from.X ? 1 : -1; // if we're castling kingside (right) or queenside (left)
            bool isKingside = direction == 1;
            string castlingSide = isKingside ? "Kingside" : "Queenside";
            string colour = king.Colour == PieceColour.White ? "White" : "Black";

            // Check if castling right exists inside the dictionary
            if (!castlingRights[$"{colour}{castlingSide}"])
            {
                return false;
            }


            int rookX = isKingside ? 7 : 0;
            ChessPiece rook = board.GetPieceAt(new Point(rookX, from.Y));

            if (rook == null || rook.Type != PieceType.Rook || (rook as Rook).HasMoved)
            {
                return false;
            }

            // Check if path is clear between king and rook
            int startX = Math.Min(from.X, rookX) + 1;
            int endX = Math.Max(from.X, rookX);

            if (!isKingside)
            {
                startX = rookX + 1;
                endX = from.X - 1;
            }

            for (int x = startX; x < endX; x++)
            {
                if (board.GetPieceAt(new Point(x, from.Y)) != null)
                    return false;
            }

            // Check if king passes through or ends on a square under attack
            int kingPathStart = from.X;
            int kingPathEnd = to.X;

            for (int x = kingPathStart; x <= kingPathEnd; x++)
            {
                if (IsSquareUnderAttack(new Point(x, from.Y), king.Colour))
                    return false;
            }

            return true;
        }





        private void UndoMove(Point from, Point to, ChessPiece capturedPiece, Point capturedPosition, bool wasCastling)
        {
            if (wasCastling)
            {

                ChessPiece king = board.GetPieceAt(to);
                board.PlacePiece(king, from.X, from.Y);
                board.PlacePiece(null, to.X, to.Y);

                // Move rook back
                int direction = to.X > from.X ? 1 : -1;
                int rookToX = direction == 1 ? from.X + 1 : from.X - 1;
                int rookFromX = direction == 1 ? 7 : 0;

                ChessPiece rook = board.GetPieceAt(new Point(rookToX, from.Y));
                board.PlacePiece(rook, rookFromX, from.Y);
                board.PlacePiece(null, rookToX, from.Y);
            }
            else
            {
                // Undo regular move
                ChessPiece piece = board.GetPieceAt(to);
                board.PlacePiece(piece, from.X, from.Y);
                board.PlacePiece(capturedPiece, to.X, to.Y);
            }
        }

        public bool IsSquareUnderAttack(Point square, PieceColour colour)
        {
            PieceColour opponentColour = colour == PieceColour.White ? PieceColour.Black : PieceColour.White;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ChessPiece piece = board.board[x, y];
                    if (piece != null && piece.Colour == opponentColour)
                    {
                        Point piecePos = new Point(x, y);
                        if (piece.IsValidMove(board, piecePos, square))
                            return true;
                    }
                }
            }
            return false;
        }

        // Update the game state (check for check, checkmate, stalemate)
        private void UpdateGameState()
        {
            if (IsInCheckmate(currentTurn))
            {
                gameOver = true;
                result = (currentTurn == PieceColour.White) ? GameResult.BlackWins : GameResult.WhiteWins;
            }
            else if (IsInStalemate(currentTurn))
            {
                gameOver = true;
                result = GameResult.Draw;
            }
        }

        public bool IsInCheck(PieceColour colour)
        {
            // Find the king's position
            Point kingPosition = FindKing(colour);

            if (kingPosition.X == -1)
            {
                return false;

            }

            return IsSquareUnderAttack(kingPosition, colour);
        }


        private Point FindKing(PieceColour colour)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ChessPiece piece = board.board[x, y];
                    if (piece != null && piece.Type == PieceType.King && piece.Colour == colour)
                    {
                        return new Point(x, y);
                    }
                }
            }

            return new Point(-1, -1);
        }


        public bool IsInCheckmate(PieceColour colour)
        {

            if (!IsInCheck(colour))
            {
                return false;
            }



            return !CanMakeAnyLegalMove(colour);
        }


        public bool IsInStalemate(PieceColour colour)
        {

            if (IsInCheck(colour))
            {
                return false;
            }

            return !CanMakeAnyLegalMove(colour);
        }


        private bool CanMakeAnyLegalMove(PieceColour colour)
        {
            for (int fromX = 0; fromX < 8; fromX++)
            {
                for (int fromY = 0; fromY < 8; fromY++)
                {
                    ChessPiece piece = board.board[fromX, fromY];
                    if (piece != null && piece.Colour == colour)
                    {
                        Point piecePosition = new Point(fromX, fromY);

                        // Try all possible moves for the king
                        for (int toX = 0; toX < 8; toX++)
                        {
                            for (int toY = 0; toY < 8; toY++)
                            {
                                Point targetPosition = new Point(toX, toY);

                                // Skip if piece can't move there
                                if (!piece.IsValidMove(board, piecePosition, targetPosition))
                                {
                                    continue;
                                }
                                ChessPiece capturedPiece = board.GetPieceAt(targetPosition);
                                bool isCastling = piece.Type == PieceType.King && Math.Abs(targetPosition.X - piecePosition.X) == 2;

                                // Temporarily make the move
                                Point capturedPos = targetPosition;
                                board.MovePiece(piecePosition, targetPosition);

                                // Check if king would be in check after this move
                                bool wouldBeInCheck = IsInCheck(colour);

                                // Undo the move
                                UndoMove(piecePosition, targetPosition, capturedPiece, capturedPos, isCastling);

                                // If any legal move is found, return true
                                if (!wouldBeInCheck)
                                    return true;
                            }
                        }
                    }
                }
            }

            // If no legal move is found, return false
            return false;
        }
    }
}