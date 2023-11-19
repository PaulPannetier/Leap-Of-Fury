using UnityEngine;

public class ColorGradient
{
    public Color[] colors;
    public float[] colorPositions;
    public static ColorGradient Create(int colorCount)
    {
        ColorGradient instance = new ColorGradient();
        instance.colors = new Color[colorCount];
        instance.colorPositions = new float[colorCount];
        return instance;
    }

    public void SetColor(int index, float position, float r, float g, float b, float a)
    {
        colors[index] = new Color(r, g, b, a);
        colorPositions[index] = position;
    }

    public Color Evaluate(float position)
    {
        int indexFrom = 0;
        int indexTo = 1;
        position = Mathf.Clamp(position, 0, 1);
        for (int i = 0; i < (colors.Length - 1); i++)
        {
            if ((position >= colorPositions[i]) && (position <= colorPositions[i + 1]))
            {
                indexFrom = i;
                indexTo = i + 1;
                break;
            }
        }
        if (indexFrom == indexTo) return colors[indexTo];

        float proSpan = colorPositions[indexTo] - colorPositions[indexFrom];
        float pro = (position - colorPositions[indexFrom]) / proSpan;
        return Color.Lerp(colors[indexFrom], colors[indexTo], pro);
    }
}
