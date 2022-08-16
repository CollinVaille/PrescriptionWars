using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyPill: GalaxyGroundUnit
{
    //Constructors for the class.
    public GalaxyPill(string name, PillClass pillClass) : this(name, pillClass != null ? pillClass.className : string.Empty)
    {
        
    }

    public GalaxyPill(string name, string className)
    {
        //Assigns the pill the specified name.
        this.nameVar = name;
        //Assigns the pill the specified class.
        this.className = className;
        //Assigns the type of the pill to the initial pill type specified in the PillClass of the pill.
        if (pillClass != null)
            pillTypeVar = pillClass.initialPillType;
        //Assigns the lowest possible experience level to the pill.
        experience = experienceBounds.x;
    }

    public override string name
    {
        get => base.name;
        set
        {
            base.name = value;
            if (specialPill != null)
                specialPill.name = value;
        }
    }

    /// <summary>
    /// Indicates the amount of experience that the pill has.
    /// </summary>
    private float experienceVar;
    public override float experience
    {
        get
        {
            return experienceVar;
        }
        set
        {
            experienceVar = value;
            if (experienceVar < experienceBounds.x)
                experienceVar = experienceBounds.x;
            else if (experienceVar > experienceBounds.y)
                experienceVar = experienceBounds.y;
            if (specialPill != null)
                specialPill.SetExperience(GalaxySpecialPill.Skill.Soldiering, experienceVar);
        }
    }
    public override int experienceLevel { get => (int)experience; }
    /// <summary>
    /// The x value indicates the minimum amount of experience that a pill can have while the y value indicates the maximum amount of experience that a pill can have, the final x and y values depend on empire bonuses.
    /// </summary>
    public Vector2Int experienceBounds { get => new Vector2Int(1 + (assignedSquad != null && assignedSquad.assignedArmy != null && assignedSquad.assignedArmy.owner != null ? assignedSquad.assignedArmy.owner.pillExperienceBoundingEffects.x : 0), 5 + (assignedSquad != null && assignedSquad.assignedArmy != null && assignedSquad.assignedArmy.owner != null ? assignedSquad.assignedArmy.owner.pillExperienceBoundingEffects.y : 0)); }
    /// <summary>
    /// The x value indicates the minimum amount of experience that a pill can have while the y value indicates the maximum amount of experience that a pill can have (not factoring in any empire bonuses or really any bonuses at all).
    /// </summary>
    public static readonly Vector2Int baseExperienceBounds = new Vector2Int(1, 5);

    /// <summary>
    /// The class of the pill contains the primary and secondary weapon game objects and the head gear and body gear game objects.
    /// </summary>
    public PillClass pillClass
    {
        get => PillClass.pillClasses.ContainsKey(className) ? PillClass.pillClasses[className] : null;
        set
        {
            className = value != null ? value.className : null;
            if (specialPill != null)
                specialPill.pillClass = pillClass;
        }
    }
    /// <summary>
    /// Indicates what type of class the class of the pill is (ex: Assault or Officer).
    /// </summary>
    public PillClassType pillClassType { get => PillClass.pillClasses[className].classType; }
    private string className = null;

    /// <summary>
    /// Indicates what type of pill the pill is (Example: Player or Bot1).
    /// </summary>
    public PillType pillType
    {
        get => pillTypeVar;
        set
        {
            pillTypeVar = value;
            if (specialPill != null)
                specialPill.pillType = pillType;
        }
    }
    private PillType pillTypeVar = PillType.Bot1;

    /// <summary>
    /// The squad that this pill is assigned to.
    /// </summary>
    public GalaxySquad assignedSquad
    {
        get => assignedSquadVar;
        set
        {
            GalaxySquad previousSquad = assignedSquadVar;
            GalaxySpecialPill specialPillTemp = specialPill;
            assignedSquadVar = value;
            specialPillTemp = specialPill != null ? specialPill : specialPillTemp;
            if (specialPillTemp != null && specialPillTemp.isBusy && (specialPillTemp.task.Equals("Serving In A Squad") || (previousSquad != null && specialPillTemp.task.Equals("Serving In " + previousSquad.name))))
            {
                specialPillTemp.task = value == null ? "Serving In A Squad" : "Serving In " + value.name;
            }
        }
    }
    private GalaxySquad assignedSquadVar = null;

    /// <summary>
    /// Indicates whether the pill is in a squad and is the leader of said squad.
    /// </summary>
    public bool isSquadLeader { get => assignedSquad != null ? assignedSquad.squadLeader == this : false; }

    /// <summary>
    /// Returns the skin material of the pill that is specified either in the squad (checked first) or the army.
    /// </summary>
    public Material skin
    {
        get
        {
            //Returns the pill skin assigned to the squad if the squad is a special squad.
            if (assignedSquadVar.assignedPillSkinName != null)
                return Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(assignedSquad.assignedArmy.owner.empireCulture.ToString()) + "/" + assignedSquad.assignedPillSkinName);
            //Else if the squad is not a special squad then it returns the pill skin assigned to the army.
            return Resources.Load<Material>("Planet/Pill Skins/" + GeneralHelperMethods.GetEnumText(assignedSquad.assignedArmy.owner.empireCulture.ToString()) + "/" + assignedSquad.assignedArmy.assignedPillSkinName);
        }
    }

    /// <summary>
    /// Indicates whether the secondary weapon game object of the pill should be visible in pill views.
    /// </summary>
    public bool isSecondaryVisible { get => pillTypeVar == PillType.Player; }

    /// <summary>
    /// Public property that should be used both to access the galaxy pill's special pill if it exists (returns null if it does not exist).
    /// </summary>
    public GalaxySpecialPill specialPill { get => assignedSquad != null && assignedSquad.assignedArmy != null && assignedSquad.assignedArmy.owner != null ? assignedSquad.assignedArmy.owner.GetSpecialPill(specialPillID) : null; set => specialPillID = value == null ? -1 : value.specialPillID; }
    private int specialPillID = -1;
}