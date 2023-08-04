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

    public NewGalaxySquad(string name, Color color) : base(name)
    {
        //Initializes the pills observable collection and sets its collection changed method call.
        pills = new ObservableCollection<NewGalaxyPill>();
        pills.CollectionChanged += pills_CollectionChanged;

        //Initializes the color graphics observable collection and sets its collection changed method call.
        colorGraphics = new ObservableCollection<Graphic>();
        colorGraphics.CollectionChanged += colorGraphics_CollectionChanged;
    }

    public NewGalaxySquad(NewGalaxySquadData squadData) : base(squadData.groundUnitData)
    {
        //Initializes the pills observable collection and sets its collection changed method call.
        pills = new ObservableCollection<NewGalaxyPill>();
        pills.CollectionChanged += pills_CollectionChanged;

        //Initializes the color graphics observable collection and sets its collection changed method call.
        colorGraphics = new ObservableCollection<Graphic>();
        colorGraphics.CollectionChanged += colorGraphics_CollectionChanged;
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
        }
        //List changed - a pill was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each pill that was just removed and ensures that its assigned squad is no longer this squad.
            foreach (NewGalaxyPill removedPill in e.NewItems)
                if (removedPill.assignedSquad == this)
                    removedPill.assignedSquad = null;

            //Updates the experience of the squad to accurately reflect the average experience of all of the pills within the squad.
            UpdateExperience();
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
}

[System.Serializable]
public class NewGalaxySquadData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public List<int> pillIDs = null;
    public float[] color = null;

    public NewGalaxySquadData(NewGalaxySquad squad)
    {
        groundUnitData = new NewGalaxyGroundUnitData(squad);

        pillIDs = new List<int>();
        foreach (NewGalaxyPill pill in squad.pills)
            pillIDs.Add(pill.ID);

        color = new float[] { squad.color.r, squad.color.g, squad.color.b, squad.color.a };
    }
}