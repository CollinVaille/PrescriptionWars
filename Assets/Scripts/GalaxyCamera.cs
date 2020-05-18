using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyCamera : MonoBehaviour
{
    public float moveSpeed = 50;

    private Vector3 movementVector = Vector3.zero;
    private Vector3 previousMousePosition = Vector3.zero;

    private void Update ()
    {
        
        //Speed up/slow down
        if (Input.GetButton("Sprint"))
            moveSpeed += 25 * Time.deltaTime;
        else if (Input.GetButton("Equip"))
            moveSpeed -= 25 * Time.deltaTime;

        //WASD and scrollwheel movement
        movementVector.x = Input.GetAxis("Horizontal");
        movementVector.y = Input.GetAxis("Vertical");
        movementVector.z = Input.GetAxis("Mouse ScrollWheel") * 40;

        //Click and drag movement
        if (Input.GetMouseButton(0))
        {
            movementVector.x += (previousMousePosition.x - Input.mousePosition.x) / 20.0f;
            movementVector.y += (previousMousePosition.y - Input.mousePosition.y) / 20.0f;
        }

        //Apply movement
        transform.Translate(movementVector * moveSpeed * Time.deltaTime);

        //Clean up at end of update
        previousMousePosition = Input.mousePosition;
    }
}
