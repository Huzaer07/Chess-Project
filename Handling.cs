using System;
using System.Collections.Generic;
using System.Drawing;
using static ChessLogic.ChessPiece;
using ChessLogic;

namespace ChessLogic
{

    public class Handling
    {     
        public class Points      
        {
            public int X { get; set; }
            public int Y { get; set; }

            public Points(int x, int y)
            {
                X = x;
                Y = y;
            }
        }

       

        private Piece selectedPiece = null;   
        private Point selectedPosition;  

        public void SelectPiece(Piece piece, Point position)
        {
            selectedPiece = piece;                   
            selectedPosition = position;
        }
        public void DeselectPiece() 

        {
            selectedPiece = null;
        }

        public Piece GetSelectedPiece() 
        {
            return selectedPiece;
        }

        public Point GetSelectedPosition()
        {
            return selectedPosition;
        }

        public bool IsPieceSelected()
        {
            return selectedPiece != null;
        }
    }
    public class Piece
    {
    }

    public class PieceColourPick
    {
        public Player Player { get; set; }
        public PieceType Type { get; set; }


    }

}


