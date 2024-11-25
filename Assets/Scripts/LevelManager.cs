using UnityEngine;
using System.Collections.Generic;
using VContainer;
using Cysharp.Threading.Tasks;
using System;

public class LevelManager
{

    private List<List<Node>> currentMap;

    private readonly MapLoader mapLoader;
    private List<string> levelSequence;
    private int currentLevelIndex = -1;
    private bool isCurrentLevelComplete = false;
    private Frog currentFrog;
    public Frog CurrentFrog => currentFrog;
    private readonly MoveManager moveManager;

    public LevelManager(MapLoader mapLoader, MoveManager moveManager)
    {
        this.mapLoader = mapLoader;
        this.moveManager = moveManager;
        InitializeLevelSequence();
        GameEvents.OnFrogShootStarted += HandleFrogShootStarted;
        GameEvents.OnFrogShootFinished += HandleFrogShootFinished;
        GameEvents.OnMapLoaded += HandleMapLoaded;
        GameEvents.OnMovesExhausted += OnMovesExhausted;
    }

    private void OnMovesExhausted()
    {
        Debug.Log("Moves exhausted");
    }

    private void HandleFrogShootFinished(Frog frog)
    {
        currentFrog = null;
        bool allNodesEmpty = true;

        foreach (var row in currentMap)
        {
            foreach (var node in row)
            {
                var cell = node.GetTopCell();
                if (cell == null) continue;
                if (cell.TryGetOwner<Frog>())
                {
                    allNodesEmpty = false;
                    break;
                }
            }
            if (!allNodesEmpty) break;
        }

        if (allNodesEmpty)
        {
            ResetLevel();
        }
        else
        {
            if (moveManager.RemainingMoves <= 0)
            {
                QuitGame();
            }
        }

    }
    private void QuitGame()
    {
        Debug.Log("You Lost!");
    }
    private void ResetLevel()
    {
        moveManager.Reset();
        isCurrentLevelComplete = true;
        Debug.Log("Level complete");
        GameEvents.GameStateChanged(GameState.Win);
        LoadNextLevel();
    }
    private void HandleMapLoaded(List<List<Node>> list)
    {
        currentMap = list;
    }

    private void InitializeLevelSequence()
    {
        levelSequence = new List<string>
        {
            "MapEasy",
            "MapMedium",
            "MapHard"
        };
    }

    public bool IsCurrentLevelComplete => isCurrentLevelComplete;

    public int CurrentLevelIndex { get => currentLevelIndex; set => currentLevelIndex = value; }

    public void SetLevelComplete()
    {
        isCurrentLevelComplete = true;
    }

    public async UniTask LoadNextLevel()
    {
        isCurrentLevelComplete = false;
        CurrentLevelIndex++;
        if (CurrentLevelIndex >= levelSequence.Count)
        {
            Debug.LogWarning("No more levels available!");
            return;
        }

        await LoadLevel(levelSequence[CurrentLevelIndex]);
    }

    public async UniTask LoadLevel(string levelName)
    {
        CurrentLevelIndex = levelSequence.IndexOf(levelName);
        await mapLoader.LoadMap(levelName);
    }

    public async UniTask ReloadCurrentLevel()
    {
        if (CurrentLevelIndex >= 0 && CurrentLevelIndex < levelSequence.Count)
        {
            await LoadLevel(levelSequence[CurrentLevelIndex]);
        }
    }

    public bool HasNextLevel()
    {
        return CurrentLevelIndex < levelSequence.Count - 1;
    }

    public void SetCurrentFrog(Frog frog)
    {
        currentFrog = frog;
    }

    private void HandleFrogShootStarted(Frog frog)
    {
        currentFrog = frog;
    }
}