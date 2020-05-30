using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Army : MonoBehaviour
{
    private static List<Army> armies;

    //Army composition
    public List<Squad> squads;
    public Pill fieldGeneral;
    
    //Faction allegience
    public int team = 0;

    //Comms channel
    private CommsChannel commsChannel;

    public void Awake ()
    {
        //Initialize comms channel
        commsChannel = GetComponent<CommsChannel>();
        commsChannel.InitializeCommsChannel(this);

        //When done with set up, make army available for referencing
        if (armies == null)
            armies = new List<Army>();
        armies.Add(this);
    }

    public static Army GetArmy (int team)
    {
        Army army = null;

        for(int x = 0; x < armies.Count; x++)
        {
            if(armies[x].team == team)
            {
                army = armies[x];
                break;
            }
        }

        return army;
    }

    public void AddSquad ()
    {

    }

    public CommsChannel Comms () { return commsChannel; }
}