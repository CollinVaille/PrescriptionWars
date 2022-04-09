using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioWordPronounciation : MonoBehaviour
{
    public static List<RadioClip> PronounceWords(string words)
    {
        //Make it all upper case first of all
        words = words.ToUpper();

        List<RadioClip> radioClips = new List<RadioClip>();

        //Convert from string to list of strings where each string is a word
        char[] wordSeparators = new char[] { ' ', '-', ',', '.', '_' };
        string[] wordList = words.Split(wordSeparators, System.StringSplitOptions.RemoveEmptyEntries);

        //Pronounce word by word
        foreach(string word in wordList)
            RadioClip.Combine(radioClips, PronounceWord(word));

        //By default, all clips produced in this class will have a pause of zero. But, we don't want any pausing after the last clip because that's the end of the sentence
        if (radioClips.Count > 0)
            radioClips[radioClips.Count - 1].pauseDurationAfter = RadioClip.StandardPauseLength();

        return radioClips;
    }

    private static List<RadioClip> PronounceWord(string word)
    {
        //If it's a predefined word, just grab it
        RadioClip predefinedWordClip = LoadPredefinedWordClip(word);
        if (predefinedWordClip != null)
            return new List<RadioClip>() { predefinedWordClip } ;

        //If it's a number pronounce that
        if (float.TryParse(word, out float number))
            return RadioNumericalPronounciation.PronounceNumber(number);

        //Sound out the syllables with massive margin of error
        List<RadioClip> radioClips = SoundItOut(word);

        //By default, all clips produced in this class will have a pause of zero. But, we don't want any pausing after the last clip because that's the end of the word
        if (radioClips.Count > 0)
            radioClips[radioClips.Count - 1].pauseDurationAfter = Random.Range(0.3f, 0.5f);

        return radioClips;
    }

    private static List<RadioClip> SoundItOut(string word)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        string nextPhoeneticSyllable;
        for (int letterIndex = 0; letterIndex < word.Length;)
        {
            nextPhoeneticSyllable = null;

            //Look at the next three letters, or however many is left, and get the next phoenetic syllable
            if (letterIndex + 3 < word.Length)
                nextPhoeneticSyllable = GetPhoeneticSyllable(word.Substring(letterIndex, 4));

            //Got a match at three letters
            if (nextPhoeneticSyllable != null)
            {
                radioClips.Add(LoadPhoeneticSyllableClip(nextPhoeneticSyllable));
                letterIndex += 4;
                continue;
            }

            //Look at the next three letters, or however many is left, and get the next phoenetic syllable
            if (letterIndex + 2 < word.Length)
                nextPhoeneticSyllable = GetPhoeneticSyllable(word.Substring(letterIndex, 3));

            //Got a match at three letters
            if (nextPhoeneticSyllable != null)
            {
                radioClips.Add(LoadPhoeneticSyllableClip(nextPhoeneticSyllable));
                letterIndex += 3;
                continue;
            }

            if (letterIndex + 1 < word.Length)
                nextPhoeneticSyllable = GetPhoeneticSyllable(word.Substring(letterIndex, 2));

            //Got a match at two letters
            if (nextPhoeneticSyllable != null)
            {
                radioClips.Add(LoadPhoeneticSyllableClip(nextPhoeneticSyllable));
                letterIndex += 2;
                continue;
            }

            nextPhoeneticSyllable = GetPhoeneticSyllable(word.Substring(letterIndex, 1));

            //Got a match at one letter
            if (nextPhoeneticSyllable != null)
            {
                radioClips.Add(LoadPhoeneticSyllableClip(nextPhoeneticSyllable));
                letterIndex++;
                continue;
            }

            //No matches, just move to next letter and don't even try to pronounce it
            letterIndex++;
        }

        return radioClips;
    }

    private static string GetPhoeneticSyllable(string syllable)
    {
        switch(syllable)
        {
            case "A":
            case "AI":
            case "AY":
                return "A";
            case "AH":
            case "AW":
                return "AHH";
            case "ALK":
            case "AWK":
                return "ALK";
            case "ANG":
            case "AIN":
                return "ANG";
            case "ANK":
                return "ANK";
            case "B":
            case "BUH":
                return "BUH";
            case "C":
            case "K":
            case "CK":
            case "CUH":
            case "KUH":
                return "KUH";
            case "CR":
            case "CUR":
            case "COR":
            case "CORR":
            case "KER":
                return "KUR";
            case "D":
            case "DUH":
                return "DUH";
            case "EA":
            case "EE":
            case "EI":
                return "EEE";
            case "E":
            case "EH":
                return "EH";
            case "EO":
            case "IO":
                return "EO";
            case "EYE":
            case "IE":
            case "EY":
                return "EYE";
            case "F":
            case "FUH":
                return "FUH";
            case "G":
            case "GU":
                return "GUH";
            case "H":
            case "HOO":
            case "HU":
                return "HOO";
            case "I":
                return "IH";
            case "IN":
            case "EN":
            case "INN":
                return "IN";
            case "INE":
            case "EIN":
            case "IGN":
                return "INE";
            case "ING":
            case "ENG":
                return "ING";
            case "ION":
            case "TION":
                return "ION";
            case "IT":
            case "ET":
                return "IT";
            case "J":
            case "JU":
                return "JUH";
            case "L":
            case "LA":
                return "LAH";
            case "M":
            case "MM":
                return "MMM";
            case "N":
            case "NU":
                return "NUH";
            case "O":
            case "OH":
                return "OH";
            case "OO":
            case "OU":
                return "OOO";
            case "ON":
            case "AWN":
                return "ON";
            case "OR":
            case "ORE":
                return "OR";
            case "OW":
            case "AO":
                return "OW";
            case "P":
            case "PU":
                return "PUH";
            case "Q":
            case "QU":
                return "QUH";
            case "R":
            case "RR":
            case "AR":
            case "ER":
            case "UR":
            case "URR":
                return "RRR";
            case "RE":
            case "REE":
                return "REE";
            case "S":
            case "SS":
                return "SSS";
            case "STR":
            case "STIR":
                return "STR";
            case "T":
            case "TA":
            case "TU":
                return "TUH";
            case "TR":
            case "TER":
            case "TUR":
                return "TUR";
            case "U":
            case "UH":
                return "UH";
            case "V":
                return "VVV";
            case "W":
            case "WH":
            case "WU":
            case "WHA":
                return "WUH";
            case "X":
                return "X";
            case "Y":
            case "YE":
            case "YEH":
                return "YEH";
            case "Z":
            case "ZU":
                return "ZUH";
            default:
                return null;
        }
    }

    private static RadioClip LoadPhoeneticSyllableClip(string syllable) { return new RadioClip("Planet/Radio/Syllables/" + syllable, false, 0.0f); }

    private static RadioClip LoadPredefinedWordClip(string word)
    {
        AudioClip audioClip = Resources.Load<AudioClip>("Planet/Radio/Words/" + word);

        if (!audioClip)
            return null;
        else
            return new RadioClip(audioClip, Random.Range(0.3f, 0.5f));
    }
}
