﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ArmyManagementScrollListButton : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [Header("Text Components")]

    public Text nameText = null;

    [Header("Image Components")]

    [SerializeField]
    private Image buttonImage = null;
    [SerializeField]
    private Image arrowImage = null;
    [SerializeField]
    private Image transferArrowImage = null;

    [Header("Audio Options")]

    [SerializeField]
    private AudioClip clickDropdownButtonSFX = null;

    [Header("Logic Options")]

    [SerializeField]
    [Range(500, 2000)]
    private float arrowRotateSpeed = 1000;

    public enum ArmyManagementButtonType
    {
        ArmyDropDownButton,
        SquadChildButton,
        SquadDropDownButton,
        PillChildButton
    }

    [Header("Additional Information")]

    [SerializeField]
    [ReadOnly] private ArmyManagementButtonType type;

    //Non-inspector variables.
    private int arrowRotationTargetDegrees;
    private int previousDataIndex;

    [HideInInspector]
    public bool isDropdownButton;
    private bool expanded;
    private bool additionalDataChangedOnFrame;

    private ArmyManagerScrollList assignedScrollList;

    [HideInInspector]
    public Vector3 initalDragPosition;

    private List<int> additionalData = new List<int>();
    private List<ArmyManagementScrollListButton> assignedScrolllistButtons = new List<ArmyManagementScrollListButton>();

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

        additionalDataChangedOnFrame = false;
    }

    public static ArmyManagementButtonType GetChildType(ArmyManagementButtonType parentType)
    {
        switch (parentType)
        {
            case ArmyManagementButtonType.ArmyDropDownButton:
                return ArmyManagementButtonType.SquadChildButton;
            case ArmyManagementButtonType.SquadDropDownButton:
                return ArmyManagementButtonType.PillChildButton;

            default:
                return ArmyManagementButtonType.SquadChildButton;
        }
    }

    public static ArmyManagementButtonType GetParentType(ArmyManagementButtonType childType)
    {
        switch (childType)
        {
            case ArmyManagementButtonType.SquadChildButton:
                return ArmyManagementButtonType.ArmyDropDownButton;
            case ArmyManagementButtonType.PillChildButton:
                return ArmyManagementButtonType.SquadDropDownButton;

            default:
                return ArmyManagementButtonType.ArmyDropDownButton;
        }
    }

    public ArmyManagementButtonType GetButtonType()
    {
        return type;
    }

    public void SetButtonType(ArmyManagementButtonType buttonType)
    {
        type = buttonType;
    }

    public void SetAssignedScrollList(ArmyManagerScrollList assignedScrollList)
    {
        this.assignedScrollList = assignedScrollList;
    }

    public void SetButtonImageColor(Color buttonImageColor)
    {
        buttonImage.color = buttonImageColor;
    }

    public void FlipTransferArrowImageLocalScaleX()
    {
        transferArrowImage.transform.localScale = new Vector3(transferArrowImage.transform.localScale.x * -1, 1, 1);
    }

    public bool IsExpanded()
    {
        return expanded;
    }

    public void SetExpanded(bool isExpanded)
    {
        expanded = isExpanded;
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

        PlayClickDropdownButtonSFX();
    }

    void PlayClickDropdownButtonSFX()
    {
        if(clickDropdownButtonSFX != null)
            GalaxyManager.galaxyManager.sfxSource.PlayOneShot(clickDropdownButtonSFX);
    }

    public void ExecuteDropDownLogic()
    {
        assignedScrollList.ClickDropDownButton(transform.GetSiblingIndex());

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

    public void ClickTransferButton()
    {
        if(type == ArmyManagementButtonType.SquadChildButton)
        {
            if(assignedScrolllistButtons.Count <= 0)
                ArmyManagementMenu.Menu.AddNewSquadDropdownButton(GetParentDataIndex(), GetDataIndex(), this);
            else
            {
                List<ArmyManagementScrollListButton> childButtons = assignedScrolllistButtons[0].assignedScrollList.GetChildButtons(assignedScrolllistButtons[0]);
                foreach(ArmyManagementScrollListButton childButton in childButtons)
                {
                    Destroy(childButton.gameObject);
                }
                Destroy(assignedScrolllistButtons[0].gameObject);
                assignedScrolllistButtons.Clear();
            }

            transferArrowImage.transform.localScale = new Vector3(transferArrowImage.transform.localScale.x * -1, 1, 1);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        initalDragPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = new Vector3(transform.position.x, Input.mousePosition.y, transform.position.z);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //transform.position = new Vector3(initialDragXPosition, transform.position.y, transform.position.z);
        assignedScrollList.ButtonEndDrag(this);
    }

    public int GetParentDataIndex()
    {
        if(type == ArmyManagementButtonType.SquadDropDownButton)
        {
            return additionalData[0];
        }
        if(GetParentType(type) == ArmyManagementButtonType.SquadDropDownButton)
        {
            return transform.parent.GetChild(GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetDataIndex();
        }

        if (isDropdownButton)
        {
            Debug.Log("Something went wrong. You are attempting to get the parent button data index for a dropdown button that does not have a button parent.");
            return -1;
        }

        int parentButtonDataIndex = -1;
        for (int x = 0; x < transform.parent.childCount; x++)
        {
            ArmyManagementScrollListButton scrollListButton = transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();

            if (scrollListButton == this)
                return parentButtonDataIndex;

            if (scrollListButton.type == GetParentType(type))
                parentButtonDataIndex++;
        }

        return parentButtonDataIndex;
    }

    public int GetDataIndex()
    {
        if (isDropdownButton)
        {
            if(type != ArmyManagementButtonType.SquadDropDownButton)
            {
                int dataIndex = 0;

                for (int x = 0; x < transform.parent.childCount; x++)
                {
                    ArmyManagementScrollListButton scrollListButton = transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>();

                    if (scrollListButton == this)
                        return dataIndex;

                    if (scrollListButton.type == type)
                        dataIndex++;
                }
            }
            else
            {
                return additionalData[1];
            }
        }

        int parentSiblingIndex = 0;

        for (int x = transform.GetSiblingIndex() - 1; x >= 0; x--)
        {
            if (transform.parent.GetChild(x).GetComponent<ArmyManagementScrollListButton>().type == GetParentType(type))
            {
                parentSiblingIndex = x;
                break;
            }
        }

        return transform.GetSiblingIndex() - parentSiblingIndex - 1;
    }

    public int GetParentSiblingIndex()
    {
        if (isDropdownButton)
        {
            Debug.Log("Something has gone wrong, cannot get the parent sibling index of a dropdown button which has no parent button.");
            return 0;
        }

        for(int index = transform.GetSiblingIndex() - 1; index >= 0; index--)
        {
            if(transform.parent.GetChild(index).GetComponent<ArmyManagementScrollListButton>().type == GetParentType(type))
            {
                return index;
            }
        }

        Debug.Log("Something has gone wrong, there is no parent button to get the sibling index for.");
        return 0;
    }

    public void Click()
    {
        switch (type)
        {
            case ArmyManagementButtonType.ArmyDropDownButton:
                ArmyManagementMenu.Menu.SetArmySelected(GalaxyManager.planets[ArmyManagementMenu.Menu.PlanetSelected].Armies[GetDataIndex()]);
                break;
            case ArmyManagementButtonType.SquadChildButton:
                ArmyManagementMenu.Menu.SetSquadSelected(GalaxyManager.planets[ArmyManagementMenu.Menu.PlanetSelected].Armies[GetParentDataIndex()].GetSquadAt(GetDataIndex()));
                break;
            case ArmyManagementButtonType.SquadDropDownButton:
                ArmyManagementMenu.Menu.SetSquadSelected(GalaxyManager.planets[ArmyManagementMenu.Menu.PlanetSelected].Armies[GetParentDataIndex()].GetSquadAt(GetDataIndex()));
                break;
            case ArmyManagementButtonType.PillChildButton:
                ArmyManagementMenu.Menu.SetPillSelected(GalaxyManager.planets[ArmyManagementMenu.Menu.PlanetSelected].Armies[transform.parent.GetChild(GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex()].GetSquadAt(GetParentDataIndex()).GetPillAt(GetDataIndex()));
                break;
        }
    }

    public void AddAdditionalData(int data)
    {
        additionalData.Add(data);
    }

    public void SetAdditionalData(int index, int data)
    {
        if(index < additionalData.Count)
        {
            additionalData[index] = data;
            additionalDataChangedOnFrame = true;
        }
    }

    public void AddAssignedScrolllistButton(ArmyManagementScrollListButton scrollListButton)
    {
        assignedScrolllistButtons.Add(scrollListButton);
    }

    public void SiblingIndexUpdate()
    {
        switch (type)
        {
            case ArmyManagementButtonType.SquadChildButton:
                if(assignedScrolllistButtons.Count > 0)
                {
                    assignedScrolllistButtons[0].SetAdditionalData(0, GetParentDataIndex());
                    assignedScrolllistButtons[0].SetAdditionalData(1, GetDataIndex());
                }
                break;
            case ArmyManagementButtonType.ArmyDropDownButton:
                if (assignedScrolllistButtons.Count > 0)
                {
                    foreach (ArmyManagementScrollListButton scrollListButton in assignedScrolllistButtons)
                    {
                        scrollListButton.SetAdditionalData(0, GetDataIndex());
                    }

                    assignedScrolllistButtons.Clear();
                }
                break;

            default:
                break;
        }
    }

    public void SiblingIndexUpdateOccuringNextFrame()
    {
        previousDataIndex = GetDataIndex();

        if(type == ArmyManagementButtonType.ArmyDropDownButton)
        {
            if (!expanded)
            {
                assignedScrolllistButtons = ArmyManagementMenu.Menu.GetSquadDropdownButtonsWithParentDataIndex(GetDataIndex());
                //Debug.Log(assignedScrolllistButtons.Count);
            }
        }
    }

    public bool HasAdditionalDataChangedOnFrame()
    {
        return additionalDataChangedOnFrame;
    }
}
