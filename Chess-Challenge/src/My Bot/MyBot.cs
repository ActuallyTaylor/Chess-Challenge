using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer)
    {
        Move[] moves = board.GetLegalMoves();

        int bestEval = int.MinValue;
        Move bestMove = moves[0];

        foreach (Move move in moves)
        {
            board.MakeMove(move);
            int eval = minimax(board, 3, int.MinValue, int.MaxValue, true);
            if (eval >= bestEval)
            {
                bestEval = eval;
                bestMove = move;
            }
            board.UndoMove(move);
        }
        Console.Write("Best Eval ({0}) - Best Move ({1}, {2} -> {3})\n", bestEval, bestMove, board.GetPiece(bestMove.StartSquare), board.GetPiece(bestMove.TargetSquare));

        return bestMove;
    }

    private int minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
    {
        // Return static evaluation of the board if we are at the lowest depth
        // or we have reached an end state
        if (depth == 0 || board.IsDraw() || board.IsInCheck() ||
            board.IsInCheckmate() || board.IsRepeatedPosition() ||
            board.IsInsufficientMaterial())
        {
            return evaluate(board);
        }

        Move[] moves = board.GetLegalMoves();

        if (maximizingPlayer)
        {
            int maxEval = int.MinValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = minimax(board, depth - 1, alpha, beta, false);
                board.UndoMove(move);
                maxEval = Math.Max(maxEval, eval);
                alpha = Math.Max(alpha, eval);

                if (beta <= alpha)
                {
                    break;
                }
            }
            return maxEval;
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (Move move in moves)
            {
                board.MakeMove(move);
                int eval = minimax(board, depth - 1, alpha, beta, true);
                board.UndoMove(move);
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


    // Evaluate the board and return a value based on the current pieces in play.
    private int evaluate(Board board)
    {
        int whiteEval = countBoard(board, true);
        int blackEval = countBoard(board, false);

        int evaluation = whiteEval - blackEval;

        return evaluation * (board.IsWhiteToMove ? 1 : -1);
    }

    // Count all of hte pieces on the board.
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

                value += pieceValues[(int)piece.PieceType];
            }
        }

        return value;
    }
}