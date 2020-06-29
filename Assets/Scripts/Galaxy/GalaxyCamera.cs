using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyCamera : MonoBehaviour
{
    public float moveSpeed = 50;

    private Vector3 movementVector = Vector3.zero;
    private Vector3 previousMousePosition = Vector3.zero;

    public InputField consoleInputField;

    public GameObject planetManagementMenu;

    public bool mouseOverPlanetManagementMenu;

    private void Update ()
    {

        //Speed up/slow down
        /*if (Input.GetButton("Sprint"))
            moveSpeed += 25 * Time.deltaTime;
        else if (Input.GetButton("Equip"))
            moveSpeed -= 25 * Time.deltaTime;*/

        //WASD and scrollwheel movement
        if (!consoleInputField.isFocused)
        {
            movementVector.x = Input.GetAxis("Horizontal");
            movementVector.y = Input.GetAxis("Vertical");
        }
        else
        {
            movementVector.x = 0.0f;
            movementVector.y = 0.0f;
        }
        movementVector.z = Input.GetAxis("Mouse ScrollWheel") * 40;

        //Click and drag movement
        if (!planetManagementMenu.activeInHierarchy)
            mouseOverPlanetManagementMenu = false;

        if (Input.GetMouseButton(0) && !mouseOverPlanetManagementMenu)
        {
            movementVector.x += (previousMousePosition.x - Input.mousePosition.x) / 20.0f;
            movementVector.y += (previousMousePosition.y - Input.mousePosition.y) / 20.0f;
        }

        //Apply movement
        transform.Translate(movementVector * moveSpeed * Time.deltaTime);

        //Camera restrictions
        if (transform.position.y > 300)
            transform.position = new Vector3(transform.position.x, 300, transform.position.z);
        else if (transform.position.y < 50)
            transform.position = new Vector3(transform.position.x, 50, transform.position.z);
        if (transform.position.x < transform.position.y - 75)
            transform.position = new Vector3(transform.position.y - 75, transform.position.y, transform.position.z);
        else if (transform.position.x > 300 - transform.position.y + 225)
            transform.position = new Vector3(300 - transform.position.y + 225, transform.position.y, transform.position.z);
        if (transform.position.z < ((transform.position.y / 50 * 0.0733f + 1) * 0.3f * transform.position.y))
            transform.position = new Vector3(transform.position.x, transform.position.y, ((transform.position.y / 50 * 0.0733f + 1) * 0.3f * transform.position.y));
        else if (transform.position.z > 275 - ((transform.position.y - 50) / 250 * 145.418f))
            transform.position = new Vector3(transform.position.x, transform.position.y, 275 - ((transform.position.y - 50) / 250 * 145.418f));

        //Clean up at end of update
        previousMousePosition = Input.mousePosition;
    }

    public void ToggleMouseOverPlanetManagementMenu()
    {
        mouseOverPlanetManagementMenu = !mouseOverPlanetManagementMenu;
    }
}
