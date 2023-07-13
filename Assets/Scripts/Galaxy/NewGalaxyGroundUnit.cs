using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.ObjectModel;

public abstract class NewGalaxyGroundUnit
{
    public enum GroundUnitType
    {
        Army,
        Squad,
        Pill
    }
    /// <summary>
    /// Public property that should be used in order to access the enum value that indicate the type of ground unit. Only protectedly mutatable to prevent external meddling.
    /// </summary>
    public GroundUnitType groundUnitType { get; protected set; } = 0;

    /// <summary>
    /// Public property that should be used in order to access and modify the string value that represents the name of the ground unit.
    /// </summary>
    public virtual string name
    {
        get => _name;
        set
        {
            //Sets the ground unit's name to the specified value.
            _name = value;
            //Updates the name texts to reflect this change.
            if (nameTexts != null)
                foreach (Text nameText in nameTexts)
                    if (nameText != null)
                        nameText.text = _name;
        }
    }
    /// <summary>
    /// Public variable that holds the text components that are set to the name of the ground unit (does not save when the object is serialized into data).
    /// </summary>
    public ObservableCollection<Text> nameTexts = null;
    /// <summary>
    /// Protected variable that holds the string value that represents the name of the ground unit.
    /// </summary>
    protected string _name = null;

    /// <summary>
    /// Public property that should be used in order to access and modify the float value that represents the amount of experience that the ground unit has.
    /// </summary>
    public virtual float experience { get => _experience; set => _experience = value; }
    /// <summary>
    /// Public property that should be used in order to access the integer value that represents the amount of experience that the ground unit has casted or floored from a float value to an integer value.
    /// </summary>
    public virtual int experienceLevel { get => (int)experience; }
    /// <summary>
    /// Public variable that holds the text components that are set to the experience level of the ground unit (does not save when the object is serialized into data).
    /// </summary>
    public ObservableCollection<Text> experienceLevelTexts = null;
    /// <summary>
    /// Protected variable that holds the float value that represents the amount of experience that the ground unit has.
    /// </summary>
    protected float _experience = 1;

    protected NewGalaxyGroundUnit(string name, float experience = 1)
    {
        //Initializes the name texts observable collection and sets its collection changed method call.
        nameTexts = new ObservableCollection<Text>();
        nameTexts.CollectionChanged += nameTexts_CollectionChanged;

        //Initializes the experience level texts observable collection and sets its collection changed method call.
        experienceLevelTexts = new ObservableCollection<Text>();
        experienceLevelTexts.CollectionChanged += experienceLevelTexts_CollectionChanged;

        //Initializes the ground unit with the specified parameters.
        this.name = name;
        this.experience = experience;
    }

    protected NewGalaxyGroundUnit(NewGalaxyGroundUnitData groundUnitData)
    {
        //Initializes the name texts observable collection and sets its collection changed method call.
        nameTexts = new ObservableCollection<Text>();
        nameTexts.CollectionChanged += nameTexts_CollectionChanged;

        //Initializes the experience level texts observable collection and sets its collection changed method call.
        experienceLevelTexts = new ObservableCollection<Text>();
        experienceLevelTexts.CollectionChanged += experienceLevelTexts_CollectionChanged;

        //Initializes the ground unit with the specified saved parameters.
        name = groundUnitData.name;
        experience = groundUnitData.experience;
    }

    /// <summary>
    /// Protected method that is called whenever the nameTexts collection changes in any way and properly deals with the change such as updating the text component to display the name of the ground unit when the component is added to the list.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void nameTexts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a name text was added.
        if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each new name text added and sets its text to the name of the ground unit.
            foreach(Text nameText in e.NewItems)
            {
                if(nameText != null)
                    nameText.text = name;
            }
        }
    }

    /// <summary>
    /// Protected method that is called whenever the experienceLevelTexts collection changes in any way and properly deals with the change such as updating the text component to display the experience level of the ground unit when the component is added to the list.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void experienceLevelTexts_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - an experience level text was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each new experience level text added and sets its text to the experience level of the ground unit.
            foreach (Text experienceLevelText in e.NewItems)
            {
                if (experienceLevelText != null)
                    experienceLevelText.text = experienceLevel.ToString();
            }
        }
    }
}

[System.Serializable]
public class NewGalaxyGroundUnitData
{
    public string name = null;
    public float experience = 0;

    public NewGalaxyGroundUnitData(NewGalaxyGroundUnit groundUnit)
    {
        name = groundUnit.name;
        experience = groundUnit.experience;
    }
}