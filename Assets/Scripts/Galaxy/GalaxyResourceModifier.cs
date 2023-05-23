using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyResourceModifier
{
    /// <summary>
    /// Private variable that is used to hold the integer value that represents the resource modifier's ID in the global dictionary of resource modifiers in the galaxy manager.
    /// </summary>
    private int _ID = -1;
    /// <summary>
    /// Public property that should be accessed in order to obtain the integer value that represents the resource modifier's ID in the global dictionary of resource modifiers in the galaxy manager.
    /// </summary>
    public int ID { get => _ID; }

    /// <summary>
    /// Private variable that is used to hold the enum value that represents the type of resource that the modifier is affecting.
    /// </summary>
    private GalaxyResourceType _resourceType = 0;
    /// <summary>
    /// Public property that should be used in order to both access and mutate the enum value that represents the type of resource that the modifier is affecting.
    /// </summary>
    public GalaxyResourceType resourceType
    {
        get => _resourceType;
        set
        {
            GalaxyResourceType previousResourceType = resourceType;
            _resourceType = value;
            if(appliedEmpire != null)
            {
                appliedEmpire.resourceModifiers[previousResourceType][mathematicalOperation].Remove(this);
                appliedEmpire.resourceModifiers[resourceType][mathematicalOperation].Add(this);
                appliedEmpire.UpdateResourcePerTurnForResourceType(previousResourceType);
                appliedEmpire.UpdateResourcePerTurnForResourceType(resourceType);
            }
        }
    }

    public enum MathematicalOperation
    {
        /// <summary>
        /// The resource modifier's amount is added to the other resource modifier addition amounts before multiplication is performed on the summed addition amounts.
        /// </summary>
        Addition = 0,
        /// <summary>
        /// The resource modifier's amount is added to the other resource modifier additive multipication amounts for the same empire before then multiplying the addition resource modifier sum.
        /// </summary>
        AdditiveMultiplication = 1,
        /// <summary>
        /// The resource modifier's amount directly multiplies the total resource modifier amount.
        /// </summary>
        MultiplicativeMultiplication = 2
    }
    /// <summary>
    /// Private variable that is used to hold the enum value that represents the type of mathematical operation that should be performed with the resource modifier's amount.
    /// </summary>
    private MathematicalOperation _mathematicalOperation = 0;
    /// <summary>
    /// Public property that should be used in order to both access and mutate the enum value that represents the type of mathematical operation that should be performed with the resource modifier's amount.
    /// </summary>
    public MathematicalOperation mathematicalOperation
    {
        get => _mathematicalOperation;
        set
        {
            MathematicalOperation previousMathematicalOperation = mathematicalOperation;
            _mathematicalOperation = value;
            if(appliedEmpire != null)
            {
                appliedEmpire.resourceModifiers[resourceType][previousMathematicalOperation].Remove(this);
                appliedEmpire.resourceModifiers[resourceType][mathematicalOperation].Add(this);
                appliedEmpire.UpdateResourcePerTurnForResourceType(resourceType);
            }
        }
    }

    /// <summary>
    /// Private variable that is used to hold the float value that represents the amount that has the mathematical operation applied to it.
    /// </summary>
    private float _amount = 0;
    /// <summary>
    /// Public property that should be used in order to both access and mutate the float value that represents the amount that has the mathematical operation applied to it.
    /// </summary>
    public float amount
    {
        get => _amount;
        set
        {
            _amount = value;
            if(appliedEmpire != null)
                appliedEmpire.UpdateResourcePerTurnForResourceType(resourceType);
        }
    }

    /// <summary>
    /// Private variable that is used to hold the empire that has the modifier applied to its resources.
    /// </summary>
    private int _appliedEmpireID = -1;
    /// <summary>
    /// Public property that should be used in order to both access and mutate the empire that has the modifier applied to its resources.
    /// </summary>
    public NewEmpire appliedEmpire
    {
        get => _appliedEmpireID >= 0 && _appliedEmpireID < NewGalaxyManager.empires.Count ? NewGalaxyManager.empires[_appliedEmpireID] : null;
        set
        {
            //If previously applied to another empire, then remove the resource modifier from the dictionary of resource modifiers affecting said previous empire.
            if(appliedEmpire != null)
            {
                appliedEmpire.resourceModifiers[resourceType][mathematicalOperation].Remove(this);
                appliedEmpire.UpdateResourcePerTurnForResourceType(resourceType);
            }
            //Set the resource modifier as being applied to the newly specified empire.
            _appliedEmpireID = value == null ? -1 : value.ID;
            if(appliedEmpire != null)
            {
                appliedEmpire.resourceModifiers[resourceType][mathematicalOperation].Add(this);
                appliedEmpire.UpdateResourcePerTurnForResourceType(resourceType);
            }
        }
    }

    public GalaxyResourceModifier(GalaxyResourceType resourceType, MathematicalOperation mathematicalOperation, float amount, NewEmpire appliedEmpire)
    {
        _resourceType = resourceType;
        _mathematicalOperation = mathematicalOperation;
        _amount = amount;

        if (NewGalaxyManager.activeInHierarchy)
        {
            _ID = NewGalaxyManager.resourceModifiersCount;
            NewGalaxyManager.resourceModifiers.Add(_ID, this);
            NewGalaxyManager.resourceModifiersCount++;
            this.appliedEmpire = appliedEmpire;
        }
        else
        {
            _appliedEmpireID = appliedEmpire == null ? -1 : appliedEmpire.ID;
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 1);
        }
    }

    public GalaxyResourceModifier(GalaxyResourceModifierData resourceModifierData)
    {
        _ID = resourceModifierData.ID;
        _resourceType = resourceModifierData.resourceType;
        _mathematicalOperation = resourceModifierData.mathematicalOperation;
        _amount = resourceModifierData.amount;

        if (NewGalaxyManager.activeInHierarchy)
        {
            appliedEmpire = resourceModifierData.appliedEmpireID >= 0 && resourceModifierData.appliedEmpireID < NewGalaxyManager.empires.Count ? NewGalaxyManager.empires[resourceModifierData.appliedEmpireID] : null;
        }
        else
        {
            _appliedEmpireID = resourceModifierData.appliedEmpireID;
            NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 1);
        }
    }

    /// <summary>
    /// Private method that is executed by the galaxy generator whenever the galaxy is finished generating and the method has a priority index of 0 (meaning it is top priority and can be done immediately). The method properly sets the ID (if necessary) and apllied empire of the resource modifier.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        if(_ID < 0)
        {
            _ID = NewGalaxyManager.resourceModifiersCount;
            NewGalaxyManager.resourceModifiers.Add(_ID, this);
            NewGalaxyManager.resourceModifiersCount++;
        }
        appliedEmpire = NewGalaxyManager.empires[_appliedEmpireID];
    }
}

public enum GalaxyResourceType
{
    Credits = 0,
    Prescriptions = 1,
    Science = 2,
    Production = 3
}

[System.Serializable]
public class GalaxyResourceModifierData
{
    public int ID = -1;
    public GalaxyResourceType resourceType = 0;
    public GalaxyResourceModifier.MathematicalOperation mathematicalOperation = 0;
    public float amount = 0;
    public int appliedEmpireID = -1;

    public GalaxyResourceModifierData(GalaxyResourceModifier resourceModifier)
    {
        ID = resourceModifier.ID;
        resourceType = resourceModifier.resourceType;
        mathematicalOperation = resourceModifier.mathematicalOperation;
        amount = resourceModifier.amount;
        appliedEmpireID = resourceModifier.appliedEmpire != null ? resourceModifier.appliedEmpire.ID : -1;
    }
}