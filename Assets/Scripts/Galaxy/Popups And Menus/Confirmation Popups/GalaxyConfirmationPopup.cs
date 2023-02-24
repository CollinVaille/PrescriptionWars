using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Confirmation Popup Components")]

    [SerializeField] private Text bodyText = null;

    [SerializeField] private Button confirmButton = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private Button okayButton = null;

    //Non-inspector variables.

    /// <summary>
    /// Public static property that should be accessed in order to obtain the confirmation popup prefab that all confirmation popups should be instantiated from.
    /// </summary>
    public static GameObject confirmationPopupPrefab { get => Resources.Load<GameObject>("Galaxy/Prefabs/Confirmation Popups/Confirmation Popup"); }

    /// <summary>
    /// Indicates whether the confirmation popup can have only one possible answer (answer = okay, which is acknowledgement, answer is technically cancel).
    /// </summary>
    public bool isAcknowledgementOnly
    {
        get
        {
            return !confirmButton.gameObject.activeInHierarchy && !cancelButton.gameObject.activeInHierarchy && okayButton.gameObject.activeInHierarchy;
        }
        set
        {
            confirmButton.gameObject.SetActive(!value);
            cancelButton.gameObject.SetActive(!value);
            okayButton.gameObject.SetActive(value);
        }
    }

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

    public void CreateConfirmationPopup(string popupTopText, string popupBodyText, bool acknowledgementOnly = false)
    {
        CreateConfirmationPopup(popupTopText);

        bodyText.text = popupBodyText;
        isAcknowledgementOnly = acknowledgementOnly;
    }
}
