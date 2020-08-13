﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShip : MonoBehaviour
{
    public int attachedPlanetID;

    private void OnMouseUpAsButton()
    {
        if (!GalaxyCamera.mouseOverArmyManagementMenu && !GalaxyCamera.mouseOverPlanetManagementMenu)
        {
            ArmyManagementMenu.armyManagementMenu.planetSelected = attachedPlanetID;
            ArmyManagementMenu.armyManagementMenu.OpenMenu();
        }
    }
}