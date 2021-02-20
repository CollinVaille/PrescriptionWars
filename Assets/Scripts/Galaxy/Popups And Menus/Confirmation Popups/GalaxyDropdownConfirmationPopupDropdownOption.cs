using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GalaxyDropdownConfirmationPopupDropdownOption : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [SerializeField]
    private GalaxyDropdownConfirmationPopup dropdownConfirmationPopup = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(dropdownConfirmationPopup != null)
            dropdownConfirmationPopup.MouseOverDropdownOption();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(dropdownConfirmationPopup != null)
            dropdownConfirmationPopup.ClickDropdownOption();
    }
}
