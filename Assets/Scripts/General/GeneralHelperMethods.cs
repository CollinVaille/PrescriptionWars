using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GeneralHelperMethods
{
    //TEXT FILE MANAGEMENT--------------------------------------------------------------------------

    private static Dictionary<string, string[]> textFileCache;

    public static TextAsset GetTextAsset(string resourcePath, bool startPathFromGeneralTextFolder = true)
    {
        if (startPathFromGeneralTextFolder)
            return Resources.Load<TextAsset>("General/Text/" + resourcePath);
        else
            return Resources.Load<TextAsset>(resourcePath);
    }

    //Returns one line randomly picked from text file- WITHOUT CARRIAGE RETURN!
    public static string GetLineFromFile (string resourcePath, bool cacheLines = true, bool startPathFromGeneralTextFolder = true, bool nullSafe = false)
    {
        //Returns possible lines WITHOUT carriage return
        string[] possibleLines = GetLinesFromFile(resourcePath, cacheLines: cacheLines, startPathFromGeneralTextFolder: startPathFromGeneralTextFolder, nullSafe: nullSafe);

        //Null check
        if (nullSafe && possibleLines == null)
            return null;

        //Choose a line and return it
        return possibleLines[Random.Range(0, possibleLines.Length)];
    }

    //Returns all lines from text file- WITHOUT ANY CARRIAGE RETURNS!
    public static string[] GetLinesFromFile (string resourcePath, bool cacheLines = true, bool startPathFromGeneralTextFolder = true, bool nullSafe = false)
    {
        //First, see if lines are already cached
        string[] lines = GetLinesFromCache(resourcePath);
        if (lines != null)
            return lines;

        //Cache miss, so retrieve from file...

        //Define delimiter... Enter key on Windows = "\r\n" (carriage return + line feed)
        //Since carriage return is part of delimiter, it is not included in parsed strings
        string[] delimiters = new string[] { "\r\n" };

        //Read in text file
        TextAsset fileContentsAsTextAsset = GetTextAsset(resourcePath, startPathFromGeneralTextFolder: startPathFromGeneralTextFolder);

        //Null check
        if (nullSafe && !fileContentsAsTextAsset)
            return null;

        //Get contents as string
        string fileContentsAsString = fileContentsAsTextAsset.text;

        //Parse string into lines
        lines = fileContentsAsString.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

        //Cache lines
        if (cacheLines)
            textFileCache.Add(resourcePath, lines);

        //Return them
        return lines;
    }

    private static string[] GetLinesFromCache (string resourcePath)
    {
        if (textFileCache == null)
        {
            textFileCache = new Dictionary<string, string[]>();
            return null;
        }
        else if (textFileCache.ContainsKey(resourcePath))
            return textFileCache[resourcePath];
        else
            return null;
    }

    public static void ClearTextFileCache () { textFileCache.Clear(); }

    public static string RemoveCarriageReturn (string text)
    {
        string returnText = "";
        char[] charArray = text.ToCharArray();

        foreach (char c in charArray)
        {
            if ((int)c != 13)
            {
                returnText += c;
            }
        }

        return returnText;
    }

    //OTHER SHIT------------------------------------------------------------------------------------

    //Spaces out enum text
    public static string GetEnumText (string text)
    {
        List<char> charList = new List<char>();
        char[] charArray = text.ToCharArray();
        foreach (char c in charArray)
        {
            charList.Add(c);
        }

        for (int x = text.Length - 1; x > -1; x--)
        {
            if (char.IsUpper(charList[x]) && x != 0)
            {
                charList.Insert(x, ' ');
            }
        }

        string finalText = "";
        foreach (char c in charList)
        {
            finalText += c;
        }

        return finalText;
    }

    public static string GetStartsWithText(string text)
    {
        string startsWithText = "";

        foreach(char c in text.ToCharArray())
        {
            if (c == ' ')
                break;
            startsWithText += c;
        }

        return startsWithText;
    }

    public static int GetNumberFromText(string text, int startingIndex, int endingIndex)
    {
        string numberTextFromText = "";

        char[] textCharArray = text.ToCharArray();

        for(int characterIndex = startingIndex; characterIndex <= endingIndex; characterIndex++)
        {
            numberTextFromText += char.IsDigit(textCharArray[characterIndex]) ? textCharArray[characterIndex].ToString() : "";
        }

        return int.Parse(numberTextFromText);
    }

    public static float GetRandomValueFromRange(float[] range, float defaultValue = 0.0f)
    {
        if (range == null || range.Length == 0)
            return defaultValue;

        if (range.Length == 1)
            return range[0];

        return Random.Range(range[0], range[1]);
    }

    public static int GetRandomValueFromRange(int[] range, int defaultValue = 0)
    {
        if (range == null || range.Length == 0)
            return defaultValue;

        if (range.Length == 1)
            return range[0];

        return Random.Range(range[0], range[1]);
    }

    public static string GetOneOf(string[] options)
    {
        if (options == null || options.Length == 0)
            return null;

        return options[Random.Range(0, options.Length)];
    }

    public static void TrimToRandomSubset(List<string> toTrim, int subsetSize)
    {
        while (toTrim.Count > subsetSize)
            toTrim.RemoveAt(Random.Range(0, toTrim.Count));
    }

    public static bool GetColorIfSpecified(HumanFriendlyColorJSON colorJSON, out Color color)
    {
        if (colorJSON == null || (colorJSON.r == 0 && colorJSON.g == 0 && colorJSON.b == 0 && Mathf.Approximately(colorJSON.a, 0.0f)))
        {
            color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
            return false;
        }

        color = new Color(colorJSON.r / 255.0f, colorJSON.g / 255.0f, colorJSON.b / 255.0f, colorJSON.a);
        return true;
    }

    //Returns the canvas that the specified transform is under in the hierarchy.
    public static Canvas GetParentCanvas(Transform transform)
    {
        Canvas currentParentCanvas = null;

        Transform nextTransformToCheck = transform.parent;
        while (currentParentCanvas == null)
        {
            if (nextTransformToCheck.GetComponent<Canvas>() != null)
            {
                currentParentCanvas = nextTransformToCheck.GetComponent<Canvas>();
                break;
            }

            if (nextTransformToCheck.parent != null)
                nextTransformToCheck = nextTransformToCheck.parent;
            else
                break;
        }

        return currentParentCanvas;
    }

    /// <summary>
    /// This method should be called in order to reset the texture of the cursor texture.
    /// </summary>
    public static void ResetCursorTexture()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
#region Editor
#if UNITY_EDITOR
public class ReadOnlyAttribute : PropertyAttribute
{
    //Collin please do not delete this.
}

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif
#endregion