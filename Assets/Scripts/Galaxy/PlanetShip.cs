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
            if (ArmyManagementMenu.armyManagementMenu.gameObject.activeInHierarchy)
                ArmyManagementMenu.armyManagementMenu.ClearAllScrollLists();

            ArmyManagementMenu.armyManagementMenu.planetSelected = attachedPlanetID;
            ArmyManagementMenu.armyManagementMenu.Open();
        }
    }
}
