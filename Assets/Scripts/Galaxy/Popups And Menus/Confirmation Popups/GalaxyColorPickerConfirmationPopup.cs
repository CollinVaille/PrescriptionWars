using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GalaxyColorPickerConfirmationPopup : GalaxyConfirmationPopupBehaviour
{
    [Header("Color Picker Confirmation Popup Components")]

    [SerializeField] private Text bodyText = null;

    [SerializeField] private FlexibleColorPicker colorPicker = null;

    /// <summary>
    /// Returns the color that the player selected when using the color picker.
    /// </summary>
    public Color ColorSelected { get => colorPicker.color; }

    public static GameObject galaxyColorPickerConfirmationPopupPrefab = null;

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

    //This method should be called in order to properly create the confirmation popup and set the color picker's initial color value.
    public void CreateConfirmationPopup(string topText, string bodyText, Color colorPreselected)
    {
        CreateConfirmationPopup(topText);
        this.bodyText.text = bodyText;
        colorPicker.color = colorPreselected;
    }
}
