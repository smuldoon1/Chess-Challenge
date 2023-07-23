using ChessChallenge.API;
using System;
using System.Collections.Generic;
using System.Linq;

public class MyBot : IChessBot
{
    // Base piece values
    readonly Dictionary<PieceType, float> PieceValues = new()
    {
        { PieceType.Pawn, 1f },
        { PieceType.Knight, 3f },
        { PieceType.Bishop, 3f },
        { PieceType.Rook, 5f },
        { PieceType.Queen, 9f },
        { PieceType.King, 1000f } // King is given a very high value to always prioritise its safety
    };

    public Move Think(Board board, Timer timer)
    {
        List<(Move move, float rating)> moveRatings = new();

        foreach (var move in board.GetLegalMoves())
        {
            moveRatings.Add((move, EvaluateBoard(board, move)));
        }

        var bestMove = moveRatings.OrderByDescending(x => x.rating).ThenBy(_ => Random.Shared.Next()).First().move;
        return bestMove;
    }

    public float EvaluateBoard(Board board, Move move)
    {
        board.MakeMove(move);

        float returnScore;

        // If checkmate in one, this move is best
        if (board.IsInCheckmate())
            returnScore = 100000f;

        // If move is a draw
        else if (board.IsDraw())
        {
            // If down on material, take the draw
            if (GetMaterialScore(board) < 0)
                returnScore = 100000f;
            else
            // If up on material, avoid the draw
                returnScore = -100000f;
        }

        // Else, rate the move based on a response from the opponent in the next move if this move is made
        else
        {
            var nextMoveRating = 10000f;

            // For each legal response from the opponent.
            foreach (Move nextMove in board.GetLegalMoves())
            {
                board.MakeMove(nextMove);

                // If opponent will be able to checkmate, avoid the move
                if (board.IsInCheckmate())
                    nextMoveRating = -100000f;

                var boardScore = -GetMaterialScore(board);

                board.UndoMove(nextMove);

                // Always take the lowest rating
                nextMoveRating = Math.Min(nextMoveRating, boardScore);
            }

            returnScore = nextMoveRating;
        }
        board.UndoMove(move);
        return returnScore;
    }

    // Get the score 
    float GetMaterialScore(Board board)
    {
        float whiteScore = 0f;
        foreach (var pieceType in PieceValues.Keys)
        {
            whiteScore += board.GetPieceList(pieceType, white: true).Count * PieceValues[pieceType];
        }

        float blackScore = 0f;
        foreach (var pieceType in PieceValues.Keys)
        {
            blackScore += board.GetPieceList(pieceType, white: false).Count * PieceValues[pieceType];
        }

        return (whiteScore - blackScore) * (board.IsWhiteToMove ? -1f : 1f);
    }
}