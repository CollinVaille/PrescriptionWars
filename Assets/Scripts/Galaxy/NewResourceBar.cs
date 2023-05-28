using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewResourceBar : MonoBehaviour
{
    [Header("Image Components")]

    [SerializeField] private Image flagImage = null;

    [Header("Text Components")]

    [SerializeField] private Text turnNumberText = null;
    [SerializeField] private Text creditsText = null;
    [SerializeField] private Text prescriptionsText = null;

    [Header("Tooltip Components")]

    [SerializeField] private GalaxyTooltip empireNameTooltip = null;

    //Non-inspector variables.

    /// <summary>
    /// Private static resource bar instance variable that is initialized in the awake method.
    /// </summary>
    private static NewResourceBar resourceBar = null;

    /// <summary>
    /// Public static property that should be used in order to access the height of the resource bar's rect transform.
    /// </summary>
    public static float resourceBarHeight { get => ((RectTransform)resourceBar.transform).sizeDelta.y; }

    private void Awake()
    {
        //Initializes the static instance variable.
        resourceBar = this;
        //Adds the private OnGalaxyGenerationCompletion function to the list of functions to be executed once the galaxy finishes generating with an execution order number of 2.
        NewGalaxyGenerator.ExecuteFunctionOnGalaxyGenerationCompletion(OnGalaxyGenerationCompletion, 2);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Public static method that should be called in order to update the empire name tooltip.
    /// </summary>
    public static void UpdateEmpireNameTooltip()
    {
        //Logs a warning and returns if the resource bar static instance variable is null and has not been initialized in the awake method.
        if(resourceBar == null)
        {
            Debug.LogWarning("Cannot update the empire name tooltip on the resource bar because the static instance variable has not been initialzed in the awake method yet.");
            return;
        }
        //Updates the empire name tooltip's text to match the player empire's name.
        resourceBar.empireNameTooltip.Text = NewGalaxyManager.playerEmpire.name;
    }

    /// <summary>
    /// Public static method that should be called in order to update the flag.
    /// </summary>
    public static void UpdateFlag()
    {
        //Logs a warning and returns if the resource bar static instance variable is null and has not been initialized in the awake method.
        if (resourceBar == null)
        {
            Debug.LogWarning("Cannot update the flag on the resource bar because the static instance variable has not been initialzed in the awake method yet.");
            return;
        }
        //Updates the flag image sprite to match the player empire's flag sprite.
        resourceBar.flagImage.sprite = NewGalaxyManager.playerEmpire.flag.sprite;
    }

    /// <summary>
    /// Public static method that should be called in order to update the amount of credits that the resource bar shows the player as having.
    /// </summary>
    public static void UpdateCredits()
    {
        //Logs a warning and returns if the resource bar static instance variable is null and has not been initialized in the awake method.
        if (resourceBar == null)
        {
            Debug.LogWarning("Cannot update the credits on the resource bar because the static instance variable has not been initialzed in the awake method yet.");
            return;
        }
        //Updates the credits text to accurately display the number of credits that the player empire has.
        resourceBar.creditsText.text = Mathf.Floor(NewGalaxyManager.playerEmpire.credits) + " +" + (int)NewGalaxyManager.playerEmpire.GetResourcePerTurn(GalaxyResourceType.Credits);
    }

    /// <summary>
    /// Public static method that should be called in order to update the amount of prescriptions that the resource bar shows the player as having.
    /// </summary>
    public static void UpdatePrescriptions()
    {
        //Logs a warning and returns if the resource bar static instance variable is null and has not been initialized in the awake method.
        if (resourceBar == null)
        {
            Debug.LogWarning("Cannot update the prescriptions on the resource bar because the static instance variable has not been initialzed in the awake method yet.");
            return;
        }
        //Updates the prescriptions text to accurately display the number of prescriptions that the player empire has.
        resourceBar.prescriptionsText.text = Mathf.Floor(NewGalaxyManager.playerEmpire.prescriptions) + " +" + (int)NewGalaxyManager.playerEmpire.GetResourcePerTurn(GalaxyResourceType.Prescriptions);
    }

    /// <summary>
    /// Public static method that should be called in order to update all empire dependent components on the resource bar (such as the flag and empire name tooltip).
    /// </summary>
    public static void UpdateAllEmpireDependentComponents()
    {
        UpdateEmpireNameTooltip();
        UpdateFlag();
        UpdateCredits();
        UpdatePrescriptions();
    }

    /// <summary>
    /// Public static method that should be called in order to update all resource dependent components on the resource bar (such as the credits text and prescriptions text).
    /// </summary>
    public static void UpdateAllResourceDependentComponents()
    {
        UpdateCredits();
        UpdatePrescriptions();
    }

    /// <summary>
    /// Public static method that should be called in order to update the text component on the resource bar that displays the number of turns that have passed since the current game started.
    /// </summary>
    public static void UpdateTurnNumberText()
    {
        //Logs a warning and returns if the resource bar static instance variable is null and has not been initialized in the awake method.
        if (resourceBar == null)
        {
            Debug.LogWarning("Cannot update the turn number on the resource bar because the static instance variable has not been initialzed in the awake method yet.");
            return;
        }
        //Updates the turn number text to accurately display the number of turns that have passed since the start of the current game.
        resourceBar.turnNumberText.text = "Turn: " + NewGalaxyManager.turnNumber;
    }

    /// <summary>
    /// Private method that is called by the galaxy generator whenever the galaxy is finished generating.
    /// </summary>
    private void OnGalaxyGenerationCompletion()
    {
        UpdateFlag();
        UpdateEmpireNameTooltip();
        UpdateCredits();
        UpdatePrescriptions();
        UpdateTurnNumberText();
    }

    /// <summary>
    /// This private method is called whenever the resource bar is destroyed in the project hierarchy and resets the static instance variable to null.
    /// </summary>
    private void OnDestroy()
    {
        //Resets the static instance variable.
        resourceBar = null;
    }
}
