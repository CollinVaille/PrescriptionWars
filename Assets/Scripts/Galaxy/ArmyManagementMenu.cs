using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArmyManagementMenu : MonoBehaviour
{
    public Image backgroundColorImage;

    public Text planetNameText;

    public int planetSelected;

    public static ArmyManagementMenu armyManagementMenu;

    bool beingMoved = false;
    Vector2 mouseToMenuDistance;

    public List<ArmyManagerScrollList> armyManagerScrollLists;

    public List<Image> scrollBarHandleImages;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && transform.GetSiblingIndex() == transform.parent.childCount - 1 && !GalaxyManager.popupClosedOnFrame && !GalaxyConfirmationPopup.IsAGalaxyConfirmationPopupOpen())
        {
            CloseMenu();
        }

        //Deals with the army management menu being dragged.
        if (beingMoved)
        {
            transform.position = new Vector2(Input.mousePosition.x - mouseToMenuDistance.x, Input.mousePosition.y - mouseToMenuDistance.y);

            //Left barrier.
            if (transform.localPosition.x < -175)
            {
                transform.localPosition = new Vector2(-175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x < GalaxyManager.galaxyCamera.pixelWidth * (-.2806122449f))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * (-.2806122449f);
            }
            //Right barrier.
            if (transform.localPosition.x > 175)
            {
                transform.localPosition = new Vector2(175, transform.localPosition.y);

                mouseToMenuDistance.x = Input.mousePosition.x - transform.position.x;

                if (mouseToMenuDistance.x > GalaxyManager.galaxyCamera.pixelWidth * (.2806122449f))
                    mouseToMenuDistance.x = GalaxyManager.galaxyCamera.pixelWidth * (.2806122449f);
            }
            //Top barrier.
            if (transform.localPosition.y > 30)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, 30);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y > GalaxyManager.galaxyCamera.pixelHeight * (.3625229798f))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * (.3625229798f);
            }
            //Bottom barrier.
            if (transform.localPosition.y < -62)
            {
                transform.localPosition = new Vector2(transform.localPosition.x, -62);

                mouseToMenuDistance.y = Input.mousePosition.y - transform.position.y;

                if (mouseToMenuDistance.y < GalaxyManager.galaxyCamera.pixelHeight * (-.3625229798f))
                    mouseToMenuDistance.y = GalaxyManager.galaxyCamera.pixelHeight * (-.3625229798f);
            }
        }

        //Brings the army management menu above all of the other pop-ups if it is being pressed on.
        if (GalaxyCamera.mouseOverArmyManagementMenu && Input.GetMouseButtonDown(0))
            transform.SetAsLastSibling();
    }

    public void OpenMenu()
    {
        //Test code.
        GalaxyPill bob = new GalaxyPill();
        bob.name = "Bob";
        bob.pillClass = GalaxyPill.PillClass.Assault;
        GalaxySquad deltaSquad = new GalaxySquad();
        deltaSquad.name = "Delta Squad";
        deltaSquad.pills.Add(bob);
        GalaxyArmy armyOfTheSouth = new GalaxyArmy();
        armyOfTheSouth.name = "Army " + (GalaxyManager.planets[planetSelected].armies.Count + 1);
        armyOfTheSouth.squads.Add(deltaSquad);
        GalaxyManager.planets[planetSelected].armies.Add(armyOfTheSouth);

        //Activates the army mangagement menu gameobject.
        transform.gameObject.SetActive(true);
        //Brings the army management menu on top of all of the other pop-ups.
        transform.SetAsLastSibling();

        //Sets the color of the menu's foreground background image to the player empire's color.
        backgroundColorImage.color = Empire.empires[GalaxyManager.playerID].empireColor;

        //Sets the color of every scroll bar handle to the player empire's color.
        foreach(Image scrollBarHandleImage in scrollBarHandleImages)
        {
            scrollBarHandleImage.color = Empire.empires[GalaxyManager.playerID].empireColor;
        }

        //Sets the planet name text at the top of the menu to the name of the planet that the player has selected to manage the armies on.
        planetNameText.text = GalaxyManager.planets[planetSelected].nameLabel.text;

        //Populates each scroll list with what information they need to display.
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.PopulateScrollList();
        }
    }

    public void CloseMenu()
    {
        //Logs with the galaxy manager that a pop-up has been closed on this frame (so that other pop-ups are not allowed to close with the escape key on the same frame).
        GalaxyManager.popupClosedOnFrame = true;

        //Puts the army management menu at the top of the pop-ups gameobject (least priority).
        transform.SetSiblingIndex(0);

        //Deactivates the whole army management menu gameobject.
        transform.gameObject.SetActive(false);

        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }
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

    public void ClearAllScrollLists()
    {
        foreach(ArmyManagerScrollList scrollList in armyManagerScrollLists)
        {
            scrollList.ClearScrollList();
        }
    }
}
