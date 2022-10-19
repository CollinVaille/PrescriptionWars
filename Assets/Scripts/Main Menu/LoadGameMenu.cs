using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadGameMenu : GalaxyMenuBehaviour
{
    /// <summary>
    /// Holds the save game data that is passed over to the galaxy view. Only populated once the player presses the load game button.
    /// </summary>
    public static GalaxyData saveGameData = null;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }
}
