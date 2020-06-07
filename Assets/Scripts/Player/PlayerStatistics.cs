using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class PlayerStatistics
{
    public int Score { get; private set; }
    public float TotalFuelUsed { get; private set; }
    public float MaxFuel;
    public UnityAction<int> OnScoreUpdated;
    public UnityAction<float> OnFuelUpdated;
    public UnityEvent OnFuelFull;
    public UnityEvent OnOutOfFuel;

    private bool fuelExhausted = false;
    [SerializeField] private float fuel;
    public float Fuel
    {
        get { return fuel; }
        private set => SetFuel(value);
    }

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

    private void SetFuel(float value)
    {
        if (fuelExhausted)
            return;

        if (value < fuel)
        {
            TotalFuelUsed += (fuel - value);
        }

        fuel = fuel <= MaxFuel ? value : MaxFuel;

        if (fuel >= MaxFuel)
        {
            OnFuelFull?.Invoke();
        }

        if (fuel <= 0)
        {
            OnOutOfFuel.Invoke();
            DialogCanvasManager.Instance.midInfo.Show("Out Of Fuel!");
            fuelExhausted = true;
        }
    }
}