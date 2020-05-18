using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Squad : MonoBehaviour
{
    public enum Orders { Standby, Roam, HoldPosition, Follow, FormLine, FormSquare }
    public enum SquadType { Mobilized, Citizen, DayGuard, NightGuard }

    //Basic squad variables
    private Orders orders = Orders.Standby;
    private int ordersID = 0;
    public SquadType squadType = SquadType.Mobilized;
    public Pill leader = null;

    //Members
    public List<Pill> members;

    public void InitializeSquad (int predictedSize)
    {
        members = new List<Pill>(predictedSize);

        name = GetRandomSquadName();
    }

    //Adds a squad member
    public void ReportingForDuty (Pill pill)
    {
        pill.squad = this;
        members.Add(pill);

        if (!leader)
            leader = pill;
    }

    //Removes a squad member
    public void ReportingDeparture (Pill pill)
    {
        pill.squad = null;
        members.Remove(pill);

        if(pill == leader)
        {
            if (members.Count == 0)
                leader = null;
            else
                leader = members[0];

            orders = Orders.Standby;
        }
    }

    //Called by leader to change the orders
    public void SetOrders (Orders orders)
    {
        this.orders = orders;

        ordersID++;

        //This is used by formation orders to determine the location and rotation of the formation
        transform.position = leader.transform.position;
        transform.eulerAngles = new Vector3(0, leader.transform.rotation.y, 0); //Only copy y rotation
    }

    public Orders GetOrders () { return orders; }

    public int GetOrdersID () { return ordersID; }

    public void AlertSquadOfAttacker (Pill attacker, Pill alerting, float withinRadius)
    {
        Vector3 alertingPosition = alerting.transform.position;

        //Alert each member of squad that is within specified radius to alerting pill of a new potential attacker
        for (int x = 0; x < members.Count; x++)
        {
            if (members[x] != alerting && Vector3.Distance(members[x].transform.position, alertingPosition) < withinRadius)
                members[x].AlertOfAttacker(attacker, true);
        }
    }

    //Called by squad members to figure out where to stand in the formation
    public Vector3 GetPlaceInFormation (Pill pill)
    {
        int pillIndex = members.IndexOf(pill);

        Vector3 localPosition = Vector3.zero;

        float spacing = 1.5f;

        if(orders == Orders.FormLine) //Form line
        {
            localPosition.x += (pillIndex - members.Count * 0.5f) * spacing;
        }
        else //Form square
        {
            float indexInRow = 0;
            if(pillIndex < members.Count * 0.25f) //Left rank
            {
                //Positioning of row
                localPosition.x -= members.Count * 0.25f * spacing;

                //Positioning within row
                indexInRow = pillIndex;
                localPosition.z += (indexInRow - members.Count * 0.125f) * spacing * 2;
            }
            else if (pillIndex < members.Count * 0.5f) //Bottom rank
            {
                //Positioning of row
                localPosition.z -= members.Count * 0.25f * spacing;

                //Positioning within row
                indexInRow = pillIndex - members.Count * 0.25f;
                localPosition.x += (indexInRow - members.Count * 0.125f) * spacing * 2;
            }
            else if (pillIndex < members.Count * 0.75f) //Right rank
            {
                //Positioning of row
                localPosition.x += members.Count * 0.25f * spacing;

                //Positioning within row
                indexInRow = pillIndex - members.Count * 0.5f;
                localPosition.z += (indexInRow - members.Count * 0.125f) * spacing * 2;
            }
            else //Top rank
            {
                //Positioning of row
                localPosition.z += members.Count * 0.25f * spacing;

                //Positioning within row
                indexInRow = pillIndex - members.Count * 0.75f;
                localPosition.x += (indexInRow - members.Count * 0.125f) * spacing * 2;
            }
        }

        return transform.TransformPoint(localPosition);
    }

    //Called by squad members to figure out how to rotate once in position in formation
    public void SetRotationInFormation (Transform pill)
    {
        if (orders == Orders.FormLine) //Form line (look forward)
            pill.rotation = transform.rotation;
        else //Form square (look out from center)
        {
            pill.LookAt(transform.position);
            pill.Rotate(Vector3.up * 180);
        }
    }

    public void PopulateSquadMenu (Transform squadMenu)
    {
        squadMenu.Find("Squad Name").GetComponent<Text>().text = name;

        //Squad leader
        if(leader)
        {
            if(leader.GetComponent<Player>())
                squadMenu.Find("Squad Leader").GetComponent<Text>().text = "Leader: " + leader.name + " (YOU)";
            else
                squadMenu.Find("Squad Leader").GetComponent<Text>().text = "Leader: " + leader.name;
        }
        else
            squadMenu.Find("Squad Leader").GetComponent<Text>().text = "Leader: ???";

        squadMenu.Find("Squad Orders").GetComponent<Text>().text = "Orders: " + God.SpaceOutString(orders.ToString());

        squadMenu.Find("Squad Objective").GetComponent<Text>().text = "Objective: ???";

        string squadMembers = "Members (" + members.Count + "):\n";

        for(int x = 0; x < members.Count - 1; x++)
            squadMembers += members[x].name + ", ";

        if (members.Count != 0)
            squadMembers += members[members.Count - 1].name;

        squadMenu.Find("Squad Members").GetComponent<Text>().text = squadMembers;
    }

    private string GetRandomSquadName ()
    {
        string squadName = "";

        int picker = Random.Range(0, 38);

        switch (picker)
        {
            case 0: squadName = "Alpha"; break;
            case 1: squadName = "Bravo"; break;
            case 2: squadName = "Charlie"; break;
            case 3: squadName = "Delta"; break;
            case 4: squadName = "Echo"; break;
            case 5: squadName = "Foxtrot"; break;
            case 6: squadName = "Gamma"; break;
            case 7: squadName = "Hotel"; break;
            case 8: squadName = "Kilo"; break;
            case 9: squadName = "November"; break;
            case 10: squadName = "Oscar"; break;
            case 11: squadName = "Romeo"; break;
            case 12: squadName = "Sierra"; break;
            case 13: squadName = "Tango"; break;
            case 14: squadName = "Victor"; break;
            case 15: squadName = "Whiskey"; break;
            case 16: squadName = "Yankee"; break;
            case 17: squadName = "Zulu"; break;
            case 18: squadName = "Omicron"; break;
            case 19: squadName = "Iota"; break;
            case 20: squadName = "Tau"; break;
            case 21: squadName = "Beta"; break;
            case 22: squadName = "Rho"; break;
            case 23: squadName = "Phi"; break;
            case 24: squadName = "Chi"; break;
            case 25: squadName = "Upsilon"; break;
            case 26: squadName = "Psi"; break;
            case 27: squadName = "Theta"; break;
            case 28: squadName = "Hunter"; break;
            case 29: squadName = "Sigma"; break;
            case 30: squadName = "Grizzly"; break;
            case 31: squadName = "Overlord"; break;
            case 32: squadName = "Viper"; break;
            case 33: squadName = "Warpig"; break;
            case 34: squadName = "Badger"; break;
            case 35: squadName = "Cobra"; break; //Broker, Reaper, Venator, Vulcan, Belarus, India
            case 36: squadName = "Stalker"; break; //X-Ray, Kansas, Lima, Butcher, Pikeman, Virginia
            default: squadName = "Zeta"; break; //Woodrat, Packrat, Muskrat, Vulture, Winter, Mike
        }

        return squadName + " Squad";
    }
}