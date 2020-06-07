using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShipThruster : MonoBehaviour
{
    private Material material;
    private MeshRenderer renderer;
    private Sequence activeSequence;
    private float rotation = 0f;
    private float currentParentYScale;
    [SerializeField] private float maxRotation = 2f;
    private bool canRotate = false;

    void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        material = renderer.sharedMaterials[0];
    }

    private void Update()
    {
        this.SetRotation();
        this.SetScale();
    }

    private void SetRotation()
    {
        if (canRotate)
        {
            rotation = Mathf.Lerp(rotation, PlayerController.Instance.DelayedForwardAngle / 45, .3f);
        }
        material.SetFloat("_Rotation", rotation);
    }

    private void SetScale()
    {
        currentParentYScale = transform.parent.transform.localScale.y;
        //float yScale = Mathf.Lerp(transform.parent.transform.localScale.y, 4*PlayerController.Instance.CurrentToMaximumVelocityMagnitudeRatio, 0.05f);
        //transform.parent.transform.localScale = new Vector3(1, yScale, 1);
    }

    public void SetActive(bool active)
    {
        activeSequence.Kill();
        if(active)
        {
            activeSequence = GetStartSequence();
        }
        else
        {
            activeSequence = GetHideSequence();
        }
    }

    private Sequence GetStartSequence()
    {
        renderer.enabled = true;
        //this.canRotate = true;
        Sequence seq = DOTween.Sequence();
        transform.parent.transform.localScale = new Vector3(1, currentParentYScale, 1);
        seq.Append(transform.parent.transform.DOScaleY(.25f, 1.25f).SetEase(Ease.OutExpo))
            .AppendCallback(() => canRotate = true)
            .Append(transform.parent.transform.DOScaleY(.55f, 1.25f).SetEase(Ease.OutExpo))
            .Append(transform.parent.transform.DOScaleY(1f, 1.35f).SetEase(Ease.InExpo));
        return seq;
    }

    private Sequence GetHideSequence()
    {
        Sequence seq = DOTween.Sequence();
        this.canRotate = false;
        seq.Append(DOTween.To(()=>rotation, x => rotation = x, 0, .15f))
            .Join(transform.parent.transform.DOScaleY(0, .2f))
            .AppendCallback(() => {
                renderer.enabled = false;
            });

        return seq;
    }
}
