using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewGalaxyPauseMenu : NewGalaxyPopupBehaviour
{
    [Header("Text Components")]

    [SerializeField, Tooltip("The text component at the bottom of the pause menu that displays the current game version to the user.")] private Text versionText = null;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();

        //Sets the version text.
        versionText.text = "Version: " + Application.version;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
