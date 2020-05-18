using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bed : MonoBehaviour
{
    private Pill occupant;

    public void GoToBed (Pill pill)
    {
        if (occupant || !pill || !pill.CanSleep())
            return;

        occupant = pill;
        occupant.controlOverride = true;

        occupant.transform.parent = transform.parent;
        occupant.transform.localPosition = Vector3.up * 0.25f;
        occupant.transform.localEulerAngles = new Vector3(-90, 180, 0);

        occupant.Sleep(this);
    }

    public void WakeUp ()
    {
        occupant.transform.localEulerAngles = new Vector3(0, 0, 0);
        occupant.transform.parent = null;

        occupant.controlOverride = false;
        occupant.WakeUp();

        occupant = null;
    }

    private void OnCollisionEnter (Collision collision)
    {
        Pill pill = collision.gameObject.GetComponent<Pill>();

        if (pill && !collision.gameObject.GetComponent<Player>())
            GoToBed(pill);
    }
}
