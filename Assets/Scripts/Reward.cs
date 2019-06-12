using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public interface IRewardArgs
{
}

public interface IReward
{
    int GetReward(IRewardArgs args);
}

public abstract class Reward : IReward
{
    public enum RewardType { LandingReward, DeliveryReward, FuelReward };
    public delegate void RewardGranted(Reward reward);
    public static event RewardGranted OnRewardGranted;
    public int Value { get; private set; }

    protected IRewardArgs Args;
    protected Type argsType;
    public virtual int GetReward(IRewardArgs args)
    {
        if (args.GetType() != argsType)
        {
            Debug.LogError("Wrong Datatype in getting rewards!");
            return 0;
        }
        else
        {
            this.Args = args;
            if (args == null) throw new ArgumentException();
            Value = this.CalculateReward();
            OnRewardGranted(this);
            return Value;
        }
    }

    protected abstract int CalculateReward();
    //protected abstract string GetName();

    public Reward(Type typeOfArgs)
    {
        argsType = typeOfArgs;
    }
}

#region LandingReward

public class LandingRewardArgs : IRewardArgs
{
    public float Angle { get; set; }
    public float Velocity { get; set; }
    public float Distance { get; set; }

    public LandingRewardArgs(float _angle, float _velocity, float _distance)
    {
        Angle = _angle;
        Velocity = _velocity;
        Distance = _distance;
    }
}

public class LandingReward : Reward
{
    public LandingReward(Type typeOfArgs) : base(typeOfArgs) { }

    protected override int CalculateReward()
    {
        //dynamic data = Convert.ChangeType(Args, argsType);
        var data = Args as LandingRewardArgs;
        int score = (int)(45 - data.Angle) * GameController.Instance.Settings.LandingRewardMultiplier;
        return score;
    }
}

#endregion

#region DeliveryReward

public class DeliveryRewardArgs : IRewardArgs
{
    public float MaximumTime { get; set; }
    public float DeliveryTime { get; set; }
    public float CurrentTime { get; private set; }
    public float CurrentToMaxTimeRatio => CurrentTime / MaximumTime;

    public DeliveryRewardArgs(float _maxTime, float _deliveryTime = 0)
    {
        MaximumTime = _maxTime;
        DeliveryTime = _deliveryTime;
        CurrentTime = _maxTime;
    }

    public void Process()
    {
        CurrentTime -= Time.deltaTime;
    }
}

public class DeliveryReward : Reward
{
    public DeliveryReward(Type typeOfArgs) : base(typeOfArgs) { }

    protected override int CalculateReward()
    {
        var data = Args as DeliveryRewardArgs;
        //int score = (int)(data.MaximumTime - data.DeliveryTime);
        int score = (int)(data.MaximumTime - data.DeliveryTime) * GameController.Instance.Settings.DeliveryRewardMultiplier;
        return score;
    }
}

#endregion

#region FuelReward

public class FuelRewardArgs : IRewardArgs
{
    public float MaxFuel { get; set; }
    public float RemainingFuel { get; set; }
    public float TotalFuelUsed { get; set; }

    public FuelRewardArgs(float maxFuel, float remainingFuel, float totalFuelUsed = 0)
    {
        MaxFuel = maxFuel;
        RemainingFuel = remainingFuel;
        TotalFuelUsed = totalFuelUsed;
    }
}

public class FuelReward : Reward
{
    public FuelReward(Type typeOfArgs) : base(typeOfArgs) { }

    protected override int CalculateReward()
    {
        var data = Args as FuelRewardArgs;
        int score = (int)((data.RemainingFuel / data.MaxFuel) * GameController.Instance.Settings.MaxRewardForRemainingFuel);
        if(data.TotalFuelUsed > 0)
        {
            score += (int)(GameController.Instance.Settings.MaxRewardForTotalFuelUsed / data.TotalFuelUsed);
        }
        return score;
    }
}

#endregion

#region RewardFactory

public class RewardFactory
{
    public int GetReward(Reward.RewardType rewardType, IRewardArgs rewardArgs)
    {
        switch(rewardType)
        {
            case Reward.RewardType.DeliveryReward:
                return new DeliveryReward(typeof(DeliveryRewardArgs)).GetReward(rewardArgs);
            case Reward.RewardType.LandingReward:
                return new LandingReward(typeof(LandingRewardArgs)).GetReward(rewardArgs);
            case Reward.RewardType.FuelReward:
                return new FuelReward(typeof(FuelRewardArgs)).GetReward(rewardArgs);
            default:
                throw new System.NotImplementedException();
        }
    }
}

#endregion