using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSettingsMenu : GalaxyMenuBehaviour
{
    [Header("Settings Menu")]

    [SerializeField]
    private AudioSettingsMenu audioSettingsMenu = null;
    [SerializeField]
    private VideoSettingsMenu videoSettingsMenu = null;

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

    public void ClickAudioSettingsButton()
    {
        audioSettingsMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ClickVideoSettingsButton()
    {
        videoSettingsMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }
}
