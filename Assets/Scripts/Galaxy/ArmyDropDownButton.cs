using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyDropDownButton : MonoBehaviour
{
    public enum ArmyDropDownButtonType
    {
        ArmyDropDownButton,
        SquadChildButton
    }
    public ArmyDropDownButtonType type;

    public int index;

    public Text nameText;

    public Image arrowImage;

    public ArmyManagerScrollList scrollList;

    public bool expanded;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ClickDropDownButton()
    {
        scrollList.ClickDropDownButton(transform.GetSiblingIndex());

        if(arrowImage.transform.localEulerAngles.z == 0)
        {
            arrowImage.transform.localEulerAngles = new Vector3(0, 0, -90);
        }
        else
        {
            arrowImage.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
    }
}
