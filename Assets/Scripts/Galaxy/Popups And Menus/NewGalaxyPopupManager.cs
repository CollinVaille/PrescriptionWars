using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGalaxyPopupManager : MonoBehaviour
{
    /// <summary>
    /// Private list that contains all of the popups currently active within the galaxy scene.
    /// </summary>
    private List<NewGalaxyPopup> popups = null;

    /// <summary>
    /// Public property that should be used in order to access the list of save data for the popups that are active within the galaxy scene. Popups that are in the action of closing will not be in the list of save data since saving them is not neccessary.
    /// </summary>
    public List<NewGalaxyPopupData> popupsSaveData
    {
        get
        {
            //Declares and initializes a new list of popup save data.
            List<NewGalaxyPopupData> popupsSaveData = new List<NewGalaxyPopupData>();

            //Returns the empty list if the popups list is either null or empty itself.
            if (popups == null || popups.Count == 0)
                return popupsSaveData;

            //Loops through each popup and adds its save data to the save data list if it is not in the action of closing.
            foreach (NewGalaxyPopup popup in popups)
                if (!popup.closing)
                    popupsSaveData.Add(new NewGalaxyPopupData(popup));

            //Returns the list of popup save data.
            return popupsSaveData;
        }
    }

    /// <summary>
    /// Public method that should be called in order to initialize the popup manager either at the start of a new game or with save game popup data.
    /// </summary>
    /// <param name="popupsSaveData"></param>
    public void Initialize(List<NewGalaxyPopupData> popupsSaveData = null)
    {
        //Destroys any popups that might exist at the moment and resets the popups list to null.
        if (popups != null)
            for (int popupIndex = popups.Count - 1; popupIndex >= 0; popupIndex--)
                Destroy(popups[popupIndex].gameObject);
        popups = null;

        //Loops through each popup save data and recreates the popup.
        if (popupsSaveData != null)
            foreach (NewGalaxyPopupData popupSaveData in popupsSaveData)
                CreatePopup(popupSaveData);
    }

    /// <summary>
    /// Public method that should be called in order to create a brand new popup with the specified parameters. Method should not be used to recreate a popup from save data, the public CreatePopup() method handles that.
    /// </summary>
    /// <param name="headerText"></param>
    /// <param name="bodySpriteName"></param>
    /// <param name="bodyText"></param>
    public void CreatePopup(string headerText, string bodySpriteName, string bodyText, string openedSFXName, List<NewGalaxyPopupOptionData> options)
    {
        //Initializes the list of popups if it has not yet been initialized.
        if (popups == null)
            popups = new List<NewGalaxyPopup>();

        //Instantiates a new popup from the popup prefab and adds it to the list of popups.
        popups.Add(Instantiate(NewGalaxyPopup.prefab).GetComponent<NewGalaxyPopup>());

        //Sets the popup's parent as the transform of the popup manager.
        popups[popups.Count - 1].transform.SetParent(transform);

        //Initializes the popup by providing it its save data.
        popups[popups.Count - 1].Initialize(headerText, bodySpriteName, bodyText, openedSFXName, options, OnPopupClosed);
    }

    /// <summary>
    /// Public method that should be called in order to recreate a popup from its save data.
    /// </summary>
    /// <param name="popupData"></param>
    public void CreatePopup(NewGalaxyPopupData popupData)
    {
        //Initializes the list of popups if it has not yet been initialized.
        if (popups == null)
            popups = new List<NewGalaxyPopup>();

        //Instantiates a new popup from the popup prefab and adds it to the list of popups.
        popups.Add(Instantiate(NewGalaxyPopup.prefab).GetComponent<NewGalaxyPopup>());

        //Sets the popup's parent as the transform of the popup manager.
        popups[popups.Count - 1].transform.SetParent(transform);

        //Initializes the popup by providing it its save data.
        popups[popups.Count - 1].Initialize(popupData, OnPopupClosed);
    }

    /// <summary>
    /// Private method that should be called by a popup after it has finished closing and is about to be destroyed. Removes the specified popup from the list of popups that are active within the galaxy scene.
    /// </summary>
    /// <param name="popup"></param>
    private void OnPopupClosed(NewGalaxyPopup popup)
    {
        if (popup != null && popups != null && popups.Contains(popup))
        {
            //Removes the closed popup from the list of popups that are active within the galaxy scene.
            popups.Remove(popup);
        }
    }
}