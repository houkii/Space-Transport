using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatistics
{
    public int Score { get; private set; }

    public UnityAction<int> OnScoreUpdated;

    public void AddScore(int value)
    {
        Score += value;
        OnScoreUpdated?.Invoke(Score);
    }

    public void Initialize()
    {

    }
}
