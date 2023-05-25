using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DavyJonesLocker : MonoBehaviour
{
    /*
        Maps dead pills to their most recent corpse, if still present. Used to get corpses for special handling like executions.

        Flow:
        1. When corpses are created, they are registed here.
        2. When corpses stick in the ground, they are removed from here.
        3. When performing executions, the corpse to execute is retrieved from here.
        4. Its theoretically possible you could die again before your previous corpse is removed... in that case the old entry here is replaced by the new one.
    */

    private static Dictionary<Pill, Corpse> theLocker;

    //Call this once at the beginning of each planet scene.
    public static void PrepareTheLockerForSouls ()
    {
        if (theLocker != null)
            theLocker.Clear();

        theLocker = new Dictionary<Pill, Corpse>();
    }

    public static void CheckIn (Pill pill, Corpse corpse)
    {
        //If pill not present, adds as new entry. If pill already present, replaces corpse with new corpse.
        theLocker[pill] = corpse;
    }

    public static void CheckOut (Pill pill, Corpse corpse)
    {
        //Ensure we have a match before removing. This is needed in the scenario where you've died twice in quick succession and the original entry got overridden by the new.
        if(GetResident(pill) == corpse)
            theLocker.Remove(pill);
    }

    public static Corpse GetResident (Pill pill)
    {
        //Return null if not present.
        theLocker.TryGetValue(pill, out Corpse corpse);
        return corpse;
    }
}
