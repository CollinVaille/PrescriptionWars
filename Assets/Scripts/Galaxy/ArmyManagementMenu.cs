using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : MonoBehaviour
{
    public Image backgroundColorImage;

    public int planetSelected;

    public static ArmyManagementMenu armyManagementMenu;

    bool beingMoved = false;
    Vector2 mouseToMenuDistance;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.childCount - 2)
        {
            CloseMenu();
        }

        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -175)
            {
                transform.localPosition = new Vector2(-175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < -440)
                    mouseToMenuDistance.x = -440;
            }
            //Right barrier.
            if (transform.localPosition.x > 175)
            {
                transform.localPosition = new Vector2(175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > 440)
                    mouseToMenuDistance.x = 440;
            }
            //Top barrier.
            if (transform.localPosition.y > 30)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 30);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > 323)
                    mouseToMenuDistance.y = 323;
            }
            //Bottom barrier.
            if (transform.localPosition.y < -62)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -62);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < -323)
                    mouseToMenuDistance.y = -323;
            }
        }
    }

    public void OpenMenu()
    {
        transform.gameObject.SetActive(true);

        backgroundColorImage.color = Empire.empires[GalaxyManager.playerID].empireColor;
    }

    public void CloseMenu()
    {
        transform.gameObject.SetActive(false);
    }

    public void PointerDownArmyManagementMenu()
    {
        //Tells the update function that the player is dragging the menu.
        beingMoved = true;

        //Tells the update function the set difference between the mouse position and the menu's position.
        mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;
        mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;
    }

    public void PointerUpArmyManagementMenu()
    {
        //Tells the update function that the player is no longer dragging the menu.
        beingMoved = false;

        //Resets the vector that says the difference between the mouse position and the menu's position.
        mouseToMenuDistance = Vector2.zero;
    }

    public void BringToTopInHierarchy()
    {
        transform.SetAsLastSibling();
        GalaxyManager.galaxyManager.endTurnButton.transform.SetAsLastSibling();
    }
}
