using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsView : Singleton<RewardsView>
{
    [SerializeField] private GameObject UIRewardPrefab;
    private Coroutine showRewardsCoroutine = null;
    private Queue<Action> UIRewardsQueue = new Queue<Action>();
    private int currentDir = 1;

    private void Start()
    {
        if (showRewardsCoroutine == null)
        {
            showRewardsCoroutine = StartCoroutine(ShowInQueue());
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
        Vector2 position = new Vector2(UnityEngine.Random.Range(75, 200), UnityEngine.Random.Range(50, 75));
        currentDir *= (-1);
        position = new Vector2(position.x * currentDir, position.y);
        var rewardObj = Instantiate(UIRewardPrefab, transform);
        rewardObj.GetComponent<RectTransform>().anchoredPosition = position;
        var rewardEntry = rewardObj.GetComponent<UIRewardEntry>();
        rewardEntry.Initialize(reward, currentDir);
        UIRewardsQueue.Enqueue(() => rewardEntry.Show());
    }

    private IEnumerator ShowInQueue()
    {
        while (true)
        {
            yield return new WaitUntil(() => UIRewardsQueue.Count > 0);
            UIRewardsQueue.Dequeue().Invoke();
            yield return new WaitForSeconds(0.9f);
        }
    }
}