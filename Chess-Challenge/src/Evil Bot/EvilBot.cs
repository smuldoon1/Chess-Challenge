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
            return Minimax(board, board.IsWhiteToMove, int.MinValue, int.MaxValue, 4).move;
        }

        public (Move move, int score) Minimax(Board board, bool isWhite, int alpha, int beta, int depth)
        {
            if (depth == 0 || board.GetLegalMoves().Length == 0)
            {
                return (Move.NullMove, EvaluateBoard(board));
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

        int EvaluateBoard(Board board)
        {
            int whiteScore = 0;
            whiteScore += EvaluateCheckmate(board);
            whiteScore += EvaluateMaterialAdvantage(board);
            whiteScore += Random.Shared.Next(-5, 5);
            return whiteScore;
        }

        int EvaluateCheckmate(Board board)
        {
            return board.IsInCheckmate() ? (board.IsWhiteToMove ? -1000000 : 1000000) : 0;
        }

        int EvaluateMaterialAdvantage(Board board)
        {
            int whiteScore = 0;
            foreach (var pieceType in PieceValues.Keys)
            {
                whiteScore += board.GetPieceList(pieceType, white: true).Count * PieceValues[pieceType];
                whiteScore -= board.GetPieceList(pieceType, white: false).Count * PieceValues[pieceType];
            }
            return whiteScore;
        }
    }
}