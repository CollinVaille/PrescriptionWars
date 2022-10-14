using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyViewBehaviour: MonoBehaviour, IGalaxyTooltipHandler, IGalaxyPopupBehaviourHandler
{
    [Header("Galaxy View Galaxy Tooltip Handler")]

    [SerializeField, LabelOverride("Tooltips Parent")] private Transform tooltipsParentVar = null;
    public Transform tooltipsParent { get => tooltipsParentVar; }

    [Header("Galaxy View Galaxy Popup Handler")]

    [SerializeField] private Vector2 popupScreenBoundsVar = new Vector2(291, 99);
    public Vector2 popupScreenBounds { get => popupScreenBoundsVar; }

    [Header("Galaxy View Audio Options")]

    [SerializeField]
    private AudioSource sfxSource = null;

    //Non-inspector variables.

    //Indicates whether a popup has been closed on this frame on this view.
    private bool popupClosedOnFrameVar = false;
    public bool popupClosedOnFrame { get => popupClosedOnFrameVar; set => popupClosedOnFrameVar = value; }

    //Indicates whether the audio sources have been set already on the current frame.
    private bool audioSourcesAlreadySetOnFrame = false;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        //Sets the sfx and music audio sources of the view.
        if (!audioSourcesAlreadySetOnFrame)
        {
            AudioManager.SetSFXSource(sfxSource);
            audioSourcesAlreadySetOnFrame = true;
        }
    }

    protected virtual void Awake()
    {
        //Makes sure that the audio settings have been loaded in.
        if (!AudioSettings.loaded)
            AudioSettings.LoadSettings();
        //Makes sure that video seetings have been loaded in.
        if (!VideoSettings.loaded)
            VideoSettings.LoadSettings();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Resets the variable that indicates whether the audio sources have been set already on the current frame.
        audioSourcesAlreadySetOnFrame = false;
    }

    protected virtual void OnEnable()
    {
        //Sets the sfx and music audio sources of the view.
        if (!audioSourcesAlreadySetOnFrame)
        {
            AudioManager.SetSFXSource(sfxSource);
            audioSourcesAlreadySetOnFrame = true;
        }
    }

    //Resets the boolean that indicates whether a popup has been closed on this frame for this view to false.
    public virtual void ResetPopupClosedOnFrame()
    {
        popupClosedOnFrameVar = false;
    }
}
