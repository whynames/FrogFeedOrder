using System;
using System.Collections.Generic;

public static class GameEvents
{
    public static event Action<List<List<Node>>> OnMapLoaded;
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<Frog> OnFrogShootStarted;
    public static event Action<Frog> OnFrogShootFinished;
    public static event Action<int> OnMovesChanged;
    public static event Action OnMovesExhausted;
    public static event Action OnBerryCollected;
    public static event Action OnCollectionFinished;

    public static void MapLoaded(List<List<Node>> map)
    {
        OnMapLoaded?.Invoke(map);
    }

    public static void GameStateChanged(GameState newState)
    {
        OnGameStateChanged?.Invoke(newState);
    }

    public static void FrogShootStarted(Frog frog)
    {
        OnFrogShootStarted?.Invoke(frog);
    }

    public static void FrogShootFinished(Frog frog)
    {
        OnFrogShootFinished?.Invoke(frog);
    }

    public static void MovesChanged(int remainingMoves)
    {
        OnMovesChanged?.Invoke(remainingMoves);
    }

    public static void MovesExhausted()
    {
        OnMovesExhausted?.Invoke();
    }

    public static void BerryCollected()
    {
        OnBerryCollected?.Invoke();
    }

    internal static void CollectionFinished()
    {
        OnCollectionFinished?.Invoke();
    }
}