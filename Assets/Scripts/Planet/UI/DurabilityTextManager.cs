using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DurabilityTextManager
{
    private static Text durabilityPrimaryText, durabilitySecondaryText;

    public static void SetUp(Transform hud)
    {
        durabilityPrimaryText = hud.Find("Durability Primary Text").GetComponent<Text>();
        durabilitySecondaryText = hud.Find("Durability Primary Text").Find("Durability Secondary Text").GetComponent<Text>();
    }

    public static void ClearDurabilityText()
    {
        durabilityPrimaryText.enabled = false;
        durabilitySecondaryText.enabled = false;
    }

    public static void SetDurabilityText(int primaryDurability)
    {
        SetDurabilityPrimaryText(Mathf.Min(primaryDurability, 999).ToString());
        durabilitySecondaryText.enabled = false;
    }

    public static void SetDurabilityText(int primaryDurability, int secondaryDurability)
    {
        SetDurabilityPrimaryText(primaryDurability.ToString());
        SetDurabilitySecondaryText("/ " + secondaryDurability);
    }

    public static bool ShowingDurabilityTextCurrently() { return durabilityPrimaryText.enabled; }

    private static void SetDurabilityPrimaryText(string primaryText)
    {
        if (primaryText == null || primaryText.Equals(""))
            durabilityPrimaryText.enabled = false;
        else
        {
            durabilityPrimaryText.text = primaryText;
            durabilityPrimaryText.enabled = true;
        }
    }

    private static void SetDurabilitySecondaryText(string secondaryText)
    {
        if (secondaryText == null || secondaryText.Equals(""))
            durabilitySecondaryText.enabled = false;
        else
        {
            durabilitySecondaryText.text = secondaryText;
            durabilitySecondaryText.enabled = true;
        }
    }
}
