using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementScrollListButton : MonoBehaviour
{
    public enum ArmyManagementButtonType
    {
        ArmyDropDownButton,
        SquadChildButton,
        SquadDropDownButton,
        PillChildButton
    }
    public ArmyManagementButtonType type;

    public AudioClip clickDropdownButtonSFX;

    public Text nameText;

    public Image buttonImage;
    public Image arrowImage;
    public Image transferArrowImage;

    public ArmyManagerScrollList scrollList;

    public float arrowRotateSpeed;
    public int arrowRotationTargetDegrees;
    int previousDataIndex;

    public bool expanded;
    public bool isDropdownButton;
    bool additionalDataChangedOnFrame;

    public Vector3 initalDragPosition;

    List<int> additionalData = new List<int>();
    List<ArmyManagementScrollListButton> assignedScrolllistButtons = new List<ArmyManagementScrollListButton>();

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

    public void ClickTransferButton()
    {
        if(type == ArmyManagementButtonType.SquadChildButton)
        {
            if(assignedScrolllistButtons.Count <= 0)
                ArmyManagementMenu.armyManagementMenu.AddNewSquadDropdownButton(GetParentDataIndex(), GetDataIndex(), this);
            else
            {
                List<ArmyManagementScrollListButton> childButtons = assignedScrolllistButtons[0].scrollList.GetChildButtons(assignedScrolllistButtons[0]);
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

    public void ButtonBeginDrag()
    {
        initalDragPosition = transform.position;
    }

    public void DragButton()
    {
        transform.position = new Vector3(transform.position.x, Input.mousePosition.y, transform.position.z);
    }

    public void ButtonEndDrag()
    {
        //transform.position = new Vector3(initialDragXPosition, transform.position.y, transform.position.z);
        scrollList.ButtonEndDrag(this);
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
                ArmyManagementMenu.armyManagementMenu.SetArmySelected(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[GetDataIndex()]);
                break;
            case ArmyManagementButtonType.SquadChildButton:
                ArmyManagementMenu.armyManagementMenu.SetSquadSelected(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[GetParentDataIndex()].squads[GetDataIndex()]);
                break;
            case ArmyManagementButtonType.SquadDropDownButton:
                ArmyManagementMenu.armyManagementMenu.SetSquadSelected(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[GetParentDataIndex()].squads[GetDataIndex()]);
                break;
            case ArmyManagementButtonType.PillChildButton:
                ArmyManagementMenu.armyManagementMenu.SetPillSelected(GalaxyManager.planets[ArmyManagementMenu.armyManagementMenu.planetSelected].armies[transform.parent.GetChild(GetParentSiblingIndex()).GetComponent<ArmyManagementScrollListButton>().GetParentDataIndex()].squads[GetParentDataIndex()].pills[GetDataIndex()]);
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
                assignedScrolllistButtons = ArmyManagementMenu.armyManagementMenu.GetSquadDropdownButtonsWithParentDataIndex(GetDataIndex());
                //Debug.Log(assignedScrolllistButtons.Count);
            }
        }
    }

    public bool HasAdditionalDataChangedOnFrame()
    {
        return additionalDataChangedOnFrame;
    }
}
