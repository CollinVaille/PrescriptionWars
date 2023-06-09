using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetBotPrimitiveNavigation : PlanetBotNavigationMode
{
    public override void Deactivate() { }

    public override bool TryToActivate() { return true; }

    public override void PerformUpdate()
    {
        if(!navigation.atDestination)
        {
            //Look at target position
            transform.LookAt(navigation.targetGlobalPosition);

            //Move forward
            if (rBody.velocity.magnitude < bot.moveSpeed)
                rBody.AddForce(transform.forward * (bot.moveSpeed - rBody.velocity.magnitude), ForceMode.VelocityChange);
        }
    }

    public override bool CanContinue() { return true; }
}
