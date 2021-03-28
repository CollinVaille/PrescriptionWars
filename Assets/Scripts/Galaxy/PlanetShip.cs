using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetShip : MonoBehaviour
{
    public int attachedPlanetID;

    private void OnMouseUpAsButton()
    {
        if (GalaxyCamera.IsMouseOverUIElement)
            return;

        if (ArmyManagementMenu.Menu.gameObject.activeInHierarchy)
            ArmyManagementMenu.Menu.ClearAllScrollLists();

        ArmyManagementMenu.Menu.PlanetSelected = attachedPlanetID;
        ArmyManagementMenu.Menu.Open();
    }
}
