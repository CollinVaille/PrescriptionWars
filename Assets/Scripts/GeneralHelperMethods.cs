using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GeneralHelperMethods
{
    //TEXT FILE MANAGEMENT--------------------------------------------------------------------------

    private static Dictionary<string, string[]> textFileCache;

    //Returns one line randomly picked from text file- WITHOUT CARRIAGE RETURN!
    public static string GetLineFromFile (string resourcePath, bool cacheLines = true)
    {
        //Returns possible lines WITHOUT carriage return
        string[] possibleLines = GetLinesFromFile(resourcePath, cacheLines);

        //Choose a line and return it
        return possibleLines[Random.Range(0, possibleLines.Length)];
    }

    //Returns all lines from text file- WITHOUT ANY CARRIAGE RETURNS!
    public static string[] GetLinesFromFile (string resourcePath, bool cacheLines = true)
    {
        //First, see if lines are already cached
        string[] lines = GetLinesFromCache(resourcePath);
        if (lines != null)
            return lines;

        //Cache miss, so retrieve from file...

        //Define delimiter... Enter key on Windows = "\r\n" (carriage return + line feed)
        //Since carriage return is part of delimiter, it is not included in parsed strings
        string[] delimiters = new string[] { "\r\n" };

        //Read in lines from text file
        TextAsset textFile = Resources.Load<TextAsset>("General/Text/" + resourcePath);
        lines = textFile.text.Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries);

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