using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using ChessLogic;

namespace ChessLogic
{
    public class ChessAI
    {
        private readonly Board board;
        private readonly GameState gameState;
        private readonly int maxDepth = 3; // depth can be changed to diffuculty level, higher depth, harder AI is to beat.

        // Piece weights for board evaluation
        private readonly Dictionary<PieceType, int> pieceValues = new Dictionary<PieceType, int>
        {
            { PieceType.Pawn, 1 },     // assigning values so it helps with piece evaultion
            { PieceType.Knight, 3 },
            { PieceType.Bishop, 3 },
            { PieceType.Rook, 5 },
            { PieceType.Queen, 9 },
            { PieceType.King, 9999 } 
        };

        public ChessAI(Board board, GameState gameState)
        {
            this.board = board;
            this.gameState = gameState;
        }

        public bool MakeMove()
        {
            try
            {
                // AI only plays as Black
                if (gameState.CurrentTurn != PieceColour.Black || gameState.IsGameOver)
                {
                    return false;
                }

                var bestMove = FindBestMove();
                if (bestMove.Item1.X != -1) // Valid move found
                {
                    bool moveMade = gameState.TryMakeMove(bestMove.Item1, bestMove.Item2);
                    return moveMade;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AI Error: {ex.Message}");
                return false;
            }
        }

        private Tuple<Point, Point> FindBestMove()
        {
            var legalMoves = GetAllLegalMoves(PieceColour.Black);

            if (!legalMoves.Any())
            {
                return new Tuple<Point, Point>(new Point(-1, -1), new Point(-1, -1));
            }

            // Sort moves for better alpha-beta pruning performance (initial ordering)
            legalMoves = MergeSortMoves(legalMoves);

            int bestValue = int.MinValue;
            Point bestMoveFrom = new Point(-1, -1);
            Point bestMoveTo = new Point(-1, -1);

            foreach (var move in legalMoves)
            {
                Point from = move.Item1;
                Point to = move.Item2;

                // Temporarily store the captured piece (if any)
                ChessPiece capturedPiece = board.GetPieceAt(to);
                bool isCastling = board.GetPieceAt(from)?.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;

                // Make the move
                board.MovePiece(from, to);

                // Evaluate this move
                int moveValue = Minimax(maxDepth - 1, int.MinValue, int.MaxValue, false);

                // Undo the move
                UndoMove(from, to, capturedPiece, isCastling);

                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMoveFrom = from;
                    bestMoveTo = to;
                }
            }

            return new Tuple<Point, Point>(bestMoveFrom, bestMoveTo);
        }

        private int Minimax(int depth, int alpha, int beta, bool isMaximising)
        {
            // Base case - terminal node or depth limit reached
            if (depth == 0 || gameState.IsGameOver)
            {
                return EvaluateBoard();
            }

            if (isMaximising) 
            {
                int maxEval = int.MinValue;
                var legalMoves = GetAllLegalMoves(PieceColour.Black);

                foreach (var move in legalMoves)
                {
                    Point from = move.Item1;
                    Point to = move.Item2;

                    
                    ChessPiece capturedPiece = board.GetPieceAt(to);
                    bool isCastling = board.GetPieceAt(from)?.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;

                   
                    board.MovePiece(from, to);

                    // Recursively evaluate this move
                    int eval = Minimax(depth - 1, alpha, beta, false);

                 
                    UndoMove(from, to, capturedPiece, isCastling);

                    maxEval = Math.Max(maxEval, eval);
                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                    {
                        break; // Beta cutoff
                    }
                }
                return maxEval;
            }
            else // Human (White) is minimising
            {
                int minEval = int.MaxValue;
                var legalMoves = GetAllLegalMoves(PieceColour.White);

                foreach (var move in legalMoves)
                {
                    Point from = move.Item1;
                    Point to = move.Item2;

                    
                    ChessPiece capturedPiece = board.GetPieceAt(to);
                    bool isCastling = board.GetPieceAt(from)?.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;

                   
                    board.MovePiece(from, to);

                    // will keep evaulting this move
                    int eval = Minimax(depth - 1, alpha, beta, true);

                    
                    UndoMove(from, to, capturedPiece, isCastling);

                    minEval = Math.Min(minEval, eval);
                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                    {
                        break; 
                    }
                }
                return minEval;
            }
        }

        private void UndoMove(Point from, Point to, ChessPiece capturedPiece, bool isCastling)
        {
            if (isCastling)
            {
                // Move king back
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
               
                ChessPiece piece = board.GetPieceAt(to);

                // Move it back to its original position
                board.PlacePiece(piece, from.X, from.Y);

                
                board.PlacePiece(capturedPiece, to.X, to.Y);
            }
        }

        private int EvaluateBoard()
        {
            int score = 0;

            
            if (gameState.IsInCheckmate(PieceColour.White))
                return 1000; // Black wins
            if (gameState.IsInCheckmate(PieceColour.Black))
                return -1000; // White wins
            if (gameState.IsInStalemate(PieceColour.White) || gameState.IsInStalemate(PieceColour.Black))
                return 0; // Draw

            // Calculate material advantage
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    ChessPiece piece = board.board[x, y];
                    if (piece != null)
                    {
                        int pieceValue = pieceValues[piece.Type];
                        if (piece.Colour == PieceColour.Black)
                            score += pieceValue;
                        else
                            score -= pieceValue;
                    }
                }
            }

            // Add positional evaluation
            score += EvaluatePosition();

            return score;
        }

        private int EvaluatePosition()
        {
            int score = 0;

            // Center control bonus, makes the AI have a more engaing playstyle
            for (int x = 2; x <= 5; x++)
            {
                for (int y = 2; y <= 5; y++)
                {
                    ChessPiece piece = board.board[x, y];
                    if (piece != null)
                    {
                        if (piece.Colour == PieceColour.Black)
                            score += 1;
                        else
                            score -= 1;
                    }
                }
            }

            // Check threats
            if (gameState.IsInCheck(PieceColour.White))
                score += 3;
            if (gameState.IsInCheck(PieceColour.Black))
                score -= 3;

            return score;
        }

        private List<Tuple<Point, Point>> GetAllLegalMoves(PieceColour colour)     // STACK 
        {
            var legalMoves = new List<Tuple<Point, Point>>();
            var stack = new Stack<Point>();

            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    var position = new Point(x, y);
                    ChessPiece piece = board.GetPieceAt(position);

                    if (piece != null && piece.Colour == colour)
                    {
                        stack.Push(position);
                    }
                }
            }

            // Processes each piece to find its legal moves
            while (stack.Count > 0)
            {
                Point piecePos = stack.Pop();
                ChessPiece piece = board.GetPieceAt(piecePos);

                // Check for castling moves if this is a king
                if (piece.Type == PieceType.King)
                {
                    // Kingside castling
                    Point kingsideCastling = new Point(piecePos.X + 2, piecePos.Y);
                    if (gameState.CanCastle(piecePos, kingsideCastling))
                    {
                        legalMoves.Add(new Tuple<Point, Point>(piecePos, kingsideCastling));
                    }

                    // Queenside castling
                    Point queensideCastling = new Point(piecePos.X - 2, piecePos.Y);
                    if (gameState.CanCastle(piecePos, queensideCastling))
                    {
                        legalMoves.Add(new Tuple<Point, Point>(piecePos, queensideCastling));
                    }
                }

               
                for (int toX = 0; toX < 8; toX++)
                {
                    for (int toY = 0; toY < 8; toY++)
                    {
                        Point targetPos = new Point(toX, toY);

                       
                        if (piece.IsValidMove(board, piecePos, targetPos))
                        {
                            ChessPiece capturedPiece = board.GetPieceAt(targetPos);
                            bool isCastling = piece.Type == PieceType.King && Math.Abs(targetPos.X - piecePos.X) == 2;

                            board.MovePiece(piecePos, targetPos);

                            // Verify the move doesn't leave the king in check
                            bool isLegal = !gameState.IsInCheck(colour);

                            UndoMove(piecePos, targetPos, capturedPiece, isCastling);

                            if (isLegal)
                            {
                                legalMoves.Add(new Tuple<Point, Point>(piecePos, targetPos));
                            }
                        }
                    }
                }
            }

            return legalMoves;
        }
       
        // Merge sort for move ordering to improve alpha-beta pruning
        private List<Tuple<Point, Point>> MergeSortMoves(List<Tuple<Point, Point>> moves)
        {
            if (moves.Count <= 1)
                return moves;

            int mid = moves.Count / 2;
            var left = moves.GetRange(0, mid);
            var right = moves.GetRange(mid, moves.Count - mid);

            left = MergeSortMoves(left);
            right = MergeSortMoves(right);

            return MergeMoves(left, right);
        }

        private List<Tuple<Point, Point>> MergeMoves(List<Tuple<Point, Point>> left, List<Tuple<Point, Point>> right)
        {
            var result = new List<Tuple<Point, Point>>();
            int i = 0, j = 0;

            while (i < left.Count && j < right.Count)
            {
                // Evaluate move priority (captures prioritised)
                int leftScore = EvaluateMoveScore(left[i]);
                int rightScore = EvaluateMoveScore(right[j]);

                if (leftScore >= rightScore)
                {
                    result.Add(left[i++]);
                }
                else
                {
                    result.Add(right[j++]);
                }
            }

            // Add remaining elements
            while (i < left.Count)
                result.Add(left[i++]);

            while (j < right.Count)
                result.Add(right[j++]);

            return result;
        }

        private int EvaluateMoveScore(Tuple<Point, Point> move)
        {
            Point from = move.Item1;
            Point to = move.Item2;
            ChessPiece movingPiece = board.GetPieceAt(from);
            ChessPiece capturedPiece = board.GetPieceAt(to);

            int score = 0;

            // Prioritise captures (especially high-value captures with low-value pieces)
            if (capturedPiece != null)
            {
                score += 10 * pieceValues[capturedPiece.Type] - pieceValues[movingPiece.Type];
            }

            // Prioritise castling
            bool isCastling = movingPiece.Type == PieceType.King && Math.Abs(to.X - from.X) == 2;
            if (isCastling)
            {
                score += 12; // Make castling desirable but not as important as capturing a queen
            }

            // Prioritise center control
            if ((to.X >= 2 && to.X <= 5) && (to.Y >= 2 && to.Y <= 5))
            {
                score += 2;
            }

            // Prioritise check moves
            board.MovePiece(from, to);
            if (gameState.IsInCheck(PieceColour.White))
            {
                score += 5;
            }
            UndoMove(from, to, capturedPiece, isCastling);

            return score;
        }
    }
}
