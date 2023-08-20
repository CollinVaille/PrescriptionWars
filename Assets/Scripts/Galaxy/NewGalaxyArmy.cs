using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System;
using UnityEngine.UI;

public class NewGalaxyArmy : NewGalaxyGroundUnit
{
    /// <summary>
    /// Public observable collection that holds all of the squads that are assigned to the army.
    /// </summary>
    public ObservableCollection<NewGalaxySquad> squads = null;
    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates the maximum number of squads that can be assigned to the army.
    /// </summary>
    public int maxSquadsCount { get => 3; }

    /// <summary>
    /// Public observable collection that holds all of the images that are displaying the army's icon sprite.
    /// </summary>
    public ObservableCollection<Image> iconImages = null;
    /// <summary>
    /// Public property that should be used in order to access the sprite that acts as the icon of the army. The icon should be mutated via the iconName string property.
    /// </summary>
    public Sprite iconSprite { get => iconName == null ? null : Resources.Load<Sprite>("Galaxy/Army Icons/" + iconName); }
    /// <summary>
    /// Public property that should be used both in order to access and mutate the string value that represents the name of the icon that represents the army.
    /// </summary>
    public string iconName
    {
        get => _iconName;
        set
        {
            //Returns if the specified string value is already the icon name, or if the army icon names haven't been loaded in, or if the icon name is not found within the army icon names array and therefore invalid.
            if (value == _iconName || ArmyIconNamesLoader.armyIconNames == null || (_iconName != null && Array.IndexOf(ArmyIconNamesLoader.armyIconNames, value) <= -1))
                return;

            //Sets the string value that represents the name of the icon that represents the army to the specified value.
            _iconName = value;

            //Loops through each icon image and sets its sprite to be the army's new icon sprite.
            if (iconImages != null)
                foreach (Image iconImage in iconImages)
                    if (iconImage != null)
                        iconImage.sprite = iconSprite;
        }
    }
    /// <summary>
    /// Private holder variable for the name of the icon that represents the army.
    /// </summary>
    private string _iconName = null;

    /// <summary>
    /// Public property that should be used in order to access the average experience of all of the pills within the squads of the army.
    /// </summary>
    public override float experience { get => base.experience; }

    /// <summary>
    /// Public property that should be used in order to access the empire that the army belongs to.
    /// </summary>
    public override NewEmpire empire { get => _empire; }
    /// <summary>
    /// Private holder variable for the empire that the army belongs to.
    /// </summary>
    private NewEmpire _empire = null;

    /// <summary>
    /// Public property that should be used both in order to access and modify which planet the army is stationed on.
    /// </summary>
    public NewGalaxyPlanet planetStationed
    {
        get => _planetStationedID >= 0 && NewGalaxyManager.initialized && _planetStationedID < NewGalaxyManager.planets.Count ? NewGalaxyManager.planets[_planetStationedID] : null;
        set
        {
            //Stores the planet that the army was previously stationed on in a temporary variable.
            NewGalaxyPlanet previousPlanetStationed = planetStationed;
            //Sets the army to be stationed on the specified planet via ID.
            _planetStationedID = value == null ? -1 : value.ID;
            //Checks if the previously stationed planet is still tracking the army as stationed on it and tells it not to if so.
            if (previousPlanetStationed != null && previousPlanetStationed.stationedArmies.Contains(this))
                previousPlanetStationed.stationedArmies.Remove(this);
            //Checks if the newly stationed planet is tracking the army as stationed on it yet and tell it to if not.
            if (value != null && !value.stationedArmies.Contains(this))
                value.stationedArmies.Add(this);
        }
    }
    /// <summary>
    /// Private holder variable for the ID of the planet that the army is stationed on.
    /// </summary>
    private int _planetStationedID = -1;

    public NewGalaxyArmy(NewEmpire empire, string name, string iconName = null, NewGalaxyPlanet planetStationed = null) : base(name)
    {
        //Initializes the squads observable collection and sets its collection changed method call.
        squads = new ObservableCollection<NewGalaxySquad>();
        squads.CollectionChanged += squads_CollectionChanged;

        //Initializes the icon images observable collection and sets its collection changed method call.
        iconImages = new ObservableCollection<Image>();
        iconImages.CollectionChanged += iconImages_CollectionChanged;

        //Sets the empire that the army belongs to.
        _empire = empire;

        //Checks if no icon name was specified (so iconName = null) and sets the army's icon by providing a random valid icon name.
        if (iconName == null && ArmyIconNamesLoader.armyIconNames != null && ArmyIconNamesLoader.armyIconNames.Length > 0)
            this.iconName = ArmyIconNamesLoader.armyIconNames[UnityEngine.Random.Range(0, ArmyIconNamesLoader.armyIconNames.Length)];
        //Otherwise, or if certain required conditions are not met, the army's icon name is set to the specified value.
        else
            this.iconName = iconName;

        //Sets the planet that the army is stationed on (if any).
        this.planetStationed = planetStationed;
    }

    public NewGalaxyArmy(NewEmpire empire, NewGalaxyArmyData armyData) : base(armyData.groundUnitData)
    {
        //Initializes the squads observable collection and sets its collection changed method call.
        squads = new ObservableCollection<NewGalaxySquad>();
        squads.CollectionChanged += squads_CollectionChanged;

        //Initializes the icon images observable collection and sets its collection changed method call.
        iconImages = new ObservableCollection<Image>();
        iconImages.CollectionChanged += iconImages_CollectionChanged;

        //Sets the empire that the army belongs to.
        _empire = empire;

        //Checks if there were squads assigned to the army in the save data and recreates the squads and assigns them back to the army if so.
        if (armyData.squadsData != null && armyData.squadsData.Count > 0)
            foreach (NewGalaxySquadData squadData in armyData.squadsData)
                squads.Add(new NewGalaxySquad(squadData));

        //Loads in the army's saved icon via a string value that represents the name of the icon in the resources folder.
        iconName = armyData.iconName;

        //Sets the planet that the army is stationed on. May be only properly set once the galaxy has finished generating and the stationed planet is accessible to the army via the galaxy manager.
        if (NewGalaxyManager.initialized)
            planetStationed = armyData.planetStationedID >= 0 && armyData.planetStationedID < NewGalaxyManager.planets.Count ? NewGalaxyManager.planets[armyData.planetStationedID] : null;
        else
        {
            _planetStationedID = armyData.planetStationedID;
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddArmyToPlanetStationed, 0);
        }
    }

    /// <summary>
    /// Private method that should be called on galaxy generation completion in the specific scenario when needing to initialize the army from save data without the galaxy manager being initialized yet.
    /// </summary>
    private void AddArmyToPlanetStationed()
    {
        NewGalaxyPlanet planetStationed = this.planetStationed;
        this.planetStationed = null;
        this.planetStationed = planetStationed;
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

            //Loops until the number of squads assigned to the army equals the max squad count limit by removing the last squad in the squads list each iteration.
            while (squads.Count > maxSquadsCount)
                squads.RemoveAt(squads.Count - 1);

            //Updates the experience of the army to accurately reflect the average experience of all of the pills within the all of the squads within the army.
            UpdateExperience();
        }
        //List changed - a squad was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each squad that was just removed and ensures that its assigned army is no longer this army.
            foreach (NewGalaxySquad removedSquad in e.OldItems)
                if (removedSquad.assignedArmy == this)
                    removedSquad.assignedArmy = null;

            //Updates the experience of the army to accurately reflect the average experience of all of the pills within the all of the squads within the army.
            UpdateExperience();
        }
    }

    /// <summary>
    /// Private method that is called whenever the icon images collection changes in any way and properly deals with the change such as setting the sprite of a newly added image to match the army's icon sprite.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void iconImages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - an icon image was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each newly added icon image and sets its sprite to be the army's icon sprite.
            foreach (Image iconImage in e.NewItems)
                if (iconImage != null)
                    iconImage.sprite = iconSprite;
        }
        //List changed - an icon image was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            
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

    /// <summary>
    /// Public method that should be used in order to set the empire that the army belongs to to a specified empire value.
    /// </summary>
    /// <param name="empire"></param>
    public void SetEmpire(NewEmpire empire)
    {
        //Stores the previously assigned empire value in a temporary variable.
        NewEmpire previousEmpire = _empire;
        //Sets the army's assigned empire to the specified army value.
        _empire = empire;
        //Checks if the previously assigned empire still tracks the army as belonging to it and stops that if so.
        if (previousEmpire != null && previousEmpire.armiesManager.armies.Contains(this))
            previousEmpire.armiesManager.armies.Remove(this);
        //Checks if the newly assigned empire is not yet tracking the army as belonging to it and fixes that if so.
        if (this.empire != null && !this.empire.armiesManager.armies.Contains(this))
            this.empire.armiesManager.armies.Add(this);
    }
}

[System.Serializable]
public class NewGalaxyArmyData
{
    public NewGalaxyGroundUnitData groundUnitData = null;
    public List<NewGalaxySquadData> squadsData = null;
    public string iconName = null;
    public int planetStationedID = -1;

    public NewGalaxyArmyData(NewGalaxyArmy army)
    {
        groundUnitData = new NewGalaxyGroundUnitData(army);

        if(army.squads != null && army.squads.Count > 0)
        {
            squadsData = new List<NewGalaxySquadData>();
            foreach (NewGalaxySquad squad in army.squads)
                squadsData.Add(new NewGalaxySquadData(squad));
        }

        iconName = army.iconName;

        planetStationedID = army.planetStationed == null ? -1 : army.planetStationed.ID;
    }
}