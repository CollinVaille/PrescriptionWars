using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FlagCreationMenu : MonoBehaviour
{
    public enum EditMode
    {
        Background,
        Symbol
    }
    public static EditMode mode;

    public static int symbolSelected;
    public static int symbolsCount;

    public static Vector3 backgroundColor;
    public static Vector3 symbolColor;

    public List<Sprite> symbols;

    public Image background;
    public Image symbol;

    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    public Text redSliderText;
    public Text greenSliderText;
    public Text blueSliderText;
    public Text modeText;

    public static bool initialized;

    public static void Initialize()
    {
        mode = EditMode.Symbol;

        symbolSelected = 0;

        backgroundColor = new Vector3(1.0f, 1.0f, 1.0f);
        symbolColor = new Vector3(0, 0, 0);

        initialized = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        //Remove after testing
        Initialize();

        //Don't Remove
        symbol.sprite = symbols[symbolSelected];
    }

    // Update is called once per frame
    void Update()
    {
        background.color = new Color(backgroundColor.x, backgroundColor.y, backgroundColor.z, 1);
        symbol.color = new Color(symbolColor.x, symbolColor.y, symbolColor.z, 1);

        NewGameMenu.UpdateEmpireFlag(symbolSelected, backgroundColor, symbolColor);
    }

    public void OnButtonClick(string direction)
    {
        if(direction.Equals("previous"))
        {
            if(mode == EditMode.Symbol)
            {
                if (symbolSelected > 0)
                {
                    symbolSelected--;
                }
                else
                {
                    symbolSelected = symbols.Count - 1;
                }
            }
            else if (mode == EditMode.Background)
            {

            }
        }
        else if(direction.Equals("next"))
        {
            if(mode == EditMode.Symbol)
            {
                if (symbolSelected < symbols.Count - 1)
                {
                    symbolSelected++;
                }
                else
                {
                    symbolSelected = 0;
                }
            }
            else if (mode == EditMode.Background)
            {

            }
        }

        if (mode == EditMode.Symbol)
        {
            symbol.sprite = symbols[symbolSelected];
        }
        else if (mode == EditMode.Background)
        {

        }
    }

    public void ChangeSliderValue(string color)
    {
        if(mode == EditMode.Symbol)
        {
            if (color.Equals("red"))
            {
                symbolColor.x = redSlider.value / 255.0f;
                redSliderText.text = "" + redSlider.value;
            }
            else if (color.Equals("green"))
            {
                symbolColor.y = greenSlider.value / 255.0f;
                greenSliderText.text = "" + greenSlider.value;
            }
            else if (color.Equals("blue"))
            {
                symbolColor.z = blueSlider.value / 255.0f;
                blueSliderText.text = "" + blueSlider.value;
            }
        }
        else if (mode == EditMode.Background)
        {
            if (color.Equals("red"))
            {
                backgroundColor.x = redSlider.value / 255.0f;
                redSliderText.text = "" + redSlider.value;
            }
            else if (color.Equals("green"))
            {
                backgroundColor.y = greenSlider.value / 255.0f;
                greenSliderText.text = "" + greenSlider.value;
            }
            else if (color.Equals("blue"))
            {
                backgroundColor.z = blueSlider.value / 255.0f;
                blueSliderText.text = "" + blueSlider.value;
            }
        }
    }

    public void ChangeMode()
    {
        if (mode == EditMode.Symbol)
        {
            mode = EditMode.Background;
            modeText.text = "Background Mode";
        }
        else if (mode == EditMode.Background)
        {
            mode = EditMode.Symbol;
            modeText.text = "Symbol Mode";
        }
        UpdateSliders();
    }

    void UpdateSliders()
    {
        if (mode == EditMode.Symbol)
        {
            redSlider.value = Mathf.RoundToInt(symbolColor.x * 255.0f);
            greenSlider.value = Mathf.RoundToInt(symbolColor.y * 255.0f);
            blueSlider.value = Mathf.RoundToInt(symbolColor.z * 255.0f);
        }
        else if (mode == EditMode.Background)
        {
            redSlider.value = Mathf.RoundToInt(backgroundColor.x * 255.0f);
            greenSlider.value = Mathf.RoundToInt(backgroundColor.y * 255.0f);
            blueSlider.value = Mathf.RoundToInt(backgroundColor.z * 255.0f);
        }

        redSliderText.text = "" + redSlider.value;
        greenSliderText.text = "" + greenSlider.value;
        blueSliderText.text = "" + blueSlider.value;
    }
}

public class Flag
{
    public int symbolSelected;

    public Vector3 backgroundColor;
    public Vector3 symbolColor;
}
