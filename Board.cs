using ChessLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;



namespace ChessLogic
{
    public class Position
    {
        public int Row { get; }
        public int Column { get; }

        public Position(int row, int column)
        {
            Row = row;
            Column = column;
        }
    }
    public class Board
    {
        public ChessPiece[,] board { get; private set; } = new ChessPiece[8, 8];
        public GameState GameState { get; set; }
        public Board()
        {
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board[x, y] = null;
                }
            }
        }
        public void PlacePiece(ChessPiece piece, int x, int y)
        {
            board[x, y] = piece;
            if (piece != null)
            {
                piece.X = x;
                piece.Y = y;
                piece.Position = new System.Drawing.Point(x, y);
            }
        }
        public bool IsEmptySquare(Point position)
        {
            
            bool isPositionValid = IsValidPosition(position);

            if (!isPositionValid)
            {
                return false;
            }

            return board[position.X, position.Y] == null;
        }
        public bool IsValidPosition(Point position)
        {
            return position.X >= 0 && position.X < 8 && position.Y >= 0 && position.Y < 8;
        }
        public ChessPiece GetPieceAt(Point position)
        {
            if (!IsValidPosition(position))
                return null;

            return board[position.X, position.Y];
        }
        public bool MovePiece(Point from, Point to)
        {
            ChessPiece piece = GetPieceAt(from);

            if (piece == null)
            {
                return false;
            }              

            if (!piece.IsValidMove(this, from, to))
            {
                return false;
            }
                

            bool isCastling = piece.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;

            if (isCastling)
            {
                HandleCastlingMove(from, to);
            }
            else
            {
                
                board[to.X, to.Y] = piece;
                board[from.X, from.Y] = null;
            }

            // Update castling rights and piece position
            UpdateCastlingRights(piece, from);
            piece.X = to.X;
            piece.Y = to.Y;
            piece.Position = to;


            if (piece.Type == PieceType.King)
            {
                (piece as King).HasMoved = true;
            }

            else if (piece.Type == PieceType.Rook)
            {
                (piece as Rook).HasMoved = true;
            }


            return true;
        }

        private void HandleCastlingMove(Point from, Point to)
        {
            int direction = to.X > from.X ? 1 : -1;
            int rookFromX = direction == 1 ? 7 : 0;
            int rookToX = direction == 1 ? from.X + 1 : from.X - 1;      // if rook or king step outside their squares (without castling), castlings right are gone
            ChessPiece king = board[from.X, from.Y];
            ChessPiece rook = board[rookFromX, from.Y];

           
            board[to.X, to.Y] = king;
            board[from.X, from.Y] = null;

            
            board[rookToX, from.Y] = rook;
            board[rookFromX, from.Y] = null;

            // Update rook position
            rook.X = rookToX;
            rook.Y = from.Y;
            rook.Position = new Point(rookToX, from.Y);
            (rook as Rook).HasMoved = true;
        }

        private void UpdateCastlingRights(ChessPiece piece, Point originalPosition)
        {
            if (GameState == null)
            {
                return;
            }


            if (piece.Type == PieceType.King)
            {
                string colour = piece.Colour == PieceColour.White ? "White" : "Black";
                GameState.CastlingRights[$"{colour}Kingside"] = false;
                GameState.CastlingRights[$"{colour}Queenside"] = false;
            }
            else if (piece.Type == PieceType.Rook)
            {
                string colour = piece.Colour == PieceColour.White ? "White" : "Black";
                string side = originalPosition.X == 0 ? "Queenside" : "Kingside";
                GameState.CastlingRights[$"{colour}{side}"] = false;
            }
        }

        public void CreateInitialBoard()
        {
            board = new ChessPiece[8, 8];

            // Create pawns
            for (int x = 0; x < 8; x++)
            {
                board[x, 1] = new Pawn(PieceType.Pawn, PieceColour.Black, x, 1);
                board[x, 6] = new Pawn(PieceType.Pawn, PieceColour.White, x, 6);
            }

            // Create rooks
            board[0, 0] = new Rook(PieceType.Rook, PieceColour.Black, 0, 0);
            board[7, 0] = new Rook(PieceType.Rook, PieceColour.Black, 7, 0);
            board[0, 7] = new Rook(PieceType.Rook, PieceColour.White, 0, 7);
            board[7, 7] = new Rook(PieceType.Rook, PieceColour.White, 7, 7);

            // Create knights
            board[1, 0] = new Knight(PieceType.Knight, PieceColour.Black, 1, 0);
            board[6, 0] = new Knight(PieceType.Knight, PieceColour.Black, 6, 0);
            board[1, 7] = new Knight(PieceType.Knight, PieceColour.White, 1, 7);
            board[6, 7] = new Knight(PieceType.Knight, PieceColour.White, 6, 7);

            // Create bishops
            board[2, 0] = new Bishop(PieceType.Bishop, PieceColour.Black, 2, 0);
            board[5, 0] = new Bishop(PieceType.Bishop, PieceColour.Black, 5, 0);
            board[2, 7] = new Bishop(PieceType.Bishop, PieceColour.White, 2, 7);
            board[5, 7] = new Bishop(PieceType.Bishop, PieceColour.White, 5, 7);

            // Create queens
            board[3, 0] = new Queen(PieceType.Queen, PieceColour.Black, 3, 0);
            board[3, 7] = new Queen(PieceType.Queen, PieceColour.White, 3, 7);

            // Create kings
            board[4, 0] = new King(PieceType.King, PieceColour.Black, 4, 0);
            board[4, 7] = new King(PieceType.King, PieceColour.White, 4, 7);
        }
    }
}