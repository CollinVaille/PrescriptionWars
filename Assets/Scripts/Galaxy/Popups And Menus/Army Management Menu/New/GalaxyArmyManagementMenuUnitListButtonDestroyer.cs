using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyArmyManagementMenuUnitListButtonDestroyer : MonoBehaviour
{
    [SerializeField, Tooltip("Indicates the amount of delay between the destroying of each unit list button.")] private float delayBetweenDestroyingButtons = 1;

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Holds all of the unit list buttons that need to be destroyed.
    /// </summary>
    private List<GalaxyArmyManagementMenuUnitListButton> unitListButtonsToDestroy = new List<GalaxyArmyManagementMenuUnitListButton>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// This method should be called in order to add a unit list button to the list of unit list buttons that need to be destroyed.
    /// </summary>
    public void AddUnitListButtonToDestroy(GalaxyArmyManagementMenuUnitListButton unitListButton)
    {
        //Sets the parent of the unit list button to be the unit list button destroyer.
        unitListButton.transform.SetParent(transform);
        //Disables the game object of the unit list button in order to prevent it from being visible to the user.
        unitListButton.gameObject.SetActive(false);
        //Adds the specified unit list button to the list of unit list buttons that need to be destroyed.
        unitListButtonsToDestroy.Add(unitListButton);
        //Starts up the process of destroying the unit list button if it hasn't been started already.
        if(unitListButtonsToDestroy.Count == 1)
        {
            StartCoroutine(DestroyAUnitListButton());
        }
    }

    /// <summary>
    /// Waits for the specified amount of seconds of delay between destroying unit list buttons and then destroys the unit list button at the first index in the list of unit list buttons that need to be destroyed and then checks if the function needs to be run again for another unit list button.
    /// </summary>
    /// <returns></returns>
    IEnumerator DestroyAUnitListButton()
    {
        //Waits for the specified amount of seconds of delay between destroying unit list buttons.
        yield return new WaitForSeconds(delayBetweenDestroyingButtons);

        //Destroys the unit list button at the first index in the list of unit list buttons that need to be destroyed and removes it from the list.
        GalaxyArmyManagementMenuUnitListButton unitListButtonToDestroy = unitListButtonsToDestroy[0];
        unitListButtonsToDestroy.RemoveAt(0);
        Destroy(unitListButtonToDestroy.gameObject);

        //Runs the coroutine again if there are more unit list buttons that need to be destroyed.
        if(unitListButtonsToDestroy.Count > 0)
        {
            StartCoroutine(DestroyAUnitListButton());
        }
    }
}