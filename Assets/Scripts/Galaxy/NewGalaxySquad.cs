using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;

public class NewGalaxySquad : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public observable collection that holds all of the pills that are assigned to the squad. Pills are saved in serialized data via ID.
    /// </summary>
    public ObservableCollection<NewGalaxyPill> pills = null;
    /// <summary>
    /// Private list that holds all of the IDs of pills that were saved as belonging to the squad. This list exist just in case the galaxy manager and therefore the pill manager has not been initialized as of yet when attempting to recreate a squad from save data.
    /// </summary>
    private List<int> pillIDsFromSaveData = null;

    /// <summary>
    /// Private holder variable for the ID of the pill that serves as the leader of the squad from save data.
    /// </summary>
    private int leaderIDFromSaveData = -1;
    /// <summary>
    /// Private holder variable for the pill that serves as the leader of the squad.
    /// </summary>
    private NewGalaxyPill _leader = null;
    /// <summary>
    /// Public property that should be used both in order to access and mutate which pill serves as the leader of the squad.
    /// </summary>
    public NewGalaxyPill leader
    {
        get => _leader;
        set
        {
            _leader = value != null && pills.Contains(value) ? value : null;
        }
    }

    /// <summary>
    /// Public observable collection that holds graphics whose color should be updated in order to match the squad's color.
    /// </summary>
    public ObservableCollection<Graphic> colorGraphics = null;

    /// <summary>
    /// Public property that should be used both in order to access and mutate the color of the squad. This color will be applied on the assigned army's icon thats displayed on a squad button's far left image on the army management menu.
    /// </summary>
    public Color color { get => _color; set { _color = value; if (colorGraphics != null) foreach (Graphic colorGraphic in colorGraphics) if (colorGraphic != null) colorGraphic.color = value; } }
    /// <summary>
    /// Private variable that holds the color of the squad. This color will be applied on the assigned army's icon thats displayed on a squad button's far left image on the army management menu.
    /// </summary>
    private Color _color = Color.white;

    /// <summary>
    /// Public property that should be used both in order to access and mutate the army that the pill is assigned to.
    /// </summary>
    public NewGalaxyArmy assignedArmy
    {
        get => _assignedArmy;
        set
        {
            //Returns if the specified army is the army that the squad is already assigned to.
            if (value == _assignedArmy)
                return;

            //Stores the previously assigned army to a temporary variable.
            NewGalaxyArmy previousArmy = _assignedArmy;
            //Sets the squad's assigned army as the specified value.
            _assignedArmy = value;

            //Removes the squad from its previous army if necessary.
            if (previousArmy != null && previousArmy.squads.Contains(this))
                previousArmy.squads.Remove(this);

            //Adds the squad to the specified army's collection of squads if necessary.
            if (_assignedArmy != null && !_assignedArmy.squads.Contains(this))
                _assignedArmy.squads.Add(this);
        }
    }
    /// <summary>
    /// Private holder variable for the army that the squad is assigned to.
    /// </summary>
    private NewGalaxyArmy _assignedArmy = null;

    /// <summary>
    /// Public property that should be used in order to access the average experience of all of the pills within the squad.
    /// </summary>
    public override float experience { get => base.experience; }

    /// <summary>
    /// Public property that should be used in order to access the empire that the squad belongs to.
    /// </summary>
    public override NewEmpire empire { get => assignedArmy == null ? null : assignedArmy.empire; }

    public NewGalaxySquad(string name, Color color) : base(name)
    {
        //Initializes the pills observable collection and sets its collection changed method call.
        pills = new ObservableCollection<NewGalaxyPill>();
        pills.CollectionChanged += pills_CollectionChanged;

        //Initializes the color graphics observable collection and sets its collection changed method call.
        colorGraphics = new ObservableCollection<Graphic>();
        colorGraphics.CollectionChanged += colorGraphics_CollectionChanged;

        //Sets the color of the squad to the specified color value.
        this.color = color;
    }

    public NewGalaxySquad(NewGalaxySquadData squadData) : base(squadData.groundUnitData)
    {
        //Initializes the pills observable collection and sets its collection changed method call.
        pills = new ObservableCollection<NewGalaxyPill>();
        pills.CollectionChanged += pills_CollectionChanged;

        //Initializes the color graphics observable collection and sets its collection changed method call.
        colorGraphics = new ObservableCollection<Graphic>();
        colorGraphics.CollectionChanged += colorGraphics_CollectionChanged;

        //Loads in the color of the squad from the save data (converts a serialized float array with 4 indices into a color).
        color = new Color(squadData.color[0], squadData.color[1], squadData.color[2], squadData.color[3]);

        //Loads in the ID of the pill that serves as the leader of the squad.
        leaderIDFromSaveData = squadData.leaderID;

        //Checks if the galaxy manager has been initialized and adds the correct pills from the pill manager back into the squad if so, otherwise the pills must be added back to the squad once the galaxy has finished generating and the galaxy manager has been initialized.
        if (NewGalaxyManager.isInitialized)
        {
            //Loops through each pill ID stored as belonging to the squad in the squad's save data and grabs the pill with the specified ID from the pill manager and assigns them back to the squad.
            foreach(int pillID in squadData.pillIDs)
            {
                pills.Add(NewGalaxyManager.pillManager.GetPill(pillID));
                if (pillID == leaderIDFromSaveData)
                    leader = pills[pills.Count - 1];
            }
        }
        else
        {
            //Stores the pill IDs to be added back to the squad from save data into a local list. This local list will be null if the save data list was null or if the count of the save data list was 0.
            pillIDsFromSaveData = squadData.pillIDs != null && squadData.pillIDs.Count > 0 ? squadData.pillIDs : null;
            //Checks if the list of pills to add back to the squad from save data is not null and tells the galaxy manager to execute the function to add the pills back to squad once the galaxy has finished generating if so.
            if (pillIDsFromSaveData != null)
                NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddPillsFromSaveData, 0);
        }
    }

    /// <summary>
    /// Private method that is called whenever the pills collection changes in any way and properly deals with the change such as removing the pill from its old squad if needed and updating the squad's average experience level.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void pills_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a pill was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each pill that was just added and ensures that its assigned squad is this squad.
            foreach (NewGalaxyPill addedPill in e.NewItems)
                if(addedPill.assignedSquad != this)
                    addedPill.assignedSquad = this;

            //Updates the experience of the squad to accurately reflect the average experience of all of the pills within the squad.
            UpdateExperience();

            //Checks if there is no pill serving as the leader of the squad currently and chooses a new squad leader if so.
            if (leader == null)
                ChooseSquadLeader();
        }
        //List changed - a pill was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each pill that was just removed and ensures that its assigned squad is no longer this squad and checks if the pill was the squad leader and therefore if a new squad leader is needed.
            foreach (NewGalaxyPill removedPill in e.OldItems)
            {
                if (removedPill.assignedSquad == this)
                    removedPill.assignedSquad = null;
                if (removedPill == leader)
                    leader = null;
            }

            //Updates the experience of the squad to accurately reflect the average experience of all of the pills within the squad.
            UpdateExperience();

            //Checks if a new squad leader is needed and chooses a new squad leader if so.
            if (leader == null)
                ChooseSquadLeader();
        }
    }

    /// <summary>
    /// Private method that is called whenever the color graphics collection changes in any way and properly deals with the change such as updating the color of a newly added color graphic to match the color of the squad.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void colorGraphics_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a color graphic was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each newly added color graphic and updates its color to match the squad's color.
            foreach (Graphic colorGraphic in e.NewItems)
                if (colorGraphic != null)
                    colorGraphic.color = color;
        }
    }

    /// <summary>
    /// Public method that should be called in order to make the squad choose a squad leader. The pill with the highest experience level will be chosen.
    /// </summary>
    public void ChooseSquadLeader()
    {
        //Checks if the pills observable collection is null or if it is empty and sets the leader to null and returns if so.
        if(pills == null || pills.Count == 0)
        {
            leader = null;
            return;
        }

        //Loops through each pill in the squad and finds the one with the highest experience level. If there is a tie in experience level, the pill earlier in the list will be chosen.
        NewGalaxyPill mostExperiencedPill = pills[0];
        for(int pillIndex = 1; pillIndex < pills.Count; pillIndex++)
            if (pills[pillIndex].experienceLevel > mostExperiencedPill.experienceLevel)
                mostExperiencedPill = pills[pillIndex];

        //Sets the leader of the squad to the most experienced pill in the squad (based on experience level int and not experience float).
        leader = mostExperiencedPill;
    }

    /// <summary>
    /// Public method that should be called in order to update the squad's experience to accurately reflect the average experience of the pills that belong to the squad.
    /// </summary>
    public void UpdateExperience()
    {
        //Checks if the pills collection is either null or has no pills and sets the squad's experience to 1 and returns if so.
        if(pills == null || pills.Count == 0)
        {
            base.experience = 1;
            return;
        }

        //Sums up the squad's total experience and total number of pills.
        float totalExperience = 0;
        int totalPills = 0;
        foreach(NewGalaxyPill pill in pills)
        {
            if(pill != null)
            {
                totalExperience += pill.experience;
                totalPills += 1;
            }
        }

        //Sets the squad's experience to the total experience divided by the total number of pills in order to make it the average experience of the squad.
        base.experience = totalExperience / totalPills;
    }

    /// <summary>
    /// Private method that should be called by the galaxy generator once the galaxy has finished generating in order to add the pills saved as belonging to this squad in the saved data back into the squad since the galaxy manager and the pill manager has now been initialized.
    /// </summary>
    private void AddPillsFromSaveData()
    {
        //Returns if there are no pills to add to the squad from save data or if the pills observable collection is null and hasn't been initialized yet or if the galaxy manager and therefore the pill manager is null due to not having been initialized as of yet.
        if (pillIDsFromSaveData == null || pills == null || !NewGalaxyManager.isInitialized)
            return;

        //Loops through each pill ID stored as belonging to the squad in the squad's save data and grabs the pill with the specified ID from the pill manager and assigns them back to the squad.
        foreach (int pillIDFromSaveData in pillIDsFromSaveData)
        {
            //Grabs the pill specified in the save data via ID from the pill manager.
            NewGalaxyPill pillFromSaveData = NewGalaxyManager.pillManager.GetPill(pillIDFromSaveData);
            //Checks if the specified pill is not null and still exists and adds it back to the squad if so. Then it checks if the pill was the squad leader and reassigns it as the leader of the squad if so.
            if (pillFromSaveData != null)
            {
                pills.Add(pillFromSaveData);
                if (pillFromSaveData.ID == leaderIDFromSaveData)
                    leader = pillFromSaveData;
            }
        }

        //Sets the local list of IDs of pills that should be added back to the squad back to null since all of the pill IDs have been processed and the data is no longer needed.
        pillIDsFromSaveData = null;
    }
}

[System.Serializable]
public class NewGalaxySquadData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public List<int> pillIDs = null;
    public float[] color = null;
    public int leaderID = -1;

    public NewGalaxySquadData(NewGalaxySquad squad)
    {
        groundUnitData = new NewGalaxyGroundUnitData(squad);

        pillIDs = new List<int>();
        foreach (NewGalaxyPill pill in squad.pills)
            pillIDs.Add(pill.ID);

        color = new float[] { squad.color.r, squad.color.g, squad.color.b, squad.color.a };

        leaderID = squad.leader == null ? -1 : squad.leader.ID;
    }
}