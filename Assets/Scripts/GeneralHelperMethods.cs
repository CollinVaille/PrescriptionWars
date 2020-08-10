using System.Collections;
using System.Collections.Generic;
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
        TextAsset textFile = Resources.Load<TextAsset>("Text/" + resourcePath);
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
}
