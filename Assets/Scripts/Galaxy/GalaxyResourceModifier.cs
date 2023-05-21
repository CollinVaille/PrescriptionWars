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
            _resourceType = value;
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
            _mathematicalOperation = value;
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
        }
    }

    /// <summary>
    /// Private variable that is used to hold the empire that has the modifier applied to its resources.
    /// </summary>
    private NewEmpire _appliedEmpire = null;
    /// <summary>
    /// Public property that should be used in order to both access and mutate the empire that has the modifier applied to its resources.
    /// </summary>
    public NewEmpire appliedEmpire
    {
        get => _appliedEmpire;
        set
        {
            _appliedEmpire = value;
        }
    }

    public GalaxyResourceModifier(GalaxyResourceType resourceType, MathematicalOperation mathematicalOperation, float amount, NewEmpire appliedEmpire)
    {
        _resourceType = resourceType;
        _mathematicalOperation = mathematicalOperation;
        _amount = amount;

        this.appliedEmpire = appliedEmpire;
    }

    public GalaxyResourceModifier(GalaxyResourceModifierData resourceModifierData) : this(resourceModifierData.resourceType, resourceModifierData.mathematicalOperation, resourceModifierData.amount, NewGalaxyManager.empires[resourceModifierData.appliedEmpireID])
    {

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