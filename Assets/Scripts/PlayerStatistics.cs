using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerStatistics
{
    public int Score { get; private set; }
    public float TotalFuelUsed { get; private set; }
    public float MaxFuel { get; private set; }
    [SerializeField]
    private float fuel;
    public float Fuel
    {
        get { return fuel; }
        private set
        {
            if (value < fuel)
            {
                TotalFuelUsed += (fuel - value);
            }

            fuel = fuel <= MaxFuel ? value : MaxFuel;

            if (fuel <= 0)
            {
                OnOutOfFuel.Invoke();
            }
        }
    }

    public UnityAction<int> OnScoreUpdated;
    public UnityAction<float> OnFuelUpdated;
    public UnityEvent OnOutOfFuel;

    public void AddScore(int value)
    {
        Score += value;
        OnScoreUpdated?.Invoke(Score);
    }

    public void AddFuel(float value)
    {
        Fuel += value;
        OnFuelUpdated?.Invoke(fuel);
    }

    public PlayerStatistics(float initialFuel, float maxFuel)
    {
        this.Fuel = initialFuel;
        this.MaxFuel = maxFuel;
    }
}
