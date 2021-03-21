using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShip : MonoBehaviour
{
    public int attachedPlanetID;

    private void OnMouseUpAsButton()
    {
        if (!GalaxyCamera.GetMouseOverUIElement())
        {
            if (ArmyManagementMenu.Menu.gameObject.activeInHierarchy)
                ArmyManagementMenu.Menu.ClearAllScrollLists();

            ArmyManagementMenu.Menu.PlanetSelected = attachedPlanetID;
            ArmyManagementMenu.Menu.Open();
        }
    }
}
