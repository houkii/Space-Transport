using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Effects : MonoBehaviour
{
    public float intensity;
    private Material material;

    void Awake()
    {
        material = new Material(Shader.Find("Custom/ChromaticAberrationShader"));
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (intensity == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        material.SetFloat("_Amount", intensity);
        Graphics.Blit(source, destination, material);
    }
}