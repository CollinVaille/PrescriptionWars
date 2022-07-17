using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{
    [Header("Components")]

    [SerializeField] private Text displayText = null;

    [Header("Options")]

    [SerializeField] private float refreshRate = 0;

    private float timer = 0;

    public int fps { get; private set; }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Time.unscaledTime >= timer)
        {
            fps = (int)(1f / Time.unscaledDeltaTime);
            if (displayText != null)
                displayText.text = "FPS: " + fps;
            timer = Time.unscaledTime + refreshRate;
        }
    }
}
