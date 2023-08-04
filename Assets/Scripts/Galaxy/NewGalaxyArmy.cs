using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class NewGalaxyArmy : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public observable collection that holds all of the squads that are assigned to the army.
    /// </summary>
    public ObservableCollection<NewGalaxySquad> squads = null;

    /// <summary>
    /// Public property that should be used in order to access the average experience of all of the pills within the squads of the army.
    /// </summary>
    public override float experience { get => base.experience; }

    public NewGalaxyArmy(string name) : base(name)
    {
        //Initializes the squads observable collection and sets its collection changed method call.
        squads = new ObservableCollection<NewGalaxySquad>();
        squads.CollectionChanged += squads_CollectionChanged;
    }

    public NewGalaxyArmy(NewGalaxyArmyData armyData) : base(armyData.groundUnitData)
    {
        //Initializes the squads observable collection and sets its collection changed method call.
        squads = new ObservableCollection<NewGalaxySquad>();
        squads.CollectionChanged += squads_CollectionChanged;
    }

    /// <summary>
    /// Private method that is called whenever the squads collection changes in any way and properly deals with the change such as removing the squad from its old army if needed and updating the army's average experience level.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void squads_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a squad was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each squad that was just added and ensures that its assigned army is this army.
            foreach (NewGalaxySquad addedSquad in e.NewItems)
                if (addedSquad.assignedArmy != this)
                    addedSquad.assignedArmy = this;

            //Updates the experience of the army to accurately reflect the average experience of all of the pills within the all of the squads within the army.
            UpdateExperience();
        }
        //List changed - a squad was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each squad that was just removed and ensures that its assigned army is no longer this army.
            foreach (NewGalaxySquad removedSquad in e.NewItems)
                if (removedSquad.assignedArmy == this)
                    removedSquad.assignedArmy = null;

            //Updates the experience of the army to accurately reflect the average experience of all of the pills within the all of the squads within the army.
            UpdateExperience();
        }
    }

    /// <summary>
    /// Public method that should be called in order to update the army's experience to accurately reflect the average experience of the pills that belong to the squads that belong to the army.
    /// </summary>
    public void UpdateExperience()
    {
        //Checks if the squads collection is null and sets the army's experience to 1 and returns if so.
        if (squads == null)
        {
            base.experience = 1;
            return;
        }

        //Sums up the army's total experience and total number of pills.
        float totalExperience = 0;
        int totalPills = 0;
        foreach(NewGalaxySquad squad in squads)
        {
            if(squad != null)
            {
                foreach (NewGalaxyPill pill in squad.pills)
                {
                    if (pill != null)
                    {
                        totalExperience += pill.experience;
                        totalPills += 1;
                    }
                }
            }
        }

        //Sets the army's experience to the total experience divided by the total number of pills in order to make it the average experience of the army.
        base.experience = totalExperience / totalPills;
    }
}

[System.Serializable]
public class NewGalaxyArmyData
{
    public NewGalaxyGroundUnitData groundUnitData = null;

    public NewGalaxyArmyData(NewGalaxyArmy army)
    {
        groundUnitData = new NewGalaxyGroundUnitData(army);
    }
}