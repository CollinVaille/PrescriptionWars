using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class GalaxyCloseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, ISelectHandler
{
    [Header("SFX Options")]

    [SerializeField] private AudioClip pointerEnterCloseButtonSFX = null;

    [Header("Text Components")]

    [SerializeField] private Text closeButtonText = null;

    //Non-inspector variables.

    private bool pointerDownOnCloseButton = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Called using an event trigger interface, sets the color of the text of the close button to white and plays the pointer enter close button sound effect.
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        //Sets the color of the text of the close button to white.
        closeButtonText.color = Color.white;

        //Plays the pointer enter close button sound effect.
        AudioManager.PlaySFX(pointerEnterCloseButtonSFX);
    }

    /// <summary>
    /// Called using an event trigger interface, sets the color of the text of the close button back to black if the pointer isn't down on the close button.
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        //Sets the color of the text of the close button back to black.
        closeButtonText.color = pointerDownOnCloseButton ? Color.white : Color.black;
    }

    /// <summary>
    /// Called using an event trigger interface, logs through a bool that the pointer went down on the close button.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        //Logs through a bool that the pointer went down on the close button.
        pointerDownOnCloseButton = true;
    }

    /// <summary>
    /// Called using an event trigger interface, logs through a bool that the pointer is no longer down on the close button and resets the color of the close button's text back to black.
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        //Logs through a bool that the pointer is no longer down on the close button.
        pointerDownOnCloseButton = false;

        //Resets the color of the close button's text back to white.
        closeButtonText.color = Color.black;
    }

    /// <summary>
    /// Called using an event trigger interface, deselects the button.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnSelect(BaseEventData eventData)
    {
        Button buttonComponent = gameObject.GetComponent<Button>();
        if (buttonComponent != null)
            buttonComponent.OnDeselect(eventData);
    }
}
