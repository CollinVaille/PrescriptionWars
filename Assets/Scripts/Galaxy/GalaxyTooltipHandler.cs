using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyTooltipHandler : MonoBehaviour, IGalaxyTooltipHandler
{
    [SerializeField] private Transform _tooltipsParent = null;
    public Transform tooltipsParent { get => _tooltipsParent; }
}
