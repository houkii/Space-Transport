public class RewardFactory
{
    public int GetReward(Reward.RewardType rewardType, IRewardArgs rewardArgs)
    {
        switch (rewardType)
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