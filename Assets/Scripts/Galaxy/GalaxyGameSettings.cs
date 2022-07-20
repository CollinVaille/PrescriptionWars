using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GalaxyGameSettings
{
    /// <summary>
    /// Publicly accessible and privately mutateable bool that indicates whether or not the galaxy game settings have been loaded in from playerprefs at all yet.
    /// </summary>
    public static bool loaded { get; private set; }

    /// <summary>
    /// Public static property that should be used both to access and mutate the coloring mode of each empire's interior hyperspace lanes.
    /// </summary>
    public static HyperspaceLanesManager.HyperspaceLaneColoringMode interiorHyperspaceLaneColoringMode
    {
        get => interiorHyperspaceLaneColoringModeVar;
        set
        {
            interiorHyperspaceLaneColoringModeVar = value;
            HyperspaceLanesManager.hyperspaceLanesManager.interiorHyperspaceLaneColoringMode = value;
        }
    }
    private static HyperspaceLanesManager.HyperspaceLaneColoringMode interiorHyperspaceLaneColoringModeVar = 0;
    /// <summary>
    /// Public static property that should be used both to access and mutate the static color of the empire's interior hyperspace lanes (note: this color may not be visible depending on the coloring mode).
    /// </summary>
    public static Color interiorHyperspaceLaneStaticColor
    {
        get => interiorHyperspaceLaneStaticColorVar;
        set
        {
            interiorHyperspaceLaneStaticColorVar = value;
            HyperspaceLanesManager.hyperspaceLanesManager.interiorHyperspaceLaneStaticColor = value;
        }
    }
    private static Color interiorHyperspaceLaneStaticColorVar = Color.yellow;

    /// <summary>
    /// Public static property that should be used both to access and mutate the coloring mode of each empire's border hyperspace lanes.
    /// </summary>
    public static HyperspaceLanesManager.HyperspaceLaneColoringMode borderHyperspaceLaneColoringMode
    {
        get => borderHyperspaceLaneColoringModeVar;
        set
        {
            borderHyperspaceLaneColoringModeVar = value;
            HyperspaceLanesManager.hyperspaceLanesManager.borderHyperspaceLaneColoringMode = value;
        }
    }
    private static HyperspaceLanesManager.HyperspaceLaneColoringMode borderHyperspaceLaneColoringModeVar = 0;
    /// <summary>
    /// Public static property that should be used both to access and mutate the static color of the empire's border hyperspace lanes (note: this color may not be visible depending on the coloring mode).
    /// </summary>
    public static Color borderHyperspaceLaneStaticColor
    {
        get => borderHyperspaceLaneStaticColorVar;
        set
        {
            borderHyperspaceLaneStaticColorVar = value;
            HyperspaceLanesManager.hyperspaceLanesManager.borderHyperspaceLaneStaticColor = value;
        }
    }
    private static Color borderHyperspaceLaneStaticColorVar = Color.yellow;

    /// <summary>
    /// Public static method that should be called in order to save the current galaxy game settings to Unity's player prefs.
    /// </summary>
    public static void SaveSettings()
    {
        PlayerPrefs.SetInt("Interior Hyperspace Lane Coloring Mode", (int)interiorHyperspaceLaneColoringModeVar);
        PlayerPrefs.SetFloat("Interior Hyperspace Lane Static Color R", interiorHyperspaceLaneStaticColorVar.r);
        PlayerPrefs.SetFloat("Interior Hyperspace Lane Static Color G", interiorHyperspaceLaneStaticColorVar.g);
        PlayerPrefs.SetFloat("Interior Hyperspace Lane Static Color B", interiorHyperspaceLaneStaticColorVar.b);
        PlayerPrefs.SetFloat("Interior Hyperspace Lane Static Color A", interiorHyperspaceLaneStaticColorVar.a);
        PlayerPrefs.SetInt("Border Hyperspace Lane Coloring Mode", (int)borderHyperspaceLaneColoringModeVar);
        PlayerPrefs.SetFloat("Border Hyperspace Lane Static Color R", borderHyperspaceLaneStaticColorVar.r);
        PlayerPrefs.SetFloat("Border Hyperspace Lane Static Color G", borderHyperspaceLaneStaticColorVar.g);
        PlayerPrefs.SetFloat("Border Hyperspace Lane Static Color B", borderHyperspaceLaneStaticColorVar.b);
        PlayerPrefs.SetFloat("Border Hyperspace Lane Static Color A", borderHyperspaceLaneStaticColorVar.a);
    }

    /// <summary>
    /// Public static method that should be called at the start of every scene in order to load the player's preferred galaxy game settings from Unity's player prefs.
    /// </summary>
    public static void LoadSettings()
    {
        if (loaded)
            return;
        loaded = true;

        interiorHyperspaceLaneStaticColor = new Color(PlayerPrefs.GetFloat("Interior Hyperspace Lane Static Color R", Color.yellow.r), PlayerPrefs.GetFloat("Interior Hyperspace Lane Static Color G", Color.yellow.g), PlayerPrefs.GetFloat("Interior Hyperspace Lane Static Color B", Color.yellow.b), PlayerPrefs.GetFloat("Interior Hyperspace Lane Static Color A", Color.yellow.a));
        interiorHyperspaceLaneColoringMode = (HyperspaceLanesManager.HyperspaceLaneColoringMode)PlayerPrefs.GetInt("Interior Hyperspace Lane Coloring Mode", 0);
        borderHyperspaceLaneStaticColor = new Color(PlayerPrefs.GetFloat("Border Hyperspace Lane Static Color R", Color.yellow.r), PlayerPrefs.GetFloat("Border Hyperspace Lane Static Color G", Color.yellow.g), PlayerPrefs.GetFloat("Border Hyperspace Lane Static Color B", Color.yellow.b), PlayerPrefs.GetFloat("Border Hyperspace Lane Static Color A", Color.yellow.a));
        borderHyperspaceLaneColoringMode = (HyperspaceLanesManager.HyperspaceLaneColoringMode)PlayerPrefs.GetInt("Border Hyperspace Lane Coloring Mode", 0);
    }
}
