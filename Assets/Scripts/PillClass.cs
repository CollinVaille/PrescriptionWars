using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PillClass
{
    [SerializeField] private string className;
    [SerializeField] private GameObject primary;
    [SerializeField] private GameObject secondary;
    [SerializeField, Tooltip("The material that will be applied to the capsule game object.")] private Material skin;
    [SerializeField] private GameObject bodyGear;
    [SerializeField] private GameObject headGear;
    [SerializeField, Tooltip("The probability that a pill of this class will spawn in on the planet view.")] private float probability;

    public PillClass(string className, GameObject primary, GameObject secondary, Material skin, GameObject bodyGear, GameObject headGear)
    {
        this.className = className;
        this.primary = primary;
        this.secondary = secondary;
        this.skin = skin;
        this.bodyGear = bodyGear;
        this.headGear = headGear;
    }

    //Returns the name of the pill class.
    public string GetClassName()
    {
        return className;
    }

    //Sets the name of the pill class to the specified name.
    public void SetClassName(string className)
    {
        this.className = className;
    }

    //Returns the primary weapon game object of the pill class.
    public GameObject GetPrimary()
    {
        return primary;
    }

    //Sets the primary weapon game object of the pill class to the specified primary weapon game object.
    public void SetPrimary(GameObject primary)
    {
        this.primary = primary;
    }

    //Returns the secondary weapon game object of the pill class.
    public GameObject GetSecondary()
    {
        return secondary;
    }

    //Sets the secondary weapon game object of the pill class to the specified secondary weapon game object.
    public void SetSecondary(GameObject secondary)
    {
        this.secondary = secondary;
    }

    //Returns the skin of the pill class.
    public Material GetSkin()
    {
        return skin;
    }

    //Sets the skin of the pill class to the specified skin.
    public void SetSkin(Material skin)
    {
        this.skin = skin;
    }

    //Returns the body gear game object of the pill class.
    public GameObject GetBodyGear()
    {
        return bodyGear;
    }

    //Sets the body gear of the pill class to the specified body gear.
    public void SetBodyGear(GameObject bodyGear)
    {
        this.bodyGear = bodyGear;
    }

    //Returns the head gear game object of the pill class.
    public GameObject GetHeadGear()
    {
        return headGear;
    }

    //Sets the head gear of the pill to the specified head gear.
    public void SetHeadGear(GameObject headGear)
    {
        this.headGear = headGear;
    }

    //Returns the probability of the pill class.
    public float GetProbability()
    {
        return probability;
    }

    //Sets the probability of the pill class to the specified value.
    public void SetProbability(float probability)
    {
        this.probability = probability;
    }
}
