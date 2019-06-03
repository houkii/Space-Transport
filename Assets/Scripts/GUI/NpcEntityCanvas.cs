﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class NpcEntityCanvas : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI name;
    [SerializeField]
    private Vector3 offset = new Vector3(40, 80, 0);
    private NPCEntity Target;

    public SpeechBubble SpeechBubble { get; private set; }

    void Awake()
    {
        SpeechBubble = GetComponentInChildren<SpeechBubble>();
    }

    void Update()
    {
        if(Target != null)
        {
            this.transform.position = Camera.main.WorldToScreenPoint(Target.transform.position) + offset;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void Initialize(NPCEntity target)
    {
        this.Target = target;
        this.name.text = target.name;
        //this.Target.OnGotAboard.AddListener(() => this.SpeechBubble.Hide());
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        transform.DOScale(Vector3.zero, .35f).SetEase(Ease.InExpo).OnComplete(() =>
        {
            transform.localScale = Vector3.one * .7f;
            gameObject.SetActive(false);
        });
    }
}