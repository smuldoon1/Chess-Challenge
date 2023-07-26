using ChessChallenge.API;
using System;
using System.Collections.Generic;

namespace ChessChallenge.Example
{
    // A simple bot that can spot mate in one, and always captures the most valuable piece it can.
    // Plays randomly otherwise.
    public class EvilBot : IChessBot
    {
        // Base piece values
        readonly Dictionary<PieceType, int> PieceValues = new()
    {
        { PieceType.Pawn, 100 },
        { PieceType.Knight, 300 },
        { PieceType.Bishop, 300 },
        { PieceType.Rook, 500 },
        { PieceType.Queen, 900 },
        { PieceType.King, 10000 } // King is given a very high value to always prioritise its safety
    };

        public Move Think(Board board, Timer timer)
        {
            return Minimax(board, board.IsWhiteToMove, int.MinValue, int.MaxValue, 3).move;
        }

        public (Move move, int score) Minimax(Board board, bool isWhite, int alpha, int beta, int depth)
        {
            if (depth == 0 || false)
            {
                return (Move.NullMove, GetMaterialScore(board));
            }

            if (isWhite)
            {
                (Move move, int score) bestMove = (Move.NullMove, int.MinValue);
                foreach (var move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    var nextMove = Minimax(board, !isWhite, alpha, beta, depth - 1);
                    board.UndoMove(move);

                    if (nextMove.score > bestMove.score)
                    {
                        bestMove = (move, nextMove.score);
                    }

                    alpha = Math.Max(alpha, nextMove.score);
                    if (beta <= alpha)
                        break;
                }
                return bestMove;
            }
            else
            {
                (Move move, int score) bestMove = (Move.NullMove, int.MaxValue);
                foreach (var move in board.GetLegalMoves())
                {
                    board.MakeMove(move);
                    var nextMove = Minimax(board, !isWhite, alpha, beta, depth - 1);
                    board.UndoMove(move);

                    if (nextMove.score < bestMove.score)
                    {
                        bestMove = (move, nextMove.score);
                    }

                    beta = Math.Min(beta, nextMove.score);
                    if (beta <= alpha)
                        break;
                }
                return bestMove;
            }
        }

        // Get the score 
        int GetMaterialScore(Board board)
        {
            int whiteScore = 0;
            foreach (var pieceType in PieceValues.Keys)
            {
                whiteScore += board.GetPieceList(pieceType, white: true).Count * PieceValues[pieceType];
            }

            int blackScore = 0;
            foreach (var pieceType in PieceValues.Keys)
            {
                blackScore += board.GetPieceList(pieceType, white: false).Count * PieceValues[pieceType];
            }

            return whiteScore - blackScore;
        }
    }
}