using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadioNumericalPronounciation
{
    public static List<RadioClip> PronounceNumber(float number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        //Produce list of radio clips
        if (number < 1000000000)
        {
            radioClips = RadioClip.Combine(radioClips, PronounceWholeNumberPortion((int)number));
            radioClips = RadioClip.Combine(radioClips, PronounceDecimalPortion(number));
        }
        else
            radioClips.Add(LoadUnitNumberClip("Some Giant Ass Number"));

        //By default, all clips produced in this class will have a pause of zero. But, we don't want any pausing after the last clip
        if (radioClips.Count > 0)
            radioClips[radioClips.Count - 1].pauseDurationAfter = RadioClip.StandardPauseLength();

        return radioClips;
    }

    public static List<RadioClip> PronounceNumber(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        //Produce list of radio clips
        if (number < 1000000000)
            radioClips = RadioClip.Combine(radioClips, PronounceWholeNumberPortion((int)number));
        else
            radioClips.Add(LoadUnitNumberClip("Some Giant Ass Number"));

        //By default, all clips produced in this class will have a pause of zero. But, we don't want any pausing after the last clip
        if (radioClips.Count > 0)
            radioClips[radioClips.Count - 1].pauseDurationAfter = RadioClip.StandardPauseLength();

        return radioClips;
    }

    private static List<RadioClip> PronounceWholeNumberPortion(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        if(number < 100)
            radioClips = RadioClip.Combine(radioClips, PronounceTwoWholeDigits(number));
        else if(number < 1000)
            radioClips = RadioClip.Combine(radioClips, PronounceThreeWholeDigits(number));
        else if (number < 1000000)
            radioClips = RadioClip.Combine(radioClips, PronounceSixWholeDigits(number));
        else
            radioClips = RadioClip.Combine(radioClips, PronounceNineWholeDigits(number));

        return radioClips;
    }

    private static List<RadioClip> PronounceNineWholeDigits(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        if (number >= 1000000)
        {
            radioClips.AddRange(PronounceThreeWholeDigits((int)(number / 1000000)));
            radioClips.Add(LoadUnitNumberClip("Million"));

            if (number % 1000000 != 0)
                radioClips = RadioClip.Combine(radioClips, PronounceSixWholeDigits(number % 1000000));
        }
        else
            radioClips = PronounceSixWholeDigits(number);

        return radioClips;
    }

    private static List<RadioClip> PronounceSixWholeDigits(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        if (number >= 1000)
        {
            radioClips.AddRange(PronounceThreeWholeDigits((int)(number / 1000)));
            radioClips.Add(LoadUnitNumberClip("Thousand"));

            if(number % 1000 != 0)
                radioClips = RadioClip.Combine(radioClips, PronounceThreeWholeDigits(number % 1000));
        }
        else
            radioClips = PronounceThreeWholeDigits(number);

        return radioClips;
    }

    private static List<RadioClip> PronounceThreeWholeDigits(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        if (number >= 100)
        {
            radioClips.Add(LoadUnitNumberClip((int)(number / 100)));
            radioClips.Add(LoadUnitNumberClip("Hundred"));

            if(number % 100 != 0)
            {
                radioClips.Add(LoadUnitNumberClip("And"));
                radioClips = RadioClip.Combine(radioClips, PronounceTwoWholeDigits(number % 100));
            }
        }
        else
            radioClips = PronounceTwoWholeDigits(number);

        return radioClips;
    }

    private static List<RadioClip> PronounceTwoWholeDigits(int number)
    {
        List<RadioClip> radioClips = new List<RadioClip>();

        if (number >= 20)
        {
            radioClips.Add(LoadUnitNumberClip(((int)(number / 10)) * 10));
            if (number % 10 != 0)
                radioClips.Add(LoadUnitNumberClip(number % 10));
        }
        else
            radioClips.Add(LoadUnitNumberClip(number));

        return radioClips;
    }



    private static List<RadioClip> PronounceDecimalPortion(float number)
    {
        //Check if nothing after decimal point
        if (number % 1 == 0)
            return null;

        List<RadioClip> radioClips = new List<RadioClip>();
        radioClips.Add(LoadUnitNumberClip("Point"));

        //Example: Converts to 45.32 to "0.32"
        number -= Mathf.Floor(number);
        string decimalString = number.ToString();

        //Just run through the string version of the number and prounce the digits individually
        bool passedPeriod = false;
        for(int x = 0; x < decimalString.Length; x++)
        {
            if (decimalString[x] == '.')
            {
                passedPeriod = true;
                continue;
            }
            
            if (!passedPeriod)
                continue;

            radioClips.Add(LoadUnitNumberClip(decimalString[x] - '0'));
        }

        return radioClips;
    }

    private static RadioClip LoadUnitNumberClip(int unitNumber) { return LoadUnitNumberClip(GetUnitNumberClipName(unitNumber)); }

    private static string GetUnitNumberClipName(int unitNumber)
    {
        switch(unitNumber)
        {
            case 0: return "Zero";
            case 1: return "One";
            case 2: return "Two";
            case 3: return "Three";
            case 4: return "Four";
            case 5: return "Five";
            case 6: return "Six";
            case 7: return "Seven";
            case 8: return "Eight";
            case 9: return "Nine";
            case 10: return "Ten";
            case 11: return "Eleven";
            case 12: return "Twelve";
            case 13: return "Thirteen";
            case 14: return "Fourteen";
            case 15: return "Fifteen";
            case 16: return "Sixteen";
            case 17: return "Seventeen";
            case 18: return "Eighteen";
            case 19: return "Nineteen";
            case 20: return "Twenty";
            case 30: return "Thirty";
            case 40: return "Fourty";
            case 50: return "Fifty";
            case 60: return "Sixty";
            case 70: return "Seventy";
            case 80: return "Eighty";
            case 90: return "Ninety";
            case 100: return "Hundred";
            case 1000: return "Thousand";
            case 1000000: return "Million";
            default: return "";
        }
    }

    //All our number clips are in the same folder so this just standardizes access. Also, we want zero pausing in between clips so number pronounciation is fluid
    private static RadioClip LoadUnitNumberClip(string unitNumberClipName) { return new RadioClip("Planet/Radio/Numbers/" + unitNumberClipName, false, 0.0f); }
}
