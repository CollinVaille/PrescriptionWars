using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlanetShip : MonoBehaviour
{
    /// <summary>
    /// Holds all of the planet ships.
    /// </summary>
    private static List<PlanetShip> planetShips = new List<PlanetShip>();

    public static Transform planetShipParent = null;

    public static GameObject planetShipPrefab = null;

    /// <summary>
    /// Holds all of the mesh renderers that are part of the planet ship.
    /// </summary>
    [SerializeField] private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();

    /// <summary>
    /// Tooltip that indicates to the user what army the planet ship is representing.
    /// </summary>
    public GalaxyTooltip tooltip = null;

    /// <summary>
    /// Indicates the id (index in the list of planets in the galaxy manager) of the planet that the planet ship is attached to.
    /// </summary>
    public int attachedPlanetID;

    /// <summary>
    /// Sets the shared material of all of the mesh renderers of the planet ship to the specified material.
    /// </summary>
    public Material SharedMaterial
    {
        set
        {
            foreach(MeshRenderer meshRenderer in meshRenderers)
            {
                meshRenderer.sharedMaterial = value;
            }
        }
    }

    /// <summary>
    /// Indicates whether the planet the planet ship is attached to is ruled by the player's empire.
    /// </summary>
    private bool isAttachedPlanetRuledByPlayerEmpire
    {
        get
        {
            return Empire.empires[GalaxyManager.planets[attachedPlanetID].OwnerID].IsPlayerEmpire;
        }
    }

    private void Awake()
    {
        planetShips.Add(this);
    }

    private void Start()
    {
        if (!isAttachedPlanetRuledByPlayerEmpire)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnMouseUpAsButton()
    {
        if (GalaxyCamera.IsMouseOverUIElement)
            return;

        /*
        if (ArmyManagementMenu.Menu.gameObject.activeInHierarchy)
            ArmyManagementMenu.Menu.ClearAllScrollLists();

        ArmyManagementMenu.Menu.PlanetSelected = attachedPlanetID;
        ArmyManagementMenu.Menu.Open();
        */
    }

    /// <summary>
    /// Ensures that the player's planet ships are visible while the planet ships of the bots are not visible after a player id change.
    /// </summary>
    public static void OnPlayerIDChange()
    {
        foreach(PlanetShip planetShip in planetShips)
        {
            if (planetShip.isAttachedPlanetRuledByPlayerEmpire)
                planetShip.gameObject.SetActive(true);
            else
                planetShip.gameObject.SetActive(false);
        }
    }
}
