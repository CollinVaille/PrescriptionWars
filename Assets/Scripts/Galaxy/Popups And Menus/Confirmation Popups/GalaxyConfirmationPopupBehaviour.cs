using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyConfirmationPopupBehaviour : MonoBehaviour
{
    [Header("Confirmation Popup Base Components")]

    [SerializeField] private Text topText = null;

    [Header("Confirmation Popup Base Audio Options")]

    [SerializeField] private AudioClip mouseOverSFX = null;
    [SerializeField] private AudioClip mouseClickSFX = null;

    //Non-inspector variables.
    public enum GalaxyConfirmationPopupAnswer
    {
        Confirm,
        Cancel
    }
    /// <summary>
    /// Publicly accessible property that should be accessed in order to determine whether the player chooses to either confirm or cancel their initially desired action.
    /// </summary>
    public GalaxyConfirmationPopupAnswer answer { get; protected set; } = GalaxyConfirmationPopupAnswer.Confirm;

    /// <summary>
    /// Private holder variable that indicates whether the confirmation popup has been answered yet.
    /// </summary>
    private bool answered = false;

    /// <summary>
    /// Private static list that contains all confirmation popups that are currently open.
    /// </summary>
    private static List<GalaxyConfirmationPopupBehaviour> galaxyConfirmationPopups = null;

    /// <summary>
    /// Public static boolean that should be accessed in order to determine whether a confirmation popup is open.
    /// </summary>
    public static bool isAConfirmationPopupOpen { get => galaxyConfirmationPopups != null && galaxyConfirmationPopups.Count > 0; }

    // Start is called before the first frame update
    public virtual void Start()
    {

    }

    // Update is called once per frame
    public virtual void Update()
    {
        //If the user presses escape while the pop-up is active, then it will cancel whatever action it was querying the user about.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cancel();
        }
        //If the user presses return (enter) or keypad eneter while the pop-up is active, then it will confirm whatever action it was querying the user about and thus carry it out.
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            Confirm();
        }
    }

    /// <summary>
    /// Public method that should be called in order to create the actual confirmation popup and specify its top text.
    /// </summary>
    /// <param name="popupTopText"></param>
    public virtual void CreateConfirmationPopup(string popupTopText)
    {
        topText.text = popupTopText;

        transform.parent = NewGalaxyManager.confirmationPopupsParent;
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;

        if (galaxyConfirmationPopups == null)
            galaxyConfirmationPopups = new List<GalaxyConfirmationPopupBehaviour>();
        galaxyConfirmationPopups.Add(this);
    }

    /// <summary>
    /// Public method that should be called in order to destroy the confirmation popup and its assigned game object.
    /// </summary>
    public void DestroyConfirmationPopup()
    {
        Destroy(gameObject);
    }

    /// <summary>
    /// Public method that should be called through an event trigger in the inspector whenever the player presses the confirm button on the confirmation popup.
    /// </summary>
    public virtual void Confirm()
    {
        answer = GalaxyConfirmationPopupAnswer.Confirm;
        answered = true;
    }

    /// <summary>
    /// Public method that should be called through an event trigger in the inspector whenever the player presses the cancel button on the confirmation popup.
    /// </summary>
    public void Cancel()
    {
        answer = GalaxyConfirmationPopupAnswer.Cancel;
        answered = true;
    }

    /// <summary>
    /// Public method that should be called through an event trigger in the inspector whenever the player mouses over a button on the confirmation popup.
    /// </summary>
    public void PlayMouseOverSFX()
    {
        AudioManager.PlaySFX(mouseOverSFX);
    }

    /// <summary>
    /// Public method that should be called through an event trigger in the inspector whenever the player clicks a button on the confirmation popup.
    /// </summary>
    public void PlayMouseClickSFX()
    {
        AudioManager.PlaySFX(mouseClickSFX);
    }

    /// <summary>
    /// Public method that returns a boolean that indicates whether or not the player has answered the confirmation popup yet and pressed either the confirm or cancel option. This method should be used in coroutines that wait for the confirmation popup to be answered (like: WaitUntil(confirmationPopup.IsAnswered)).
    /// </summary>
    /// <returns></returns>
    public bool IsAnswered()
    {
        return answered;
    }

    /// <summary>
    /// Protected method that is called whenever the confirmation popup is destroyed and it removes this confirmation popup from the static list of all open confirmation popups.
    /// </summary>
    protected virtual void OnDestroy()
    {
        galaxyConfirmationPopups.Remove(this);
        if (galaxyConfirmationPopups.Count == 0)
            galaxyConfirmationPopups = null;
    }
}
