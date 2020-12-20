using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPopupManager : MonoBehaviour
{
    public GameObject popupPrefab;

    public List<Sprite> popupSprites;
    public List<string> popupSpriteNames;
    public List<AudioClip> popupSFXList;
    public List<string> popupSFXNames;
    public static List<GalaxyPopup> popups = new List<GalaxyPopup>();

    public static Dictionary<string, Sprite> popupSpritesDictionary = new Dictionary<string, Sprite>();
    public static Dictionary<string, AudioClip> popupSFXDictionary = new Dictionary<string, AudioClip>();

    public static GalaxyPopupManager galaxyPopupManager;

    // Start is called before the first frame update
    void Start()
    {
        galaxyPopupManager = this;
        GeneratePopupSpriteDictionary();
        GeneratePopupSFXDictionary();
    }

    void GeneratePopupSpriteDictionary()
    {
        if(popupSprites.Count == popupSpriteNames.Count)
        {
            for(int x = 0; x < popupSpriteNames.Count; x++)
            {
                popupSpritesDictionary[popupSpriteNames[x]] = popupSprites[x];
            }
        }
        else
        {
            Debug.Log("Popup sprites list and popup sprite names list count do not match! Please fix this issue.");
        }
    }

    void GeneratePopupSFXDictionary()
    {
        if (popupSFXList.Count == popupSFXNames.Count)
        {
            for (int x = 0; x < popupSFXNames.Count; x++)
            {
                popupSFXDictionary[popupSFXNames[x]] = popupSFXList[x];
            }
        }
        else
        {
            Debug.Log("Popup SFXs list and popup SFX names list count do not match! Please fix this issue.");
        }
    }

    public static Sprite GetPopupSpriteFromName(string popupSpriteName)
    {
        if (popupSpritesDictionary.ContainsKey(popupSpriteName))
        {
            return popupSpritesDictionary[popupSpriteName];
        }
        else
        {
            Debug.Log("Invalid Popup Sprite Name (key does not exist in dictionary)");
            return null;
        }
    }

    public static AudioClip GetPopupSFXFromName(string popupSFXName)
    {
        if(popupSFXName == null)
        {
            return null;
        }

        if (popupSFXDictionary.ContainsKey(popupSFXName))
        {
            return popupSFXDictionary[popupSFXName];
        }
        else
        {
            Debug.Log("Invalid Popup SFX Name (key does not exist in dictionary)");
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        GalaxyCamera.mouseOverPopup = IsMouseOverPopup();
    }

    public static void CreateNewPopup(GalaxyPopupData popupData)
    {
        GameObject popup = Instantiate(galaxyPopupManager.popupPrefab);
        popup.transform.parent = galaxyPopupManager.transform;

        popup.GetComponent<GalaxyPopup>().CreatePopup(popupData, popups.Count);

        popups.Add(popup.GetComponent<GalaxyPopup>());
    }

    public static bool IsMouseOverPopup()
    {
        foreach(GalaxyPopup popup in popups)
        {
            if (popup.IsMouseOverPopup())
                return true;
        }

        return false;
    }

    public static void ClosePopup(int popupIndex)
    {
        GalaxyPopup popup = popups[popupIndex];
        for(int x = popupIndex + 1; x < popups.Count; x++)
        {
            popups[x].popupIndex--;
        }
        popups.RemoveAt(popupIndex);
        Destroy(popup.gameObject);

        GalaxyManager.popupClosedOnFrame = true;
    }

    public static bool ContainsNonDismissablePopup()
    {
        foreach (GalaxyPopup popup in popups)
        {
            if (popup.IsAnswerRequired())
                return true;
        }

        return false;
    }

    public static void CloseAllPopups()
    {
        for(int x = popups.Count - 1; x > -1; x--)
        {
            popups[x].Close();
        }
    }

    public static void ApplyPopupOptionEffect(GalaxyPopupOptionEffect effect)
    {
        switch (effect.effectType)
        {
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddCreditsToEmpire:
                GalaxyHelperMethods.AddCreditsToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddCreditsPerTurnToEmpire:
                GalaxyHelperMethods.AddCreditsPerTurnToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddPresciptionsToEmpire:
                GalaxyHelperMethods.AddPrescriptionsToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddPrescriptionsPerTurnToEmpire:
                GalaxyHelperMethods.AddPresciptionsPerTurnToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddScienceToEmpire:
                GalaxyHelperMethods.AddScienceToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.AddSciencePerTurnToEmpire:
                GalaxyHelperMethods.AddSciencePerTurnToEmpire(effect.effectAmount, effect.effectDependencies[0]);
                break;
            case GalaxyPopupOptionEffect.GalaxyPopupOptionEffectType.ConquerPlanet:
                GalaxyHelperMethods.ConquerPlanet(effect.effectDependencies[0], effect.effectDependencies[1]);
                break;

            default:
                break;
        }
    }
}
