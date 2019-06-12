using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RewardsView : Singleton<RewardsView>
{
    [SerializeField] private GameObject UIRewardPrefab;
    [SerializeField] private float rewardEntryRange = 175f;

    Coroutine showRewardsCR = null;
    private Queue<Action> UIRewardsQueue = new Queue<Action>();

    private int currentDir = 1;

    private void Start()
    {
        if(showRewardsCR == null)
        {
            showRewardsCR = StartCoroutine(ShowInQueue());
        }
    }

    private void OnEnable()
    {
        Reward.OnRewardGranted += ShowReward;
    }

    private void OnDisable()
    {
        Reward.OnRewardGranted -= ShowReward;
    }

    public void ShowReward(Reward reward)
    {
        //Vector2 position = UnityEngine.Random.insideUnitCircle.normalized;
        //position = rewardEntryRange * new Vector2(
        //    Mathf.Clamp(position.x, Mathf.Sign(position.x) * .5f, Mathf.Sign(position.x)),
        //    Mathf.Abs(position.y));

        Vector2 position = new Vector2(UnityEngine.Random.Range(75, 200), UnityEngine.Random.Range(50, 75));
        position = new Vector2(position.x * currentDir, position.y);
        currentDir *= (-1);

        var rewardObj = Instantiate(UIRewardPrefab, transform);
        rewardObj.GetComponent<RectTransform>().anchoredPosition = position;
        var rewardEntry = rewardObj.GetComponent<UIRewardEntry>();
        var rewardTypeName = reward.GetType().ToString();
        var rewardName = rewardTypeName.Remove(rewardTypeName.IndexOf("Reward"), "Reward".Length);

        UIRewardsQueue.Enqueue(() => rewardEntry.Show(rewardName, reward.Value));
    }

    private IEnumerator ShowInQueue()
    {
        while(true)
        {
            yield return new WaitUntil(() => UIRewardsQueue.Count > 0);
            UIRewardsQueue.Dequeue().Invoke();
            yield return new WaitForSeconds(0.9f);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            GameController.Instance.Rewards.GetReward(Reward.RewardType.LandingReward, new LandingRewardArgs(20, 20, 20));
        }
    }
}