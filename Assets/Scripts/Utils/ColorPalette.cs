using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ColorPalette
{
    [SerializeField] private List<Color> paletteColors;
    public List<Color> PaletteColors
    {
        get { return paletteColors; }
        private set { paletteColors = value; }
    }

    public Color RandomColor => PaletteColors[Random.Range(0, PaletteColors.Count)];

    public ColorPalette(List<Color32> colors)
    {
        foreach (Color32 color in colors)
            PaletteColors.Add((Color)color);
    }
}