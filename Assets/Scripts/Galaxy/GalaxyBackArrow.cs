using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyBackArrow : MonoBehaviour
{
    [Header("Options")]

    [SerializeField]
    [Tooltip("The sound effect that will be played whenever the back arrow is clicked.")]
    private AudioClip backArrowClickSFX = null;

    [Header("Additional Information")]

    [SerializeField]
    [ReadOnly] private GalaxyMenuBehaviour assignedMenu = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAssignedMenu(GalaxyMenuBehaviour menu)
    {
        assignedMenu = menu;
    }

    //This method is called whenever the back arrow is clicked and tells the menu that it is assigned to that it must switch to the previous menu.
    public void OnBackArrowClick()
    {
        //Indicates to the menu that the back arrow is assigned to that it should switch to the previous menu.
        assignedMenu.SwitchToPreviousMenu();

        //Plays the sound effect for clicking the back arrow.
        if (backArrowClickSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(backArrowClickSFX);
    }
}
