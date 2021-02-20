using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Confirmation Popup Components")]

    [SerializeField]
    private Text bodyText = null;

    //Non-inspector variables.

    public static GameObject galaxyConfirmationPopupPrefab;

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

    public void CreateConfirmationPopup(string popupTopText, string popupBodyText)
    {
        CreateConfirmationPopup(popupTopText);

        bodyText.text = popupBodyText;
    }
}
