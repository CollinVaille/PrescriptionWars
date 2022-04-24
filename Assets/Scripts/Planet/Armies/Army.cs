using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
    //STATIC ARMY MANAGEMENT----------------------------------------------------------------------------

    private static List<Army> armies;

    public static Army GetArmy(int team) { return armies.Find(army => army.team == team); }

    //ARMY INSTANCE-------------------------------------------------------------------------------------

    //Army composition
    public List<Squad> squads;
    public Pill fieldGeneral;
    
    //Faction data
    public int team = 0;
    public Empire.Culture culture;
    public Color color;
    [HideInInspector] public Material plasma1, plasma2;

    //Comms channel
    private CommsChannel commsChannel;

    public void Awake ()
    {
        //Initialize comms channel
        commsChannel = GetComponent<CommsChannel>();
        commsChannel.InitializeCommsChannel(this);

        //Initialize resources
        InitializeResources();

        //When done with set up, make army available for referencing
        if (armies == null)
            armies = new List<Army>();
        armies.Add(this);
    }

    private void InitializeResources ()
    {
        //Initialize materials used in empire-colored laser
        //Color plasmaColor = color;
        //plasmaColor *= 1.25f; //Brighter
        //plasmaColor.a = 0.5f; //Halfway transparent

        plasma1 = Resources.Load<Material>("Planet/Pooled Objects/Projectiles/Materials/" + culture.ToString() + " Plasma 1");
        //plasma1.SetColor("_TintColor", plasmaColor);

        plasma2 = Resources.Load<Material>("Planet/Pooled Objects/Projectiles/Materials/" + culture.ToString() + " Plasma 2");
        //plasma2.SetColor("_TintColor", plasmaColor);
    }

    public void AddSquad ()
    {

    }

    public CommsChannel Comms () { return commsChannel; }
}