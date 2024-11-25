using System;
using UnityEngine;

public class MoveManager
{
    private int remainingMoves;
    private int initialMoves;

    public int RemainingMoves => remainingMoves;

    public void Initialize(int moveCount)
    {
        initialMoves = moveCount;
        remainingMoves = moveCount;
    }

    public void Reset()
    {
        remainingMoves = initialMoves;
        GameEvents.MovesChanged(remainingMoves);
    }

    public bool TryUseMove()
    {
        if (remainingMoves <= 0)
        {
            GameEvents.MovesExhausted();
            return false;
        }

        remainingMoves--;
        GameEvents.MovesChanged(remainingMoves);


        return true;
    }
}