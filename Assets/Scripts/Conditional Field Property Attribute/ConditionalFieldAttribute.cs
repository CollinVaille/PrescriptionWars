﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ConditionalFieldAttribute : PropertyAttribute
{
    public string comparedPropertyName { get; private set; }
    public object comparedValue { get; private set; }
    public ConditionalFieldComparisonType comparisonType { get; private set; }
    public ConditionalFieldDisablingType disablingType { get; private set; }

    /// <summary>
    /// Only draws the field only if a condition is met.
    /// </summary>
    /// <param name="comparedPropertyName">The name of the property that is being compared (case sensitive).</param>
    /// <param name="comparedValue">The value the property is being compared to.</param>
    /// <param name="comparisonType">The type of comperison the values will be compared by.</param>
    /// <param name="disablingType">The type of disabling that should happen if the condition is NOT met. Defaulted to DisablingType.DontDraw.</param>
    public ConditionalFieldAttribute(string comparedPropertyName, object comparedValue, ConditionalFieldComparisonType comparisonType, ConditionalFieldDisablingType disablingType = ConditionalFieldDisablingType.Disappear)
    {
        this.comparedPropertyName = comparedPropertyName;
        this.comparedValue = comparedValue;
        this.comparisonType = comparisonType;
        this.disablingType = disablingType;
    }
}
