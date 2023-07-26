using ChessChallenge.API;
using System;
using System.ComponentModel;
using System.Linq;
using static System.Formats.Asn1.AsnWriter;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        Move? bestMove = null;
        int bestEval = int.MinValue;

        foreach (Move move in moves)
        {
            int eval = search(board, 3);

            if (eval > bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
        }

        return bestMove ?? moves[0];
    }

    private int search(Board board, int depth)
    {
        Move[] moves = board.GetLegalMoves();

        if (depth == 0)
        {
            return evaluate(board);
        }

        int bestEval = int.MinValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = -search(board, depth - 1);
            bestEval = Math.Max(bestEval, eval);
            board.UndoMove(move);
        }

        return bestEval;
    }

    //private int HighestValueCapture(Board board, Move[] moves)
    //{
    //    int bestValue = int.MinValue;

    //    foreach (Move move in moves)
    //    {
    //        if (!move.IsCapture) { continue; }
    //        Piece piece = board.GetPiece(move.TargetSquare);
    //        bestValue = Math.Max(pieceValues[(int)piece.PieceType], bestValue);
    //    }

    //    return bestValue;
    //}

    // Evaluate the board and return a value based on the current pieces in play.
    private int evaluate(Board board)
    {
        int whiteEval = countBoard(board, true);
        int blackEval = countBoard(board, false);

        int evaluation = whiteEval - blackEval;

        return evaluation * (board.IsWhiteToMove ? -1 : 1);
    }

    private int countBoard(Board board, bool isWhite)
    {
        int value = 0;
                
        foreach (PieceList pieceList in board.GetAllPieceLists())
        {
            foreach (Piece piece in pieceList)
            {
                if (piece.IsWhite != isWhite)
                {
                    continue;
                }

                value += pieceValues[(int) piece.PieceType];
            }
        }

        return value;
   }

}