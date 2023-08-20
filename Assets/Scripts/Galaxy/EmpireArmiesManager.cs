using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;

public class EmpireArmiesManager
{
    /// <summary>
    /// Public observable collection that holds all of the armies that should be managed by this empire armies manager and no other.
    /// </summary>
    public ObservableCollection<NewGalaxyArmy> armies = null;

    /// <summary>
    /// Public property that should be used in order to access the empire that the empire armies manager is managing the armies of.
    /// </summary>
    public NewEmpire empire { get; private set; } = null;

    public EmpireArmiesManager(NewEmpire empire, List<NewGalaxyArmy> armies = null)
    {
        this.armies = new ObservableCollection<NewGalaxyArmy>();
        this.armies.CollectionChanged += armies_CollectionChanged;

        this.empire = empire;

        if (armies != null)
            foreach (NewGalaxyArmy army in armies)
                this.armies.Add(army);
    }

    public EmpireArmiesManager(NewEmpire empire, EmpireArmiesManagerData empireArmiesManagerData)
    {
        armies = new ObservableCollection<NewGalaxyArmy>();
        armies.CollectionChanged += armies_CollectionChanged;

        this.empire = empire;

        if (empireArmiesManagerData.armies != null)
            foreach (NewGalaxyArmyData armyData in empireArmiesManagerData.armies)
                armies.Add(new NewGalaxyArmy(empire, armyData));
    }

    /// <summary>
    /// Private method that is called whenever the armies collection changes in any way and properly deals with the change.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void armies_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        //List changed - an army was added.
        if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
        {
            //Loops through each newly added army and ensures that it knows its assigned empire is the one the army manager belongs to.
            foreach (NewGalaxyArmy army in e.NewItems)
                if (army.empire != empire)
                    army.SetEmpire(empire);
        }
        //List changed - an army was removed.
        else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
        {
            //Loops through each removed army and ensures that it knows its assigned empire is no longer the one the army manager belongs to.
            foreach (NewGalaxyArmy army in e.OldItems)
                if (army.empire == empire)
                    army.SetEmpire(null);
        }
    }
}

[System.Serializable]
public class EmpireArmiesManagerData
{
    public List<NewGalaxyArmyData> armies = null;

    public EmpireArmiesManagerData(EmpireArmiesManager empireArmiesManager)
    {
        if(empireArmiesManager.armies != null && empireArmiesManager.armies.Count > 0)
        {
            armies = new List<NewGalaxyArmyData>();
            foreach (NewGalaxyArmy army in empireArmiesManager.armies)
                armies.Add(new NewGalaxyArmyData(army));
        }
    }
}