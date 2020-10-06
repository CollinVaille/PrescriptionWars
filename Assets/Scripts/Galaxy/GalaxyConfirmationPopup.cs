using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyConfirmationPopup : MonoBehaviour
{
    public Text topText;
    public Text bodyText;

    public AudioClip mouseOverSFX;
    public AudioClip mouseClickSFX;

    public enum GalaxyConfirmationPopupTask
    {
        DemolishBuilding,
        CancelBuildingQueued
    }
    public static GalaxyConfirmationPopupTask task;

    public static GalaxyConfirmationPopup galaxyConfirmationPopup;

    public static int planetID, cityID, buildingIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //If the user presses escape while the pop-up is active, then it will cancel whatever action it was querying the user about.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
        //If the user presses return (enter) or keypad eneter while the pop-up is active, then it will confirm whatever action it was querying the user about and thus carry it out.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Confirm();
        }
    }

    public static void NewDemolishBuildingConfirmation(int IDOfPlanet, int IDOfCity, int indexOfBuilding)
    {
        //Tells the confirmation pop-up that it is querying the user on whether or not they want to demolish a building.
        task = GalaxyConfirmationPopupTask.DemolishBuilding;

        //Tells the confirmation pop-up what planet the player is wanting to demolish a building on, the city the player is wanting to demolish the building in, and what index of the buildings completed list the building that they want to remove is (can also be used to get the building's type).
        planetID = IDOfPlanet;
        cityID = IDOfCity;
        buildingIndex = indexOfBuilding;

        //Sets the top text of the pop-up to tell the user what the pop-up is generally querying them about.
        galaxyConfirmationPopup.topText.text = GeneralHelperMethods.GetEnumText(task.ToString());
        //Sets the body text of the pop-up to tell that user what exactly the pop-up is querying them about (gives more details than the top text).
        galaxyConfirmationPopup.bodyText.text = GetBodyText(task);

        //Activates the confirmation pop-up gameobject.
        galaxyConfirmationPopup.transform.gameObject.SetActive(true);
    }

    public static void NewCancelBuildingQueuedConfirmation(int IDOfPlanet, int IDOfCity, int indexOfBuilding)
    {
        //Tells the confirmation pop-up that it is querying the user on whether or not they want to cencel a building queued.
        task = GalaxyConfirmationPopupTask.CancelBuildingQueued;

        //Tells the confirmation pop-up what planet the player is wanting to cancel a building queued on, the city the player is wanting to demolish the building in, and what index of the buildings completed list the building that they want to remove is (can also be used to get the building's type).
        planetID = IDOfPlanet;
        cityID = IDOfCity;
        buildingIndex = indexOfBuilding;

        //Sets the top text of the pop-up to tell the user what the pop-up is generally querying them about.
        galaxyConfirmationPopup.topText.text = GeneralHelperMethods.GetEnumText(task.ToString());
        //Sets the body text of the pop-up to tell that user what exactly the pop-up is querying them about (gives more details than the top text).
        galaxyConfirmationPopup.bodyText.text = GetBodyText(task);

        //Activates the confirmation pop-up gameobject.
        galaxyConfirmationPopup.transform.gameObject.SetActive(true);
    }

    //Returns the appropriate body text for the pop-up depending on what the pop-ups new task exactly is.
    public static string GetBodyText(GalaxyConfirmationPopupTask popupTask)
    {
        switch (popupTask)
        {
            case GalaxyConfirmationPopupTask.DemolishBuilding:
                return "Are you sure that you want to demolish a " + GeneralHelperMethods.GetEnumText(GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().cities[cityID].buildingsCompleted[buildingIndex].type.ToString()).ToLower() + " in " + GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().cities[cityID].cityName + ", " + GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().nameLabel.text + "?";
            case GalaxyConfirmationPopupTask.CancelBuildingQueued:
                return "Are you sure that you want to cancel the construction of a " + GeneralHelperMethods.GetEnumText(GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().cities[cityID].buildingQueue.buildingsQueued[buildingIndex].type.ToString()).ToLower() + " in " + GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().cities[cityID].cityName + ", " + GalaxyManager.planets[planetID].GetComponent<PlanetIcon>().nameLabel.text + "?";

            default:
                return "No Valid Body Text.";
        }
    }

    public void Confirm()
    {
        //Finally carries out what the user wanted to do depending on the task they gave.
        switch (task)
        {
            case GalaxyConfirmationPopupTask.DemolishBuilding:
                PlanetManagementMenu.planetManagementMenu.DemolishBuilding(buildingIndex);
                break;
            case GalaxyConfirmationPopupTask.CancelBuildingQueued:
                PlanetManagementMenu.planetManagementMenu.CancelBuildingQueued(buildingIndex);
                break;
        }

        //Deactivates the galaxy confirmation pop-up gameobject.
        transform.gameObject.SetActive(false);
    }

    public void Cancel()
    {
        //Deactivates the galaxy confirmation pop-up gameobject.
        transform.gameObject.SetActive(false);
    }

    public void PlayMouseOverSFX()
    {
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverSFX);
    }

    public void PlayMouseClickSFX()
    {
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseClickSFX);
    }
}
