using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GalaxyGroundUnit
{
    /// <summary>
    /// Indicates the name of the ground unit.
    /// </summary>
    public virtual string Name { get => name; set => name = value; }
    protected string name;

    /// <summary>
    /// Indicates exactly how much experience a ground unit has.
    /// </summary>
    public virtual float experience { get; set; }
    /// <summary>
    /// Indicates what level of experience the ground unit has.
    /// </summary>
    public virtual int experienceLevel { get; }
}
