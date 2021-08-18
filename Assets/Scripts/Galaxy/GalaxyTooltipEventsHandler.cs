using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GalaxyTooltipEventsHandler: MonoBehaviour
{
    /// <summary>
    /// This method is called by the GalaxyTooltip class whenever the tooltip is opened.
    /// </summary>
    public virtual void OnTooltipOpen(GalaxyTooltip tooltip)
    {

    }

    /// <summary>
    /// This method is called by the GalaxyTooltip class whenever the tooltip is closed.
    /// </summary>
    public virtual void OnTooltipClose(GalaxyTooltip tooltip)
    {

    }
}
