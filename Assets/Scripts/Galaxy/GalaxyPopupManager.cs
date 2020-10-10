using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyPopupManager : MonoBehaviour
{
    public GameObject popupPrefab;

    public List<Sprite> popupSprites;
    public List<string> popupSpriteNames;
    public static List<GalaxyPopup> popups = new List<GalaxyPopup>();

    public static Dictionary<string, Sprite> popupSpritesDictionary = new Dictionary<string, Sprite>();

    public static GalaxyPopupManager galaxyPopupManager;

    // Start is called before the first frame update
    void Start()
    {
        galaxyPopupManager = this;
        GeneratePopupSpriteDictionary();
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

    public static Sprite GetPopupSpriteFromName(string popupSpriteName)
    {
        if (popupSpritesDictionary.ContainsKey(popupSpriteName))
        {
            return popupSpritesDictionary[popupSpriteName];
        }
        else
        {
            Debug.Log("Invalid Popup Sprite Names (key does not exist in dictionary)");
            return null;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public static void CreateNewPopup(GalaxyPopupData popupData)
    {
        GameObject popup = Instantiate(galaxyPopupManager.popupPrefab);
        popup.transform.parent = galaxyPopupManager.transform;

        popup.GetComponent<GalaxyPopup>().CreatePopup(popupData, popups.Count);
        popup.transform.localScale = new Vector3(0, 0, 1);
        popup.transform.localPosition = new Vector3(0, 0, 0);
        popup.transform.SetAsLastSibling();

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
    }
}
