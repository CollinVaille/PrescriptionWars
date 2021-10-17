using UnityEngine;

class SunType
{
    public string flareName;
    public Color sunlightColor;
    public float intensity; //Measure of coldness (1 = normal temp, 0.5 = frigid, 1.5 = blazing)

    //Below are a bunch of helper functions...

    //Converts 0-255 (human readable) to 0-1 (unity format)
    public static Color GetColorRGB(int r, int g, int b)
    {
        return new Color(r / 255.0f, g / 255.0f, b / 255.0f);
    }

    //Converts 0-360/0-100/0-100 (human readable) to 0-1 (unity format)
    public static Color GetColorHSV(int h, int s, int v)
    {
        return Color.HSVToRGB(h / 360.0f, s / 100.0f, v / 100.0f);
    }

    //NEW SUN SYSTEM------------------------------------------------------------

    public SunType(float intensity) { CreateSunType(intensity, Color.white); }

    public SunType(float intensity, Color sunColor) { CreateSunType(intensity, sunColor); }

    private void CreateSunType(float intensity, Color sunColor)
    {
        //Remember intensity
        this.intensity = intensity;

        //Determine sunlight color
        sunlightColor = Color.white;
        if (intensity > 1)
        {
            sunlightColor.b = 2 - intensity;
            sunlightColor.g = 2 - intensity;
        }
        else if (intensity < 1)
        {
            sunlightColor.r = intensity;
            sunlightColor.g = Random.Range(intensity, 1.0f);
        }

        //Warm temperate suns most likely have full green value, meaning yellowish hue
        if (intensity > 1.0f && intensity < 1.15f && Random.Range(0, 4) != 0)
            sunlightColor.g = 1;

        //Determine flare name
        DetermineFlareName();
    }

    private void DetermineFlareName()
    {
        bool makeAnotherSelection = true;
        for (int attempt = 1; makeAnotherSelection && attempt <= 500; attempt++)
        {
            //Assume we don't need to pick another color afterwards until proven otherwise
            makeAnotherSelection = false;

            int picker = Random.Range(0, 16);

            switch (picker)
            {
                case 0:
                    flareName = "6 Blade Aperture";
                    break;
                case 1:
                    flareName = "35mm Lens";
                    //sunlightColor = GetColorRGB(203, 237, 255);

                    //Must be some shade of blue
                    if (sunlightColor.b < 1 || sunlightColor.g < sunlightColor.r || intensity > 0.85f)
                        makeAnotherSelection = true;

                    break;
                case 2:
                    flareName = "85mm Lens";
                    break;
                case 3:
                    flareName = "Cheap Plastic Lens";
                    //sunlightColor = GetColorRGB(255, 255, 255);

                    //Must be white
                    if (sunlightColor.r < 0.85f || sunlightColor.b < 0.85f || sunlightColor.g < 0.85f)
                        makeAnotherSelection = true;

                    break;
                case 4:
                    flareName = "Cold Clear Sun";
                    //sunlightColor = GetColorRGB(255, 255, 255);

                    //Must be white
                    if (sunlightColor.r < 0.85f || sunlightColor.b < 0.85f || sunlightColor.g < 0.85f)
                        makeAnotherSelection = true;

                    break;
                case 5:
                    flareName = "Concert";
                    break;
                case 6:
                    flareName = "Digicam Lens";
                    //sunlightColor = GetColorRGB(255, 252, 223);

                    //Must be white
                    if (sunlightColor.r < 0.85f || sunlightColor.b < 0.85f || sunlightColor.g < 0.85f)
                        makeAnotherSelection = true;

                    break;
                case 7:
                    flareName = "Digital Camera";
                    break;
                case 8:
                    flareName = "Halogen Bulb";
                    //sunlightColor = GetColorRGB(105, 184, 255);

                    //Must be deep blue
                    if (sunlightColor.b < 1 || sunlightColor.g < sunlightColor.r || intensity > 0.5f)
                        makeAnotherSelection = true;

                    break;
                case 9:
                    flareName = "Laser";
                    break;
                case 10:
                    flareName = "Subtle1";
                    break;
                case 11:
                    flareName = "Subtle2";
                    break;
                case 12:
                    flareName = "Subtle3";
                    break;
                case 13:
                    flareName = "Subtle4";
                    break;
                case 14:
                    flareName = "Sun (from space)";
                    //sunlightColor = GetColorRGB(255, 255, 255);

                    //Must be white
                    if (sunlightColor.r < 0.85f || sunlightColor.b < 0.85f || sunlightColor.g < 0.85f)
                        makeAnotherSelection = true;

                    break;
                default:
                    flareName = "Welding";
                    //sunlightColor = GetColorRGB(114, 218, 255);

                    //Must be deep blue
                    if (sunlightColor.b < 1 || sunlightColor.g < sunlightColor.r || intensity > 0.5f)
                        makeAnotherSelection = true;

                    break;
            }
        }
    }
}