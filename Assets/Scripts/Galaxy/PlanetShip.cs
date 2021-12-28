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
    /// Specifies the positions that the planet ship can be located at in terms of how far offset from the planet.
    /// </summary>
    [SerializeField] private Vector3[] offsetPositions = null;
    public Vector3[] OffsetPositions
    {
        get
        {
            return offsetPositions;
        }
    }

    /// <summary>
    /// Tooltip that indicates to the user what army the planet ship is representing.
    /// </summary>
    private GalaxyTooltip tooltip = null;

    /// <summary>
    /// Indicates the id (index in the list of planets in the galaxy manager) of the planet that the planet ship is attached to.
    /// </summary>
    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int attachedPlanetID;

    /// <summary>
    /// Indicates the index of the army in the attached planet's list of armies that the planet ship is representing.
    /// </summary>
    #region Editor
    #if UNITY_EDITOR
    [ReadOnly]
    #endif
    #endregion
    [SerializeField] private int attachedArmyIndex;

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
        //Adds the planet ship to the list of planet ships.
        planetShips.Add(this);
        //Assigns the value of the tooltip variable.
        tooltip = gameObject.GetComponent<GalaxyTooltip>();
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

        ArmyManagementMenu.CreateNewArmyManagementMenu(attachedPlanetID);
        //GalaxyManager.planets[attachedPlanetID].ChangeArmyIndex(attachedArmyIndex, 2);
    }

    /// <summary>
    /// This method is called whenever the mouse enters the box collider of the planet ship and updates the text of the tooltip to accurately reflect the name of the army that the planet ship is representing.
    /// </summary>
    private void OnMouseEnter()
    {
        tooltip.Text = GalaxyManager.planets[attachedPlanetID].GetArmyAt(attachedArmyIndex).Name;
    }

    /// <summary>
    /// Updates the position of the planet ship to accurately reflect the planet and army represented.
    /// </summary>
    private void UpdatePosition()
    {
        transform.position = new Vector3(GalaxyManager.planets[attachedPlanetID].transform.position.x + OffsetPositions[attachedArmyIndex].x, GalaxyManager.planets[attachedPlanetID].transform.position.y + OffsetPositions[attachedArmyIndex].y, GalaxyManager.planets[attachedPlanetID].transform.position.z + OffsetPositions[attachedArmyIndex].z);
    }

    /// <summary>
    /// Sets the location of the planet ship based on the specified planet id and army index.
    /// </summary>
    /// <param name="planetID"></param>
    /// <param name="armyIndex"></param>
    public void SetLocation(int planetID, int armyIndex)
    {
        attachedPlanetID = planetID;
        attachedArmyIndex = armyIndex;
        UpdatePosition();
    }

    /// <summary>
    /// Sets the location of the planet ship based on the specified army index and the current value of the attached planet id.
    /// </summary>
    /// <param name="armyIndex"></param>
    public void SetLocation(int armyIndex)
    {
        SetLocation(attachedPlanetID, armyIndex);
    }

    /// <summary>
    /// This method should be called in order to destroy the planet ship safely.
    /// </summary>
    public void DestroyPlanetShip()
    {
        //Removes the planet ship from the static list of planet ships.
        planetShips.Remove(this);
        //Destroys the planet ships's game object.
        Destroy(gameObject);
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
