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
    private GalaxyConfirmationPopupAnswer answer = GalaxyConfirmationPopupAnswer.Confirm;

    private bool answered = false;

    public static List<GalaxyConfirmationPopupBehaviour> galaxyConfirmationPopups = new List<GalaxyConfirmationPopupBehaviour>();

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

    protected void CreateConfirmationPopup(string popupTopText)
    {
        topText.text = popupTopText;

        transform.parent = GalaxyManager.galaxyConfirmationPopupParent;
        transform.localScale = Vector3.one;
        transform.localPosition = Vector3.zero;

        galaxyConfirmationPopups.Add(this);
    }

    public void DestroyConfirmationPopup()
    {
        galaxyConfirmationPopups.Remove(this);
        Destroy(gameObject);
    }

    public virtual void Confirm()
    {
        answer = GalaxyConfirmationPopupAnswer.Confirm;
        answered = true;
    }

    public void Cancel()
    {
        answer = GalaxyConfirmationPopupAnswer.Cancel;
        answered = true;
    }

    public void PlayMouseOverSFX()
    {
        AudioManager.PlaySFX(mouseOverSFX);
    }

    public void PlayMouseClickSFX()
    {
        AudioManager.PlaySFX(mouseClickSFX);
    }

    public static bool IsAGalaxyConfirmationPopupOpen()
    {
        return galaxyConfirmationPopups.Count > 0;
    }

    public bool IsAnswered()
    {
        return answered;
    }

    public GalaxyConfirmationPopupAnswer GetAnswer()
    {
        return answer;
    }
}
