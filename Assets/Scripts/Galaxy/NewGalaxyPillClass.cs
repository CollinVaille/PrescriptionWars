using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class NewGalaxyPillClass
{
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the gear that a pill can equip.
    /// </summary>
    private static string gearResourcesFolderPath { get => "Planet/Gear"; }
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the body gear that a pill can equip.
    /// </summary>
    private static string bodyGearResourcesFolderPath { get => gearResourcesFolderPath + "/Body Gear"; }
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the curated eye wear that a pill can equip.
    /// </summary>
    private static string curatedEyeWearResourcesFolderPath { get => gearResourcesFolderPath + "/Curated Eye Wear"; }
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the head gear that a pill can equip.
    /// </summary>
    private static string headGearResourcesFolderPath { get => gearResourcesFolderPath + "/Head Gear"; }
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the items that a pill can equip, including primary and secondary weapons.
    /// </summary>
    private static string itemsResourcesFolderPath { get => "Planet/Items"; }
    /// <summary>
    /// Private static property that should be used in order to access the string value that represents the path to the resources folder that contains all of the sprites that can possibly serve as an icon to a pill class.
    /// </summary>
    private static string iconSpritesResourcesFolderPath { get => "General/Pill Class Icons"; }

    /// <summary>
    /// Public property that should be used in order to access the empire that the pill class belongs to.
    /// </summary>
    public NewEmpire assignedEmpire { get => _assignedEmpire; }
    /// <summary>
    /// Private variable that holds a reference to the empire that the pill class belongs to.
    /// </summary>
    private NewEmpire _assignedEmpire = null;

    /// <summary>
    /// Public observable collection that holds all of the pills that have the class equipped.
    /// </summary>
    public ObservableCollection<NewGalaxyPill> pills = null;

    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates the class' current index in its assigned empire's list of classes. Useful value for save data.
    /// </summary>
    public int index { get => assignedEmpire == null || assignedEmpire.pillClasses == null || !assignedEmpire.pillClasses.Contains(this) ? -1 : assignedEmpire.pillClasses.IndexOf(this); }

    /// <summary>
    /// Public property that should be used in order to access and modify the string value that indicates the name of the pill class.
    /// </summary>
    public string name
    {
        get => _name;
        set
        {
            _name = value;
        }
    }
    /// <summary>
    /// Private holder variable for the string value that indicates the name of the pill class.
    /// </summary>
    private string _name = null;

    /// <summary>
    /// Public property that should be used in order to access and modify the enum value that indicates the type of pill class.
    /// </summary>
    public PillClassType classType
    {
        get => _classType;
        set
        {
            _classType = value;
        }
    }
    /// <summary>
    /// Private holder variable for the enum value that indicates the type of pill class.
    /// </summary>
    private PillClassType _classType = 0;

    /// <summary>
    /// Public property that should be used in order to acess the sprite that serves as the icon of the class. Useful for UI menus such as the army management menu.
    /// </summary>
    public Sprite iconSprite { get => Resources.Load<Sprite>(iconSpritesResourcesFolderPath + "/" + classType); }

    /// <summary>
    /// Public property that should be used in order to access the game object that the pill's currently equipped body gear would be instantiated from if viewed.
    /// </summary>
    public GameObject bodyGearPrefab { get => string.IsNullOrWhiteSpace(bodyGearName) ? null : Resources.Load<GameObject>(bodyGearResourcesFolderPath + "/" + bodyGearName); }
    /// <summary>
    /// Public property that should be used in order to both access and modify the string value that represents the name of the body gear that the pill has equipped.
    /// </summary>
    public string bodyGearName
    {
        get => _bodyGearName;
        set
        {
            //Checks if the specified body gear name is different from the already equipped body gear's name.
            if(value != _bodyGearName)
            {
                //Sets the equipped body gear to the body gear with the specified name.
                _bodyGearName = value;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its body gear to match the pill class' newly equipped body gear.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdateBodyGear();
            }
        }
    }
    /// <summary>
    /// Private holder variable for the string value that represents the name of the body gear that the pill has equipped.
    /// </summary>
    private string _bodyGearName = null;

    /// <summary>
    /// Public property that should be used in order to access the game object that the pill's currently equipped curated eye wear would be instantiated from if viewed.
    /// </summary>
    public GameObject curatedEyeWearPrefab { get => string.IsNullOrWhiteSpace(curatedEyeWearName) ? null : Resources.Load<GameObject>(curatedEyeWearResourcesFolderPath + "/" + curatedEyeWearName); }
    /// <summary>
    /// Public property that should be used to both access and modify the string value that represents the name of the curated eye wear that the pill has equipped.
    /// </summary>
    public string curatedEyeWearName
    {
        get => _curatedEyeWearName;
        set
        {
            //Checks if the specified curated eye wear name is different from the already equipped curated eye wear's name.
            if (value != _curatedEyeWearName)
            {
                //Sets the equipped curated eye wear to the curated eye wear with the specified name.
                _curatedEyeWearName = value;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its curated eye wear to match the pill class' newly equipped curated eye wear.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdateCuratedEyeWear();
            }
        }
    }
    /// <summary>
    /// Private holder variable for the string value that represents the name of the curated eye wear that the pill has equipped.
    /// </summary>
    private string _curatedEyeWearName = null;

    /// <summary>
    /// Public property that should be used in order to access the game object that the pill's currently equipped head gear would be instantiated from if viewed.
    /// </summary>
    public GameObject headGearPrefab { get => string.IsNullOrWhiteSpace(headGearName) ? null : Resources.Load<GameObject>(headGearResourcesFolderPath + "/" + headGearName); }
    /// <summary>
    /// Public property that should be used in order to both access and modify the string value that represents the name of the head gear that the pill has equipped.
    /// </summary>
    public string headGearName
    {
        get => _headGearName;
        set
        {
            //Checks if the specified head gear name is different from the already equipped head gear's name.
            if (value != _headGearName)
            {
                //Sets the equipped head gear to the head gear with the specified name.
                _headGearName = value;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its head gear to match the pill class' newly equipped head gear.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdateHeadGear();
            }
        }
    }
    /// <summary>
    /// Private holder variable for the string value that represents the name of the head gear that the pill has equipped.
    /// </summary>
    private string _headGearName = null;

    /// <summary>
    /// Public property that should be used in order to access the game object that serves as the primary weapon of pills of the class.
    /// </summary>
    public GameObject primaryPrefab { get => Resources.Load<GameObject>(itemsResourcesFolderPath + "/" + primaryName); }
    /// <summary>
    /// Public property that should be used in order to access and modify the primary weapon of pills of the class by name.
    /// </summary>
    public string primaryName
    {
        get => _primaryName;
        set
        {
            //Checks if the specified primary weapon name is different from the already equipped primary weapon's name.
            if (value != _primaryName)
            {
                //Sets the equipped primary weapon to the primary weapon with the specified name.
                _primaryName = value;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its primary weapon to match the pill class' newly equipped primary weapon.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdatePrimary();
            }
        }
    }
    /// <summary>
    /// Private holder variable for the string value that indicates the name of the primary weapon of pills of the class.
    /// </summary>
    private string _primaryName = null;

    /// <summary>
    /// Public property that should be used in order to access the game object that serves as the secondary weapon of pills of the class.
    /// </summary>
    public GameObject secondaryPrefab { get => Resources.Load<GameObject>(itemsResourcesFolderPath + "/" + secondaryName); }
    /// <summary>
    /// Public property that should be used in order to access and modify the secondary weapon of pills of the class by name.
    /// </summary>
    public string secondaryName
    {
        get => _secondaryName;
        set
        {
            //Checks if the specified secondary weapon name is different from the already equipped secondary weapon's name.
            if (value != _secondaryName)
            {
                //Sets the equipped secondary weapon to the secondary weapon with the specified name.
                _secondaryName = value;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its secondary weapon to match the pill class' newly equipped secondary weapon.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdateSecondary();
            }
        }
    }
    /// <summary>
    /// Private holder variable for the string value that indicates the name of the secondary weapon of pills of the class.
    /// </summary>
    private string _secondaryName = null;

    /// <summary>
    /// Public property that should be used in order to access the skin material that will be applied to all pills of the class.
    /// </summary>
    public Material skin { get => assignedEmpire == null ? null : Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(assignedEmpire.culture.ToString()) + "/" + skinName); }
    /// <summary>
    /// Public property that should be used in order to access and modify the skin that pills of the class have equipped by name.
    /// </summary>
    public string skinName
    {
        get => _skinName;
        set
        {
            //Checks if the specified skin name is different from the already equipped skin's name.
            if (value != _skinName)
            {
                //Sets the equipped skin to the skin with the specified name.
                _skinName = assignedEmpire != null && assignedEmpire.pillSkinNames != null && assignedEmpire.pillSkinNames.Contains(value) ? value : null;

                //Loops through every pill view of each pill that has the class equipped and tells it to update its skin to match the pill class' newly equipped skin.
                foreach (NewGalaxyPill pill in pills)
                    foreach (GalaxyPillView pillView in pill.pillViews)
                        pillView.UpdateSkin();
            }
        }
    }
    /// <summary>
    /// Private variable that holds the string value that indicates the name of the skin material that will be applied to pills of the class.
    /// </summary>
    private string _skinName = null;

    public NewGalaxyPillClass(NewEmpire assignedEmpire, string name, PillClassType classType, string bodyGearName = null, string curatedEyeWearName = null, string headGearName = null, string primaryName = null, string secondaryName = null, string skinName = null)
    {
        //Initializes the pills observable collection and sets its collection changed method call.
        pills = new ObservableCollection<NewGalaxyPill>();
        pills.CollectionChanged += pills_CollectionChanged;

        _assignedEmpire = assignedEmpire;
        this.name = name;
        this.classType = classType;
        this.bodyGearName = bodyGearName;
        this.curatedEyeWearName = curatedEyeWearName;
        this.headGearName = headGearName;
        this.primaryName = primaryName;
        this.secondaryName = secondaryName;
        this.skinName = skinName;
    }

    public NewGalaxyPillClass(NewEmpire assignedEmpire, NewGalaxyPillClassData pillClassData)
    {
        _assignedEmpire = assignedEmpire;
        name = pillClassData.name;
        classType = pillClassData.classType;
        bodyGearName = pillClassData.bodyGearName;
        curatedEyeWearName = pillClassData.curatedEyeWearName;
        headGearName = pillClassData.headGearName;
        primaryName = pillClassData.primaryName;
        secondaryName = pillClassData.secondaryName;
        skinName = pillClassData.skinName;
    }

    /// <summary>
    /// Private method that is called whenever the pills collection changes in any way and properly deals with the change.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void pills_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - a pill was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each pill that was just added and ensures that its pill class is this pill class.
            foreach (NewGalaxyPill addedPill in e.NewItems)
                if (addedPill.pillClass != this)
                    addedPill.pillClass = this;
        }
        //List changed - a pill was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each pill that was just removed and ensures that its pill class is not this pill class.
            foreach (NewGalaxyPill removedPill in e.OldItems)
                if (removedPill.pillClass == this)
                    removedPill.pillClass = null;
        }
    }
}

[System.Serializable]
public class NewGalaxyPillClassData
{
    public string name = null;
    public PillClassType classType = 0;
    public string bodyGearName = null;
    public string curatedEyeWearName = null;
    public string headGearName = null;
    public string primaryName = null;
    public string secondaryName = null;
    public string skinName = null;

    public NewGalaxyPillClassData(NewGalaxyPillClass pillClass)
    {
        name = pillClass.name;
        classType = pillClass.classType;
        bodyGearName = pillClass.bodyGearName;
        curatedEyeWearName = pillClass.curatedEyeWearName;
        headGearName = pillClass.headGearName;
        primaryName = pillClass.primaryName;
        secondaryName = pillClass.secondaryName;
        skinName = pillClass.skinName;
    }
}