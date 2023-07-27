// I have not written C# in any capacity in years. I have also barely ever
// played chess. So this is a mess

using ChessChallenge.API;
using System;

public class MyBot : IChessBot {
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer) {
        // Negamax Root
        int bestEval = int.MinValue;

        Move[] moves = board.GetLegalMoves();
        Move bestMove = moves[0];

        foreach( Move move in moves ) {
            board.MakeMove(move);
            int eval = -negamax(board, 3);//, int.MinValue, int.MaxValue, true);
            if( eval >= bestEval ) {
                bestEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);
        }
        // End Negamax Root

        return bestMove;
    }

    private int negamax(Board board, int depth) {
        if( depth == 0 ) {
            return evaluate(board);
        }
        int max = int.MinValue;
        foreach( Move move in board.GetLegalMoves() ) {
            board.MakeMove(move);
            int score = -negamax(board, depth - 1);
            board.UndoMove(move);
            max = Math.Max(score, max);
        }

        return max;
    }

    // Evaluate the board and return a value based on the current pieces in play.
    private int evaluate(Board board) {
        int whiteEval = countBoard(board, true);
        int blackEval = countBoard(board, false);

        int evaluation = whiteEval - blackEval;

        return evaluation * ( board.IsWhiteToMove ? 1 : -1 );
    }

    // Count all of hte pieces on the board.
    private int countBoard(Board board, bool isWhite) {
        int value = 0;

        foreach( PieceList pieceList in board.GetAllPieceLists() ) {
            foreach( Piece piece in pieceList ) {
                if( piece.IsWhite != isWhite ) {
                    continue;
                }

                value += pieceValues[( int ) piece.PieceType];
            }
        }

        return value;
    }
}