using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyMenuBehaviour : MonoBehaviour
{
    [Header("Galaxy Menu Base Options")]

    [SerializeField]
    [Tooltip("The menu that will be switched to whenever the back arrow is pressed.")]
    private GameObject previousMenu = null;

    [SerializeField]
    [Tooltip("The menu (game object) that will deactivates upon switching to the previous menu.")]
    private GameObject currentMenu = null;

    //Non-inspector variables.

    //The prefab that the back arrow will be instantiated from (assigned through the galaxy generator class).
    public static GameObject backArrowPrefab;

    private GalaxyBackArrow backArrow = null;

    public virtual void Start()
    {
        //Creates the back arrow of the menu if the menu has a previous menu to go back to.
        if (previousMenu != null)
        {
            CreateBackArrow();
        }
    }

    public virtual void Awake()
    {

    }

    public virtual void Update()
    {
        //Switches to the previous menu if the menu should close due to the player pressing the escape key.
        if (ShouldCloseDueToEscape())
        {
            SwitchToPreviousMenu();
        }
    }

    //This method creates the actual back arrow of the menu.
    private void CreateBackArrow()
    {
        //Instantiates the new back arrow from the back arrow prefab.
        GameObject newBackArrow = Instantiate(backArrowPrefab);
        //Gets the back arrow script component from the newly created back arrow game object and assigns it to the back arrow variable of the menu.
        backArrow = newBackArrow.GetComponent<GalaxyBackArrow>();
        //Sets the transform of the menu to be the parent of the back arrow.
        backArrow.transform.SetParent(transform);
        //Indicates to the back arrow that it belongs to this menu.
        backArrow.SetAssignedMenu(this);
        //Resets the scale of the back arrow to ensure that there is no unity funny business going on.
        backArrow.transform.localScale = backArrowPrefab.transform.localScale;
        //Resets the position of the back arrow to the position of the prefab in order to ensure that there is no unity funny business going on.
        backArrow.transform.localPosition = backArrowPrefab.transform.localPosition;
    }

    //This method activates the game object of the previous menu and then de-activates the game object of the current menu.
    public virtual void SwitchToPreviousMenu()
    {
        //Activates the game object of the previous menu.
        previousMenu.SetActive(true);

        //De-activates the game object of the current menu.
        currentMenu.SetActive(false);
    }

    //Indicates whether the menu should close (or switch to the previous menu) when the player presses escape.
    public virtual bool ShouldCloseDueToEscape()
    {
        return previousMenu != null && Input.GetKeyDown(KeyCode.Escape) && !GalaxyPopupBehaviour.IsAPopupActiveInHierarchy;
    }

    //Returns the previous menu.
    public GameObject GetPreviousMenu()
    {
        return previousMenu;
    }
}
