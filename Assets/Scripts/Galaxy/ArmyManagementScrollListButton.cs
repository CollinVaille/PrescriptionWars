using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementScrollListButton : MonoBehaviour
{
    public enum ArmyDropDownButtonType
    {
        ArmyDropDownButton,
        SquadChildButton
    }
    public ArmyDropDownButtonType type;

    public int index;

    public Text nameText;

    public Image buttonImage;
    public Image arrowImage;

    public ArmyManagerScrollList scrollList;

    public float arrowRotateSpeed;
    public int arrowRotationTargetDegrees;

    public bool expanded;
    public bool isDropdownButton;

    public Vector3 initalDragPosition;

    // Start is called before the first frame update
    void Start()
    {
        DetermineIfDropdownButton();
    }

    // Update is called once per frame
    void Update()
    {
        if(isDropdownButton)
            UpdateArrowRotationToTargetDegrees();
    }

    public static ArmyDropDownButtonType GetChildType(ArmyDropDownButtonType parentType)
    {
        switch (parentType)
        {
            case ArmyDropDownButtonType.ArmyDropDownButton:
                return ArmyDropDownButtonType.SquadChildButton;

            default:
                return ArmyDropDownButtonType.SquadChildButton;
        }
    }

    public static ArmyDropDownButtonType GetParentType(ArmyDropDownButtonType childType)
    {
        switch (childType)
        {
            case ArmyDropDownButtonType.SquadChildButton:
                return ArmyDropDownButtonType.ArmyDropDownButton;

            default:
                return ArmyDropDownButtonType.ArmyDropDownButton;
        }
    }

    void UpdateArrowRotationToTargetDegrees()
    {
        if(arrowRotationTargetDegrees == 0)
        {
            arrowImage.transform.localEulerAngles = new Vector3(arrowImage.transform.localEulerAngles.x, arrowImage.transform.localEulerAngles.y, arrowImage.transform.localEulerAngles.z + (arrowRotateSpeed * Time.deltaTime));

            if(arrowImage.transform.localEulerAngles.z < 270)
            {
                arrowImage.transform.localEulerAngles = new Vector3(arrowImage.transform.localEulerAngles.x, arrowImage.transform.localEulerAngles.y, 0);
            }
        }
        else
        {
            arrowImage.transform.localEulerAngles = new Vector3(arrowImage.transform.localEulerAngles.x, arrowImage.transform.localEulerAngles.y, arrowImage.transform.localEulerAngles.z - (arrowRotateSpeed * Time.deltaTime));

            if(arrowImage.transform.localEulerAngles.z < 270)
            {
                arrowImage.transform.localEulerAngles = new Vector3(arrowImage.transform.localEulerAngles.x, arrowImage.transform.localEulerAngles.y, 270);
            }
        }
    }

    public void ClickDropDownButton()
    {
        ExecuteDropDownLogic();
    }

    public void ExecuteDropDownLogic()
    {
        scrollList.ClickDropDownButton(transform.GetSiblingIndex());

        if (arrowRotationTargetDegrees == 0)
            arrowRotationTargetDegrees = 270;
        else
            arrowRotationTargetDegrees = 0;
    }

    public void DetermineIfDropdownButton()
    {
        isDropdownButton = false;
        if (type.ToString().ToLower().Contains("dropdown"))
            isDropdownButton = true;
    }

    public void ButtonBeginDrag()
    {
        initalDragPosition = transform.position;
    }

    public void DragButton()
    {
        transform.position = Input.mousePosition;
    }

    public void ButtonEndDrag()
    {
        //transform.position = new Vector3(initialDragXPosition, transform.position.y, transform.position.z);
        scrollList.ButtonEndDrag(this);
    }
}
