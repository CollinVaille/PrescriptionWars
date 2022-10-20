using UnityEngine;

[System.Serializable]
public struct DualColorSet
{
    public Color colorOne;
    public Color colorTwo;

    public Color this[int index]
    {
        get => GetColor(index);
        set => SetColor(index, value);
    }

    private Color GetColor(int index)
    {
        if (index == 0)
            return colorOne;
        else if (index == 1)
            return colorTwo;
        else
        {
            Debug.Log("Invalid Dual Color Set Index (must be either 0 or 1).");
            return new Color();
        }
    }

    private void SetColor(int index, Color color)
    {
        if (index == 0)
            colorOne = color;
        else if (index == 1)
            colorTwo = color;
        else
            Debug.Log("Invalid Dual Color Set Index (must be either 0 or 1).");
    }
}