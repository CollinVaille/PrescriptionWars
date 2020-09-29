using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GloriousUIConnector : MonoBehaviour, IPointerEnterHandler
{
    //Need to know which type of UI element we're connecting, since code changes based on that
    public enum UIElementType { Button, Toggle }

    public UIElementType elementType = UIElementType.Button;

    //Connect "Click" event to menu script
    private void Start()
    {
        if(elementType == UIElementType.Button)
            GetComponent<Button>().onClick.AddListener(() => PlanetPauseMenu.pauseMenu.OnButtonClick(transform));
        else if(elementType == UIElementType.Toggle)
            GetComponent<Toggle>().onValueChanged.AddListener((bool ignored) => PlanetPauseMenu.pauseMenu.OnToggleClicked(transform));
    }

    //Connect "Hover Over" event to menu script
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (elementType == UIElementType.Button)
            PlanetPauseMenu.pauseMenu.OnButtonMouseOver(transform);
        else if (elementType == UIElementType.Toggle)
            PlanetPauseMenu.pauseMenu.OnToggleMouseOver(transform);
    }
}
