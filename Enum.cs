namespace ChessLogic
{

    //  enum > a constant that wont be changed,
    
    public enum PieceType 
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen, 
        King
    }
    
    public enum PieceColour
    {
        White,
        Black
    }
    public enum Player
    {
       User,
       AI
    }
    public enum GameResult
    {
        Success,
        InProgress,
        BlackWins,
        WhiteWins,
            Draw
    }
    public enum GameOverReason
    {
        Checkmate,
        Stalemate,
        Resignation
    }
}

  