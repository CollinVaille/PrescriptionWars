using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GalaxyGroundUnit
{
    /// <summary>
    /// Public property that should be used both to access and mutate the name of the ground unit.
    /// </summary>
    public virtual string name { get => nameVar; set => nameVar = value; }
    protected string nameVar;

    /// <summary>
    /// Indicates exactly how much experience a ground unit has.
    /// </summary>
    public virtual float experience { get; set; }
    /// <summary>
    /// Indicates what level of experience the ground unit has.
    /// </summary>
    public virtual int experienceLevel { get; }
}
