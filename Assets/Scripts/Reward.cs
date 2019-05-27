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
    public delegate void RewardGranted(int rewardValue);
    public static event RewardGranted OnRewardGranted;

    protected IRewardArgs Args;
    protected Type argsType;
    public int GetReward(IRewardArgs args)
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
            int rewardValue = this.CalculateReward();
            //OnRewardGranted(rewardValue);
            return rewardValue;
        }
    }

    protected abstract int CalculateReward();

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
        int score = (int)(45 - data.Angle);
        return score;
    }
}

#endregion

#region DeliveryReward

public class DeliveryRewardArgs : IRewardArgs
{
    public float MaximumTime { get; set; }
    public float DeliveryTime { get; set; }

    public DeliveryRewardArgs(float _maxTime, float _deliveryTime = 0)
    {
        MaximumTime = _maxTime;
        DeliveryTime = _deliveryTime;
    }
}

public class DeliveryReward : Reward
{
    public DeliveryReward(Type typeOfArgs) : base(typeOfArgs) { }

    protected override int CalculateReward()
    {
        var data = Args as DeliveryRewardArgs;
        int score = (int)(data.MaximumTime - data.DeliveryTime);
        return score;
    }
}

#endregion

#region FuelReward

public class FuelRewardArgs : IRewardArgs
{
    public float MaxFuel { get; set; }
    public float RemainingFuel { get; set; }
}

public class FuelReward : Reward
{
    public FuelReward(Type typeOfArgs) : base(typeOfArgs) { }

    protected override int CalculateReward()
    {
        var data = Args as FuelRewardArgs;
        int score = (int)(data.MaxFuel - data.RemainingFuel);
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