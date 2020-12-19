using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyConfirmationPopup : MonoBehaviour
{
    public enum GalaxyConfirmationPopupAnswer
    {
        Confirm,
        Cancel
    }
    public GalaxyConfirmationPopupAnswer answer;

    public Text topText;
    public Text bodyText;

    public AudioClip mouseOverSFX;
    public AudioClip mouseClickSFX;

    public static List<GalaxyConfirmationPopup> galaxyConfirmationPopups = new List<GalaxyConfirmationPopup>();

    bool answered = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
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

    public void CreateConfirmationPopup(string popupTopText, string popupBodyText)
    {
        topText.text = popupTopText;
        bodyText.text = popupBodyText;

        transform.parent = GalaxyManager.galaxyConfirmationPopupParent;
        transform.localScale = new Vector3(1, 1, 1);
        transform.localPosition = new Vector3(0, 0, 0);

        galaxyConfirmationPopups.Add(this);
    }

    public void DestroyConfirmationPopup()
    {
        galaxyConfirmationPopups.Remove(this);
        Destroy(gameObject);
    }

    public void Confirm()
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
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseOverSFX);
    }

    public void PlayMouseClickSFX()
    {
        GalaxyManager.galaxyManager.sfxSource.PlayOneShot(mouseClickSFX);
    }

    public static bool IsAGalaxyConfirmationPopupOpen()
    {
        return galaxyConfirmationPopups.Count > 0;
    }

    public bool IsAnswered()
    {
        return answered;
    }
}
