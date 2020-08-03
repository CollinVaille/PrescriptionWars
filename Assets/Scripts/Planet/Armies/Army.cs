using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
    //STATIC ARMY MANAGEMENT----------------------------------------------------------------------------

    private static List<Army> armies;

    public static Army GetArmy(int team)
    {
        Army army = null;

        for (int x = 0; x < armies.Count; x++)
        {
            if (armies[x].team == team)
            {
                army = armies[x];
                break;
            }
        }

        return army;
    }

    //ARMY INSTANCE-------------------------------------------------------------------------------------

    //Army composition
    public List<Squad> squads;
    public Pill fieldGeneral;
    
    //Faction data
    public int team = 0;
    public Empire.Culture culture;
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
        plasma1 = Resources.Load<Material>("Projectiles/Materials/" + culture.ToString() + " Plasma 1");
        plasma2 = Resources.Load<Material>("Projectiles/Materials/" + culture.ToString() + " Plasma 2");
    }

    public void AddSquad ()
    {

    }

    public CommsChannel Comms () { return commsChannel; }
}