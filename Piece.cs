using ChessLogic;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq.Expressions;

namespace ChessLogic
{
    public class ChessPiece
    {
        public int X { get; set; }
        public int Y { get; set; }
        public Point Position { get; set; }
        public PieceColour Colour { get; set; }
        public PieceType Type { get; set; }

        public ChessPiece(PieceType type, PieceColour colour, int x, int y)
        {
            Type = type;
            Colour = colour;
            X = x;
            Y = y;
            Position = new Point(x, y);
        }


        public virtual bool IsValidMove(Board board, Point from, Point to)
        {

            if (!board.IsValidPosition(to))
                return false;

            // Cant move to a square occupied by same colour piece
            ChessPiece targetPiece = board.GetPieceAt(to);
            if (targetPiece != null && targetPiece.Colour == this.Colour)
                return false;

            return true;
        }




    }
    public class Pawn : ChessPiece
    {
        public bool HasMovedTwoSquares { get; set; } = false;
        public Pawn(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)


        {



        }
        public override bool IsValidMove(Board board, Point from, Point to)
        {
            // Initial validation
            if (!board.IsValidPosition(to))
            {
                return false;
            }

            // Check if destination has same colour piece
            ChessPiece destinationPiece = board.GetPieceAt(to);
            if (destinationPiece != null && destinationPiece.Colour == this.Colour)
                return false;

            int x = from.X;
            int y = from.Y;

            // White pawn movement
            if (Colour == PieceColour.White)
            {
                // Moving one square forward
                if (to.X == from.X && to.Y == from.Y - 1)
                {
                    return board.IsEmptySquare(to);
                }

                // Moving two squares forward from starting position
                if (to.X == from.X && to.Y == from.Y - 2 && from.Y == 6)
                {
                    Point oneSquareAhead = new Point(from.X, from.Y - 1);
                    return board.IsEmptySquare(oneSquareAhead) && board.IsEmptySquare(to);
                }

                // Diagonal capture (including en passant)
                if ((to.X == from.X - 1 || to.X == from.X + 1) && to.Y == from.Y - 1)
                {
                    // Normal capture
                    ChessPiece targetPiece = board.GetPieceAt(to);
                    if (targetPiece != null && targetPiece.Colour != this.Colour)
                        return true;

                    // En passant capture
                    Point adjacentPoint = new Point(to.X, from.Y);
                    ChessPiece adjacentPiece = board.GetPieceAt(adjacentPoint);
                    if (adjacentPiece != null && adjacentPiece.Type == PieceType.Pawn &&
                        adjacentPiece.Colour != this.Colour &&
                        ((Pawn)adjacentPiece).HasMovedTwoSquares &&
                        board.GameState.LastMovedPiece == adjacentPiece)
                    {
                        return true;
                    }
                }

                // If none of the valid moves matched
                return false;
            }
            // Black pawn movement
            else  // Colour == PieceColour.Black
            {
                // Moving one square forward
                if (to.X == from.X && to.Y == from.Y + 1)
                {
                    return board.IsEmptySquare(to);
                }

                // Moving two squares forward from starting position
                if (to.X == from.X && to.Y == from.Y + 2 && from.Y == 1)
                {
                    Point oneSquareAhead = new Point(from.X, from.Y + 1);
                    return board.IsEmptySquare(oneSquareAhead) && board.IsEmptySquare(to);
                }

                // Diagonal capture (including en passant)
                if ((to.X == from.X - 1 || to.X == from.X + 1) && to.Y == from.Y + 1)
                {
                    // Normal capture
                    ChessPiece targetPiece = board.GetPieceAt(to);
                    if (targetPiece != null && targetPiece.Colour != this.Colour)
                        return true;

                    // En passant capture for black pawns
                    Point adjacentPoint = new Point(to.X, from.Y);
                    ChessPiece adjacentPiece = board.GetPieceAt(adjacentPoint);
                    if (adjacentPiece != null && adjacentPiece.Type == PieceType.Pawn &&
                        adjacentPiece.Colour != this.Colour &&
                        ((Pawn)adjacentPiece).HasMovedTwoSquares &&
                        board.GameState.LastMovedPiece == adjacentPiece)
                    {
                        return true;
                    }
                }

                // If none of the valid moves matched
                return false;
            }
        }
    }
}
    public class Knight : ChessPiece
    {
        public Knight(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)
        {

        }
        public override bool IsValidMove(Board board, Point from, Point to)
        {
            if (!base.IsValidMove(board, from, to))
                return false;


            int x = from.X;
            int y = from.Y;

            // Knight movement patterns - all 8 possible L-shaped moves

            // Move 2 right, 1 up
            if (to.X == from.X + 2 && to.Y == from.Y - 1)
                return true;

            // Move 2 right, 1 down
            if (to.X == from.X + 2 && to.Y == from.Y + 1)
                return true;

            // Move 2 left, 1 up
            if (to.X == from.X - 2 && to.Y == from.Y - 1)
                return true;

            // Move 2 left, 1 down
            if (to.X == from.X - 2 && to.Y == from.Y + 1)
                return true;

            // Move 1 right, 2 up
            if (to.X == from.X + 1 && to.Y == from.Y - 2)
                return true;

            // Move 1 right, 2 down
            if (to.X == from.X + 1 && to.Y == from.Y + 2)
                return true;

            // Move 1 left, 2 up
            if (to.X == from.X - 1 && to.Y == from.Y - 2)
                return true;

            // Move 1 left, 2 down
            if (to.X == from.X - 1 && to.Y == from.Y + 2)
                return true;

            return false;
        }
    }
    public class King : ChessPiece
    {
        public bool HasMoved { get; set; } = false;
        public King(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)
        {

        }
        public override bool IsValidMove(Board board, Point from, Point to)
        {
            if (!base.IsValidMove(board, from, to))
            {
                return false;
            }

            if (Math.Abs(to.X - from.X) == 2 && from.Y == to.Y)
            {
                return IsValidCastlingMove(board, from, to);
            }

            int x = from.X;
            int y = from.Y;



            // Horizontal movement (left/right)
            if ((to.X == from.X + 1 || to.X == from.X - 1) && to.Y == from.Y)
            {
                return true;
            }


            // Vertical movement (up/down)
            if (to.X == from.X && (to.Y == from.Y + 1 || to.Y == from.Y - 1))
            {
                return true;
            }


            // Diagonal movement - up-right
            if (to.X == from.X + 1 && to.Y == from.Y - 1)
            {
                return true;

            }


            // Diagonal movement - up-left
            if (to.X == from.X - 1 && to.Y == from.Y - 1)
            {
                return true;
            }


            // Diagonal movement - down-right
            if (to.X == from.X + 1 && to.Y == from.Y + 1)
            {

                return true;
            }


            // Diagonal movement - down-left
            if (to.X == from.X - 1 && to.Y == from.Y + 1)
            {
                return true;
            }



            return false;


        }
        private bool IsValidCastlingMove(Board board, Point from, Point to)
        {
            if (HasMoved)
            {
                return false;


            }

            string colourPrefix = Colour == PieceColour.White ? "White" : "Black";
            string side = to.X > from.X ? "Kingside" : "Queenside";

            // Check castling rights
            if (!board.GameState.CastlingRights[$"{colourPrefix}{side}"])
            {
                return false;
            }



            int direction = to.X > from.X ? 1 : -1;
            for (int x = from.X + direction; x != to.X; x += direction)
            {
                if (!board.IsEmptySquare(new Point(x, from.Y)))
                    return false;
            }

            // Check safety
            for (int x = from.X; x != to.X + direction; x += direction)
            {
                if (board.GameState.IsSquareUnderAttack(new Point(x, from.Y), Colour))
                    return false;
            }

            // Check rook validity
            int rookX = side == "Kingside" ? 7 : 0;
            ChessPiece rook = board.GetPieceAt(new Point(rookX, from.Y));
            if (rook == null || rook.Type != PieceType.Rook || (rook as Rook).HasMoved)
            {
                return false;
            }

            return true;
        }


    }
    public class Rook : ChessPiece

    {
        public bool HasMoved { get; set; } = false;
        public Rook(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)
        {
            ;

        }
        public override bool IsValidMove(Board board, Point from, Point to)
        {

            if (!base.IsValidMove(board, from, to))
            {
                return false;
            }




            int x = from.X;
            int y = from.Y;



            Point Horizontal = new Point(x, y);
            if (from.Y == to.Y)
            {
                int step;
                if (to.X > from.X)
                {
                    step = 1;
                }
                else
                {
                    step = -1;
                }

                for (int i = from.X + step; i != to.X; i += step)
                {
                    Point Square = new Point(i, y);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                }

                return true;
            }


            Point Vertical = new Point(x, y);
            if (from.X == to.X)
            {
                int step;
                if (to.Y > from.Y)
                {
                    step = 1;
                }
                else
                {
                    step = -1;
                }

                for (int i = from.Y + step; i != to.Y; i += step)
                {
                    Point Square = new Point(x, i);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }


    }
    public class Queen : ChessPiece
    {
        public Queen(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)
        {


        }


        public override bool IsValidMove(Board board, Point from, Point to)
        {
            if (!base.IsValidMove(board, from, to))
            {
                return false;

            }



            int x = from.X;
            int y = from.Y;


            Point Horizontal = new Point(x, y);
            if (from.Y == to.Y)
            {
                int step;
                if (to.X > from.X)
                {
                    step = 1;
                }
                else
                {
                    step = -1;
                }

                for (int i = from.X + step; i != to.X; i += step)
                {
                    Point Square = new Point(i, y);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                }

                return true;
            }


            Point Vertical = new Point(x, y);
            if (from.X == to.X)
            {
                int step;
                if (to.Y > from.Y) // Moving down
                {
                    step = 1;
                }
                else // Moving up
                {
                    step = -1;
                }

                for (int i = from.Y + step; i != to.Y; i += step) // loop is going to continue until its doesnt hit the target position (piece),
                                                                  // each loop the step (square) increments
                {
                    Point Square = new Point(x, i);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                }

                return true;
            }

            // Diagonal Movement (like Bishop)
            // Check if the move is diagonal (difference in X equals difference in Y)
            if (Math.Abs(to.X - from.X) == Math.Abs(to.Y - from.Y))
            {

                int stepX = (to.X > from.X) ? 1 : -1;
                int stepY = (to.Y > from.Y) ? 1 : -1;


                Point TopRight = new Point(x, y);
                if (to.X > from.X && to.Y < from.Y) // Moving top-right
                {
                    int j = from.Y + stepY;

                    for (int i = from.X + stepX; i != to.X; i += stepX)
                    {
                        Point Square = new Point(i, j);
                        if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                        {
                            return false;
                        }

                        j += stepY;
                    }
                    return true;
                }


                Point TopLeft = new Point(x, y);
                if (to.X < from.X && to.Y < from.Y) // Moving top-left
                {
                    int j = from.Y + stepY;

                    for (int i = from.X + stepX; i != to.X; i += stepX)
                    {
                        Point Square = new Point(i, j);
                        if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                        {
                            return false;
                        }
                        j += stepY;
                    }
                    return true;
                }


                Point BottomRight = new Point(x, y);
                if (to.X > from.X && to.Y > from.Y)
                {
                    int j = from.Y + stepY;

                    for (int i = from.X + stepX; i != to.X; i += stepX)
                    {
                        Point Square = new Point(i, j);
                        if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                        {
                            return false;
                        }

                        j += stepY;
                    }
                    return true;
                }


                Point BottomLeft = new Point(x, y);
                if (to.X < from.X && to.Y > from.Y)
                {
                    int j = from.Y + stepY;

                    for (int i = from.X + stepX; i != to.X; i += stepX)
                    {
                        Point Square = new Point(i, j);
                        if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                        {
                            return false; // Blocked path
                        }
                        j += stepY;
                    }
                    return true;
                }
            }

            return false;
        }
    }
    public class Bishop : ChessPiece
    {
        public Bishop(PieceType type, PieceColour colour, int x, int y) : base(type, colour, x, y)
        {



        }
        public override bool IsValidMove(Board board, Point from, Point to)
        {

            if (!base.IsValidMove(board, from, to))
                return false;



            int x = from.X;
            int y = from.Y;

            // Check if the move is diagonal (difference in X equals difference in Y)
            if (Math.Abs(to.X - from.X) != Math.Abs(to.Y - from.Y))
            {
                return false;
            }

            // Determine direction for X and Y
            int stepX;
            if (to.X > from.X)
            {
                stepX = 1;
            }
            else
            {
                stepX = -1;
            }

            int stepY;
            if (to.Y > from.Y)
            {
                stepY = 1;
            }
            else
            {
                stepY = -1;
            }

            Point TopRight = new Point(x, y);
            if (to.X > from.X && to.Y < from.Y)
            {
                int j = from.Y + stepY;

                for (int i = from.X + stepX; i != to.X; i += stepX)
                {
                    Point Square = new Point(i, j);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false; // Blocked path
                    }
                    j += stepY;
                }
                return true;
            }


            Point TopLeft = new Point(x, y);
            if (to.X < from.X && to.Y < from.Y)
            {
                int j = from.Y + stepY;

                for (int i = from.X + stepX; i != to.X; i += stepX)
                {
                    Point Square = new Point(i, j);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                    j += stepY;
                }
                return true;
            }


            Point BottomRight = new Point(x, y);
            if (to.X > from.X && to.Y > from.Y)
            {
                int j = from.Y + stepY; // Start from next position in Y direction

                for (int i = from.X + stepX; i != to.X; i += stepX)
                {
                    Point Square = new Point(i, j);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                    j += stepY;
                }
                return true;
            }


            Point BottomLeft = new Point(x, y);
            if (to.X < from.X && to.Y > from.Y)
            {
                int j = from.Y + stepY;

                for (int i = from.X + stepX; i != to.X; i += stepX)
                {
                    Point Square = new Point(i, j);
                    if (!board.IsValidPosition(Square) || !board.IsEmptySquare(Square))
                    {
                        return false;
                    }
                    j += stepY;
                }
                return true;
            }

            return false;


        }
    }






