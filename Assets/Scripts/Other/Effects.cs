using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class Effects : MonoBehaviour
{
    public float intensity;
    private Material chromaticAberrationmaterial;
    //private Material outlineMaterial;
    [SerializeField] private Shader chromaticAberrationShader;
    //[SerializeField] private Shader outlineShader;

    void Awake()
    {
        //chromaticAberrationmaterial = new Material(chromaticAberrationShader);
        //outlineMaterial = new Material(outlineShader);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (intensity == 0)
        {
            Graphics.Blit(source, destination);
            return;
        }

        chromaticAberrationmaterial.SetFloat("_Amount", intensity);
        Graphics.Blit(source, destination, chromaticAberrationmaterial);
    }
}