using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GalaxyViewBehaviour : MonoBehaviour, IGalaxyTooltipHandler, IGalaxyPopupBehaviourHandler
{
    [Header("Galaxy View Galaxy Tooltip Handler")]

    [SerializeField] private Transform tooltipsParent = null;
    public Transform TooltipsParent
    {
        get
        {
            return tooltipsParent;
        }
    }

    [Header("Galaxy View Galaxy Popup Handler")]

    [SerializeField] private Vector2 popupScreenBounds = new Vector2(291, 99);
    public Vector2 PopupScreenBounds
    {
        get
        {
            return popupScreenBounds;
        }
    }

    [Header("Galaxy View Audio Options")]

    [SerializeField]
    private AudioSource sfxSource = null;
    [SerializeField]
    private AudioSource musicSource = null;

    //Non-inspector variables.

    //Indicates whether a popup has been closed on this frame on this view.
    private bool popupClosedOnFrame = false;
    public bool PopupClosedOnFrame
    {
        get
        {
            return popupClosedOnFrame;
        }
        set
        {
            popupClosedOnFrame = value;
        }
    }

    // Start is called before the first frame update
    public virtual void Start()
    {
        //Sets the sfx and music audio sources of the view.
        AudioManager.SetAudioSources(sfxSource, musicSource);
    }

    public virtual void Awake()
    {
        //Makes sure that the audio settings have been loaded in.
        if (!AudioSettings.Loaded)
            AudioSettings.LoadSettings();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        
    }

    public virtual void OnEnable()
    {
        //Sets the sfx and music audio sources of the view.
        AudioManager.SetAudioSources(sfxSource, musicSource);
    }

    public virtual void OnDisable()
    {
        AudioManager.SaveMusicSourceDetails();
    }

    public virtual void OnDestroy()
    {
        AudioManager.SaveMusicSourceDetails();
    }

    //Resets the boolean that indicates whether a popup has been closed on this frame for this view to false.
    public virtual void ResetPopupClosedOnFrame()
    {
        popupClosedOnFrame = false;
    }
}
