using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Interface for objects that have the following properties:
//1. Their lifetime is very short.
//2. There can be a lot of them, even at once. So having a separate coroutine for each could be excessive.
//3. Only need to be updated so many times a second. Updating them anymore like in Update or FixedUpdate would waste CPU cycles.

//Examples are projectiles and death rays.
//For such objects, they can implement this interface and use the single method required as their update function.
//The central singleton God script will manage calling the method. Just use God.god.ManageVolatileObject and God.god.UnmanageVolatileObject as needed.

public interface ManagedVolatileObject
{
    void UpdateActiveStatus(float stepTime);
}
