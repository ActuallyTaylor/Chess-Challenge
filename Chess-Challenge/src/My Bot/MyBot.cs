﻿// I have not written C# in any capacity in years. I have also barely ever
// played chess. So this is a mess

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ChessChallenge.API;

public class MyBot : IChessBot {
    int[] pieceValues = { 0, 100, 300, 300, 500, 900, 10000 };

    public Move Think(Board board, Timer timer) {
        // Negamax Root
        int bestEval = int.MinValue;

        Move[] moves = getOrderedMoves(board);
        Move bestMove = moves[0];

        foreach( Move move in moves ) {
            board.MakeMove(move);
            int eval = -negamax(board, int.MinValue, int.MaxValue, 5);
            board.UndoMove(move);

            if( eval >= bestEval ) {
                bestEval = eval;
                bestMove = move;
            }
        }
        // End Negamax Root

        return bestMove;
    }

    private int negamax(Board board, int alpha, int beta, int depth) {
        if( depth == 0 || board.IsDraw() || board.IsInCheck() ||
            board.IsInCheckmate() || board.IsRepeatedPosition() ||
            board.IsInsufficientMaterial() ) {
            return evaluate(board);
        }

        foreach( Move move in getOrderedMoves(board)) {
            board.MakeMove(move);
            int score = -negamax(board, -beta, -alpha, depth - 1);
            board.UndoMove(move);

            if( score >= beta ) {
                return beta;
            }
            if( score > alpha ) {
                alpha = score;
            }
        }

        return alpha;
    }

    private int quiesceSearch(Board board, int alpha, int beta) {
        int eval = evaluate(board);
        if ( eval >= beta) {
            return beta;
        }
        if (alpha < eval ) {
            alpha = eval;
        }

        Move[] captures = getOrderedMoves(board, true);
        foreach(Move capture in captures) {
            board.MakeMove(capture);
            int score = -quiesceSearch(board, -beta, -alpha);
            board.UndoMove(capture);

            if (score >= beta) {
                return beta;
            }
            if (score > alpha) {
                alpha = score;
            }
        }

        return alpha;
    }

    private Move[] getOrderedMoves(Board board, bool capturesOnly = false) {
        Move[] moves = board.GetLegalMoves(capturesOnly);
        int[] moveScores = new int[moves.Length];

        for( int i = 0; i < moves.Length; i++ ) {
            Move move = moves[i];
            int moveScore = 0;
            Piece movingPiece = board.GetPiece(move.StartSquare);
            Piece capturePiece = board.GetPiece(move.TargetSquare);

            // Prioritise captures based on the value of the captured piece.
            if( move.IsCapture && !capturePiece.IsNull) {
                moveScore += 10 * pieceValues[(int) capturePiece.PieceType];
            }
            // Prioritise moving to a promotion
            if( move.IsPromotion) {
                moveScore += pieceValues[( int ) move.PromotionPieceType];
            }
            //// Penalize moving into sqaures that are attacked by the opponent.
            if( board.SquareIsAttackedByOpponent(move.TargetSquare) ) {
                moveScore -= pieceValues[(int) movingPiece.PieceType];
            }
            //// Prioritise moving out of the way of attacks
            if( board.SquareIsAttackedByOpponent(move.StartSquare) ) {
                moveScore += pieceValues[(int) movingPiece.PieceType];
            }

            moveScores[i] = moveScore;
        }

        // Sort the moves based on the scores in moveScores
        Array.Sort(moveScores, moves);
        // Reverse the list so the biggest scores come first.
        Array.Reverse(moves);

        return moves;
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
                    // Evaluate if the enemy is attacking any of our pieces
                    
                    continue; // Skip. Don't add the general points
                }

                value += pieceValues[( int ) piece.PieceType];
            }
        }

        return value;
    }
}

