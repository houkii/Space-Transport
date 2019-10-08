using UnityEngine;

public class VelocityIndicator : MonoBehaviour
{
    [SerializeField] private GameObject indicatorGO;
    [SerializeField] private Rigidbody targetBody;
    [SerializeField] private float size = 12f;
    [SerializeField] private float offsetRadius = 20f;
    [SerializeField] private Material indicatorMaterial;

    private void Awake()
    {
        size = indicatorGO.transform.localScale.x;
    }

    private void FixedUpdate()
    {
        if (targetBody.velocity.magnitude <= 0) return;
        transform.rotation = Quaternion.LookRotation(targetBody.velocity.normalized, targetBody.transform.up);
        var currentSize = PlayerController.Instance.CurrentToMaximumVelocityMagnitudeRatio;
        transform.localScale = Vector3.one * currentSize * size;
        transform.position = PlayerController.Instance.transform.position + transform.forward * offsetRadius;
    }
}