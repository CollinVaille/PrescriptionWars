using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class NewGalaxyPill : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public property that should be used in order to access the unique ID of the pill. Only privately mutable for obvious reasons.
    /// </summary>
    public int ID { get; private set; }

    /// <summary>
    /// Public property that should be used to access and modify which class the pill belongs to.
    /// </summary>
    public NewGalaxyPillClass pillClass
    {
        get => _pillClass;
        set
        {
            //Checks if the specified class is different from the pill's already assigned class.
            if(value != _pillClass)
            {
                //Stores the previous pill class in a temporary variable.
                NewGalaxyPillClass previousPillClass = _pillClass;
                //Sets the pill's class to the specified class.
                _pillClass = value;
                //Checks if the previous pill class is still tracking the pill as belonging to it and stops that from happening if so.
                if (previousPillClass != null && previousPillClass.pills.Contains(this))
                    previousPillClass.pills.Remove(this);
                //Checks if the newly assigned pill class has started tracking the pill as belonging to it yet and tells it to if not.
                if (_pillClass != null && !_pillClass.pills.Contains(this))
                    _pillClass.pills.Add(this);

                //Updates the pill views that are displaying the pill to reflect the pill's class change.
                UpdatePillViews();
            }
        }
    }
    /// <summary>
    /// Private holder variable for which class the pill belongs to.
    /// </summary>
    private NewGalaxyPillClass _pillClass = null;

    /// <summary>
    /// Public property that should be used both in order to access and mutate the squad that the pill is assigned to.
    /// </summary>
    public NewGalaxySquad assignedSquad
    {
        get => _assignedSquad;
        set
        {
            //Returns if the specified squad is the squad that the pill is already assigned to.
            if (value == _assignedSquad)
                return;

            //Stores the previously assigned squad to a temporary variable.
            NewGalaxySquad previousSquad = _assignedSquad;
            //Sets the pill's assigned squad as the specified value.
            _assignedSquad = value;

            //Removes the pill from its previous squad if necessary.
            if (previousSquad != null && previousSquad.pills.Contains(this))
                previousSquad.pills.Remove(this);

            //Adds the pill to the specified squad's collection of pills if necessary.
            if (_assignedSquad != null && !_assignedSquad.pills.Contains(this))
                _assignedSquad.pills.Add(this);
        }
    }
    /// <summary>
    /// Private holder variable for the squad that the pill is assigned to.
    /// </summary>
    private NewGalaxySquad _assignedSquad = null;

    /// <summary>
    /// Public property that should be used in order to access the boolean value that indicates whether or not the pill is the leader of its squad.
    /// </summary>
    public bool isSquadLeader { get => assignedSquad != null && assignedSquad.leader == this; }

    /// <summary>
    /// Public observable collection that holds all of the pill views that are displaying the pill. Used in order to update the pill views to exactly match the current appearance of the pill.
    /// </summary>
    public ObservableCollection<GalaxyPillView> pillViews = null;

    /// <summary>
    /// Public property that should be used both in order to access and mutate the amount of experience that the pill has. The pill specific override for the base ground unit experience property also updates the experience of the pill's assigned squad if applicable.
    /// </summary>
    public override float experience { get => base.experience; set { base.experience = value; if (assignedSquad != null) assignedSquad.UpdateExperience(); } }

    /// <summary>
    /// Public property that should be used in order to access the empire that the pill belongs to.
    /// </summary>
    public override NewEmpire empire { get => assignedSquad == null || assignedSquad.assignedArmy == null ? null : assignedSquad.assignedArmy.empire; }

    public NewGalaxyPill(string name, NewGalaxyPillClass pillClass, float experience = 1) : base(name, experience)
    {
        //Initializes the pill views observable collection and sets its collection changed method call.
        pillViews = new ObservableCollection<GalaxyPillView>();
        pillViews.CollectionChanged += pillViews_CollectionChanged;

        //Checks if the galaxy manager's pill manager is not null and adds the pill to the dictionary of pills being tracked by the pill manager and assigns the pill's ID if so.
        if (NewGalaxyManager.pillManager != null)
            AddToPillManager();
        //If the pill manager is null, the galaxy is assumedly not finished generating so the AddToPillManager function should be added to the functions executed by the galaxy generator once the galaxy has finished generating.
        else
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddToPillManager, 0);

        this.pillClass = pillClass;
    }

    public NewGalaxyPill(NewGalaxyPillData pillData, NewGalaxyPillClass pillClass) : base(pillData.groundUnitData)
    {
        //Initializes the pill views observable collection and sets its collection changed method call.
        pillViews = new ObservableCollection<GalaxyPillView>();
        pillViews.CollectionChanged += pillViews_CollectionChanged;

        ID = pillData.ID;

        this.pillClass = pillClass;
    }

    /// <summary>
    /// Private method that should be called in order to add the pill to the dictionary of pills being held by the galaxy manager's pill manager and also to assign its ID.
    /// </summary>
    private void AddToPillManager()
    {
        //Returns if the galaxy manager's pill manager that contains all of the pills within the galaxy is null or if the pill has already been added to the pill manager and assigned an ID.
        if (NewGalaxyManager.pillManager == null || ID >= 0)
            return;

        //Adds the pill to the dictionary of pills being held by the pill manager and assigns its ID.
        ID = NewGalaxyManager.pillManager.AddPill(this);
    }

    /// <summary>
    /// Private method that is called whenever the pill views collection changes in any way and properly deals with the change.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void pillViews_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a pill view was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each pill view that was just added and ensures that its displayed pill is this pill.
            foreach (GalaxyPillView addedPillView in e.NewItems)
                if (addedPillView.displayedPill != this)
                    addedPillView.displayedPill = this;
        }
        //List changed - a pill view was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each pill view that was just removed and ensures that its displayed pill is not this pill.
            foreach (GalaxyPillView removedPillView in e.OldItems)
                if (removedPillView.displayedPill == this)
                    removedPillView.displayedPill = null;
        }
    }

    /// <summary>
    /// Private method that should be called in order to update the pill views that are currently displaying the pill.
    /// </summary>
    private void UpdatePillViews()
    {
        //Loops through each pill view thats displaying the pill and updates the pill appearance in said pill view.
        foreach (GalaxyPillView pillView in pillViews)
            pillView.UpdatePillAppearance();
    }
}

[System.Serializable]
public class NewGalaxyPillData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public int ID = -1;
    public int pillClassIndex = -1;

    public NewGalaxyPillData(NewGalaxyPill pill)
    {
        groundUnitData = new NewGalaxyGroundUnitData(pill);
        ID = pill.ID;
        pillClassIndex = pill.pillClass == null ? -1 : pill.pillClass.index;
    }
}
