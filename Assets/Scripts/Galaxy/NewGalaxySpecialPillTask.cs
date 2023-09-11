using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.ObjectModel;
using System;

public class NewGalaxySpecialPillTask
{
    /// <summary>
    /// Public property that should be used in order to access the integer value that indicates the ID of the special pill task within the special pill tasks dictionary that exists within the pill manager that contains all of the special pill tasks within the galaxy.
    /// </summary>
    public int ID { get; private set; } = -1;

    /// <summary>
    /// Public property that should be used in order to both access and mutate the special pill that is assigned to do the task.
    /// </summary>
    public NewGalaxySpecialPill assignedSpecialPill
    {
        get => _assignedSpecialPill;
        set
        {
            //Checks if the newly assigned special pill value is the same as the previously assigned special pill value and returns and does nothing if so.
            if (value == _assignedSpecialPill)
                return;

            //Stores the previously assigned special pill in a temporary variable.
            NewGalaxySpecialPill previouslyAssignedSpecialPill = _assignedSpecialPill;
            //Sets the task's assigned special pill to the newly assigned value.
            _assignedSpecialPill = value;
            //Checks if the previously assigned special pill still thinks its assigned to do this task and fixes that if so.
            if (previouslyAssignedSpecialPill != null && previouslyAssignedSpecialPill.task == this)
                previouslyAssignedSpecialPill.task = null;
            //Checks if the newly assigned special pill is not yet considering itself as belonging to this task and fixes that if so.
            if (_assignedSpecialPill != null && _assignedSpecialPill.task != this)
                _assignedSpecialPill.task = this;

            //Executes all of the functions that need to be executed whenever the task's assigned special pill value changes.
            if (functionsToExecuteOnAssignedSpecialPillValueChange != null)
                foreach (Action functionToExecuteOnAssignedSpecialPillValueChange in functionsToExecuteOnAssignedSpecialPillValueChange)
                    if (functionToExecuteOnAssignedSpecialPillValueChange != null)
                        functionToExecuteOnAssignedSpecialPillValueChange();
        }
    }
    /// <summary>
    /// Private holder variable for the special pill that is assigned to do the task.
    /// </summary>
    private NewGalaxySpecialPill _assignedSpecialPill = null;
    /// <summary>
    /// Private holder variable for the integer value that indicates the ID of the special pill that was assigned this task in the save data that was loaded in.
    /// </summary>
    private int assignedSpecialPillIDFromSaveData = -1;

    /// <summary>
    /// Private list that holds all of the functions that need to be executed whenever the special pill assigned to do the task is changed. This list is not saved within the task's save data.
    /// </summary>
    private List<Action> functionsToExecuteOnAssignedSpecialPillValueChange = null;

    public NewGalaxySpecialPillTask(NewGalaxySpecialPill assignedSpecialPill = null)
    {
        //Checks if the new galaxy manager and therefore the pill manager have both been initialized and adds the special pill task to the dictionary of all special pill tasks within the galaxy that exists in the pill manager if so.
        if (NewGalaxyManager.isInitialized)
            AddSpecialPillTaskToPillManager();
        //If the galaxy manager and the pill manager have both not yet been initialized, then the special pill task is added to the special pill tasks dictionary in the pill manager by the galaxy generator after the galaxy has finished generating and the managers have both been initialized.
        else
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(AddSpecialPillTaskToPillManager, 0);

        this.assignedSpecialPill = assignedSpecialPill;
    }

    public NewGalaxySpecialPillTask(NewGalaxySpecialPillTaskData specialPillTaskData)
    {
        //Loads in the special pill task's ID from the save data that indicates the tasks ID within the dictionary of special pill tasks that exists in the pill manager that contains all of the special pill tasks within the galaxy.
        ID = specialPillTaskData.ID;

        //Stores the ID of the pill that was assigned the task in the save data in a holder variable.
        assignedSpecialPillIDFromSaveData = specialPillTaskData.assignedSpecialPillID;
        //Checks if the galaxy manager and therefore the pill manager has already been initialized.
        if (NewGalaxyManager.isInitialized)
        {
            //Immediately loads in the special pill from the save data since the pill manager has already been initialized.
            if (assignedSpecialPillIDFromSaveData >= 0)
                LoadInAssignedSpecialPillFromSaveData();
        }
        //The galaxy manager and therefore the pill manager have not yet been initialized.
        else
        {
            //Loads in the special pill from the save data once the galaxy has finished generating and the galaxy manager and therefore the pill manager have both been initialized.
            if (assignedSpecialPillIDFromSaveData >= 0)
                NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(LoadInAssignedSpecialPillFromSaveData, 1);
        }
    }

    /// <summary>
    /// Private method that should be called once the galaxy has finished generating and the galaxy manager and therefore the pill manager have both been initialized and adds the special pill task to the dictionary of all special pill tasks within the galaxy that exists in the pill manager and also grabs and stores the special pill task's ID in said dictionary.
    /// </summary>
    private void AddSpecialPillTaskToPillManager()
    {
        //Checks if the special pill task's ID has already been assigned or if the pill manager and galaxy manager have not yet been initialized and returns if so.
        if (ID >= 0 || !NewGalaxyManager.isInitialized)
            return;

        //Adds the special pill task to the dictionary of all special pill tasks within the galaxy that exists within the galaxy manager's pill manager. Also grabs and stores the special pill task's assigned ID in said dictionary.
        ID = NewGalaxyManager.pillManager.AddSpecialPillTask(this);
    }

    /// <summary>
    /// Private method that should be called once the galaxy finishes generating and both the galaxy manager and pill manager have been initialized in order to load in the special pill assigned the task in the save data.
    /// </summary>
    private void LoadInAssignedSpecialPillFromSaveData()
    {
        assignedSpecialPill = NewGalaxyManager.pillManager.GetSpecialPill(assignedSpecialPillIDFromSaveData);
    }

    /// <summary>
    /// Public method that should be called in order to add a function to the list of functions that should be called whenever the task's assigned special pill is changed.
    /// </summary>
    /// <param name="function"></param>
    public void ExecuteFunctionOnAssignedSpecialPillValueChange(Action function)
    {
        //Checks if the specified function is null and returns if so since there is no actual function that could ever be executed.
        if (function == null)
            return;

        //Checks if the list of functions that need to be executed whenever the task's assigned special pill value changes is null and initializes it if so.
        if (functionsToExecuteOnAssignedSpecialPillValueChange == null)
            functionsToExecuteOnAssignedSpecialPillValueChange = new List<Action>();

        //Adds the specified function to the list of functions that need to be executed whenever the task's assigned special pill value changes.
        functionsToExecuteOnAssignedSpecialPillValueChange.Add(function);
    }

    /// <summary>
    /// Public method that should be called in order to remove a specified function from the list of functions that need to be executed whenever the task's assigned special pill value changes.
    /// </summary>
    /// <param name="function"></param>
    public void RemoveFunctionFromFunctionsToExecuteOnAssignedSpecialPillValueChange(Action function)
    {
        //Checks if either the specified function is null, the list of functions to execute is null, or if the list of functions to execute does not contain the specified function and returns if so since there is nothing to do.
        if (function == null || functionsToExecuteOnAssignedSpecialPillValueChange == null || !functionsToExecuteOnAssignedSpecialPillValueChange.Contains(function))
            return;

        //Removes the specified function from the list of functions that need to be executed whenever the task's assigned special pill value changes.
        functionsToExecuteOnAssignedSpecialPillValueChange.Remove(function);

        //Checks if the list of functions that need to be executed whenever the task's assigned special pill value changes is empty and sets it to null if so in order to save memory.
        if (functionsToExecuteOnAssignedSpecialPillValueChange.Count == 0)
            functionsToExecuteOnAssignedSpecialPillValueChange = null;
    }
}

[System.Serializable]
public class NewGalaxySpecialPillTaskData
{
    public int ID = -1;
    public int assignedSpecialPillID = -1;

    public NewGalaxySpecialPillTaskData(NewGalaxySpecialPillTask specialPillTask)
    {
        ID = specialPillTask.ID;
        assignedSpecialPillID = specialPillTask.assignedSpecialPill == null ? -1 : specialPillTask.assignedSpecialPill.ID;
    }
}