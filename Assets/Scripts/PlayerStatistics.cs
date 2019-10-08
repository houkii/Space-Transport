using System;
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

            if (fuel == MaxFuel)
            {
                OnFuelFull?.Invoke();
            }

            if (fuel <= 0)
            {
                OnOutOfFuel.Invoke();
                DialogCanvasManager.Instance.midInfo.Show("Out Of Fuel!");
            }
        }
    }

    public UnityAction<int> OnScoreUpdated;
    public UnityAction<float> OnFuelUpdated;
    public UnityEvent OnFuelFull;
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
        Fuel = initialFuel;
        MaxFuel = maxFuel;
    }
}