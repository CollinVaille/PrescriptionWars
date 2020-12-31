using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyTooltip : MonoBehaviour
{
    public enum TooltipClosingType
    {
        Deactivate,     //The tooltip game object is not destroyed, but rather still in the background and deactivated (More Processor Friendly).
        Destroy     //The tooltip game object is not in the background and deactivated, but rather completely destroyed (More Memory Friendly).
    }
    //Indicates how the tooltip will close (see the enum definition above to get more details on the closing types).
    public TooltipClosingType closingType;

    //The prefab that the tooltip game object will be instantiated from (assigned a value in the start method of the galaxy generator).
    public static GameObject tooltipPrefab;
    //The actual game object of the tooltip.
    GameObject tooltip;

    //The parent transform of the tooltip (must be set through the SetTooltipParent() method if being set through code, it is only public in order to allow it to be set through the inspector).
    public Transform tooltipParent;

    //The text that the tooltip displays to the user (must be set through the SetTooltipText() method if being set through code, it is only public in order to allow it to be set through the inspector).
    public string tooltipText;

    //Indicates whether the tooltip is open or not.
    bool tooltipOpen;

    // Start is called before the first frame update
    void Start()
    {
        //Creates the tooltip game object and puts it in the background if the tooltip game object will not be destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
        {
            CreateTooltip();
            tooltip.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Updates the position of the tooltip to the position of the player's mouse if the tooltip is open.
        if (tooltipOpen)
            tooltip.transform.position = Input.mousePosition;
    }

    //Opens the tooltip whenever the mouse/pointer enters the trigger zone (must be called through the Unity event trigger system).
    public void PointerEnter()
    {
        OpenTooltip();
    }

    //Closes the tooltip whenever the mouse/pointer exits the trigger zone (must be called through the Unity event trigger system, except for when being called by the OnDisable() method of this class).
    public void PointerExit()
    {
        CloseTooltip();
    }

    //Instantiates the tooltip game object from the tooltip prefab, sets the parent of the tooltip, resets essential transfrom components, and updates the text of the tooltip.
    void CreateTooltip()
    {
        //Instantiates the tooltip game object from the tooltip prefab.
        tooltip = Instantiate(tooltipPrefab);
        //Sets the parent of the tooltip.
        if(tooltipParent != null)
            tooltip.transform.SetParent(tooltipParent);
        //Sets the name of the new tooltip game object.
        tooltip.gameObject.name = gameObject.name + " Tooltip";
        //Resets the scale and position of the tooltip.
        tooltip.transform.localScale = Vector3.one;
        tooltip.transform.position = Vector3.zero;
        //Updates the text of the tooltip.
        UpdateTooltipText();
    }

    //Opens the tooltip.
    void OpenTooltip()
    {
        //Activates the tooltip's game object if it is not destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
            tooltip.SetActive(true);
        //Creates a whole new tooltip again if it is destroyed every time the tooltip closes.
        else if (closingType == TooltipClosingType.Destroy)
            CreateTooltip();

        //Indicates that the tooltip is open.
        tooltipOpen = true;
    }

    //Closes the tooltip.
    public void CloseTooltip()
    {
        //Deactivates the tooltip's game object if it is not destroyed every time the tooltip closes.
        if (closingType == TooltipClosingType.Deactivate)
            tooltip.SetActive(false);
        //Destroys the tooltip game object if it is supposed to be destroyed every time the tooltip closes.
        else if (closingType == TooltipClosingType.Destroy)
            Destroy(tooltip);

        //Indicates that the tooltip is closed.
        tooltipOpen = false;
    }

    //Updates the text element of the tooltip and makes sure that the width of the background image adjusts to match the width of the text.
    void UpdateTooltipText()
    {
        tooltip.transform.GetChild(1).GetComponent<Text>().text = tooltipText;
        tooltip.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta = new Vector2(tooltip.transform.GetChild(1).GetComponent<Text>().preferredWidth + 5, tooltip.transform.GetChild(0).GetComponent<Image>().rectTransform.sizeDelta.y);
    }

    //Should be called in order to set the tooltip text through code.
    public void SetTooltipText(string newTooltipText)
    {
        tooltipText = newTooltipText;
        if(tooltipOpen)
            UpdateTooltipText();
    }

    //Should be called in order to set the tooltip's parent through code.
    public void SetTooltipParent(Transform newTooltipParent)
    {
        tooltipParent = newTooltipParent;
        if(tooltipOpen)
            tooltip.transform.SetParent(tooltipParent);
    }

    //Closes the tooltip if the trigger zone is deactivated/disabled.
    private void OnDisable()
    {
        CloseTooltip();
    }
}
