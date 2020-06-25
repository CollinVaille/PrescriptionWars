using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Corpse : MonoBehaviour
{
    void Start ()
    {
        StartCoroutine(FindNiceRestingPlace());
    }

    private IEnumerator FindNiceRestingPlace ()
    {
        //Initial roll time
        yield return new WaitForSeconds(Random.Range(5.0f, 7.0f));

        //Wait until we have a good resting place
        while (!IsGrounded())
            yield return new WaitForSeconds(0.5f);

        //Now, stick pill in final resting place (no more rolling around)...

        //Push down into ground
        transform.Translate(Vector3.down * 0.6f, Space.World);

        //Get rid of rolling stuff
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<Collider>());
        Destroy(this);
    }

    private bool IsGrounded ()
    {
        return Physics.Raycast(transform.position, Vector3.down, 5);
    }
}
