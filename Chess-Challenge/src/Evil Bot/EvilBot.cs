//#define DEBUG
//#define UCI

using System;
using System.Collections.Generic;
using ChessChallenge.API;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot {
        public const int INFINITY = 999999;
        public const int MAX_DEPTH = 5;

        // 0) None (0)
        // 1) Pawn (100)
        // 2) Knight (325)
        // 3) Bishop (350)
        // 4) Rook (500)
        // 5) Queen (900)
        // 6) King (10000)
        public static int[] pieceValues = { 0, 100, 325, 350, 500, 900, 10000 };
        static Dictionary<ulong, int> transpositionTable = new Dictionary<ulong, int>();

        public Move Think(Board board, Timer timer) {
            // Negamax Root
            int bestEval = -INFINITY;

            Move[] moves = getOrderedMoves(board);
            Move bestMove = moves[0];

            foreach( Move move in moves ) {
                board.MakeMove(move);
                int eval = -negamax(board, -INFINITY, INFINITY, 0);
                board.UndoMove(move);

                if( eval > bestEval ) {
                    bestEval = eval;
                    bestMove = move;
                }
            }
            // End Negamax Root

            return bestMove;
        }

        ///A negamax implementation derived from the following sources
        /// https://en.wikipedia.org/wiki/Negamax
        /// https://www.chessprogramming.org/Negamax
        private int negamax(Board board, int alpha, int beta, int depth) {
            if( depth >= MAX_DEPTH || board.IsInCheckmate() ) {
                return quiesceSearch(board, alpha, beta);
            }

            int value = -INFINITY;
            foreach( Move move in getOrderedMoves(board) ) {
                board.MakeMove(move);
                if( transpositionTable.ContainsKey(board.ZobristKey) ) {
                    value = transpositionTable[board.ZobristKey];
                } else {
                    value = Math.Max(value, -negamax(board, -beta, -alpha, depth + 1));
                    transpositionTable[board.ZobristKey] = value;
                }
                board.UndoMove(move);
                alpha = Math.Max(alpha, value);

                if( alpha >= beta ) {
                    break; // Prune this branch, we don't need to search any more nodes.
                }
            }

            return value;
        }

        private int quiesceSearch(Board board, int alpha, int beta) {
            int standPat = evaluate(board);
            if( standPat >= beta ) {
                return beta;
            }
            if( alpha < standPat ) {
                alpha = standPat;
            }

            Move[] captures = getOrderedMoves(board, true);
            foreach( Move capture in captures ) {
                board.MakeMove(capture);
                int score = -quiesceSearch(board, -beta, -alpha);
                board.UndoMove(capture);

                if( score >= beta ) {
                    return beta;
                }
                if( score > alpha ) {
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
                if( move.IsCapture && !capturePiece.IsNull ) {
                    moveScore += 10 * pieceValues[(int) capturePiece.PieceType];
                }
                // Prioritise moving to a promotion
                if( move.IsPromotion ) {
                    moveScore += pieceValues[(int) move.PromotionPieceType];
                }
                //// Penalize moving into sqaures that are attacked by the opponent.
                if( board.SquareIsAttackedByOpponent(move.TargetSquare) ) {
                    moveScore -= 5 * pieceValues[(int) movingPiece.PieceType];
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

        // Count all of the pieces on the board.
        private int countBoard(Board board, bool isWhite) {
            int value = 0;
            int bishopCount = 0;

            foreach( PieceList pieceList in board.GetAllPieceLists() ) {
                foreach( Piece piece in pieceList ) {
                    if( piece.IsWhite != isWhite ) {
                        continue; // This piece is not our color. Skip it.
                    }

                    bishopCount += piece.IsBishop ? 1 : 0;
                    value += pieceValues[(int) piece.PieceType];
                }
            }

            // Add a bonus for the bishop pair advantage
            // https://www.chessprogramming.org/Bishop_Pair
            value += bishopCount >= 2 ? ( pieceValues[1] / 2 ) : 0;

            return value;
        }
    }
}