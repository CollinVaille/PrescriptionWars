using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEditor;
#region Editor
#if UNITY_EDITOR
using UnityEditor.Build;
#endif
#endregion
using UnityEngine.UIElements;
using UnityEngine.UI;

[System.Serializable]
public class FlagDataLoader : MonoBehaviour
{
    [Header("Flag Data Loader")]

    [SerializeField] private TextAsset flagSymbolsDataTextAsset = null;

    //Non-inspector variables.

    public static string[] flagSymbolNames;

    #region Editor
    #if UNITY_EDITOR
    private static bool flagSymbolNamesSaved = false;
    #endif
    #endregion

    public void OnFlagDataLoaderAwake()
    {
        #region Editor
        #if UNITY_EDITOR
        if (!flagSymbolNamesSaved)
        {
            string[] flagSymbolNamesFromDirectory = GetFlagSymbolNamesFromDirectory();
            SaveFlagSymbolNames(flagSymbolNamesFromDirectory);
            flagSymbolNames = flagSymbolNamesFromDirectory;
        }
        #endif
        #endregion
        if (!Application.isEditor && flagSymbolNames == null)
            LoadFlagSymbolNames();
    }

    #region Editor
    #if UNITY_EDITOR
    public static string[] GetFlagSymbolNamesFromDirectory()
    {
        List<string> flagSymbolNamesFromDirectoryList = new List<string>();
        string myPath = "Assets/Resources/Flag Symbols";
        DirectoryInfo dir = new DirectoryInfo(myPath);
        FileInfo[] info = dir.GetFiles("*.*");
        foreach (FileInfo f in info)
        {
            if (f.Extension == ".png")
            {
                string tempName = f.Name;
                //Debug.Log("tempName = " + tempName);
                string extension = f.Extension;
                //Debug.Log("extention = " + extension);
                string strippedName = tempName.Replace(extension, "");
                //Debug.Log(strippedName + " Is in the Directory");
                flagSymbolNamesFromDirectoryList.Add(strippedName);
            }
        }
        string[] flagSymbolNamesFromDirectory = new string[flagSymbolNamesFromDirectoryList.Count];
        for (int i = 0; i < flagSymbolNamesFromDirectoryList.Count; i++)
        {
            flagSymbolNamesFromDirectory[i] = flagSymbolNamesFromDirectoryList[i];
        }
        return flagSymbolNamesFromDirectory;
    }

    private void SaveFlagSymbolNames(string[] flagSymbolNamesToSave)
    {
        string flagSymbolsTextToSave = "";
        for(int flagSymbolIndex = 0; flagSymbolIndex < flagSymbolNamesToSave.Length; flagSymbolIndex++)
        {
            if (flagSymbolIndex == 0)
                flagSymbolsTextToSave = flagSymbolNamesToSave[flagSymbolIndex];
            else
                flagSymbolsTextToSave += "\n" + flagSymbolNamesToSave[flagSymbolIndex];
        }
        File.WriteAllText(AssetDatabase.GetAssetPath(flagSymbolsDataTextAsset), flagSymbolsTextToSave);
        EditorUtility.SetDirty(flagSymbolsDataTextAsset);

        flagSymbolNamesSaved = true;
    }
    #endif
    #endregion

    private void LoadFlagSymbolNames()
    {
        flagSymbolNames = flagSymbolsDataTextAsset.text.Split('\n');
    }
}

#region Editor
#if UNITY_EDITOR
public class FlagsDataBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        string path = "Assets/Read Only At Runtime Data/FlagSymbolsData.txt";

        //Write some text to the test.txt file
        StreamWriter writer = new StreamWriter(path, false);
        string[] flagSymbolNames = FlagDataLoader.GetFlagSymbolNamesFromDirectory();
        for(int flagSymbolNameIndex = 0; flagSymbolNameIndex < flagSymbolNames.Length; flagSymbolNameIndex++)
        {
            if (flagSymbolNameIndex == 0)
                writer.Write(flagSymbolNames[flagSymbolNameIndex]);
            else
                writer.Write("\n" + flagSymbolNames[flagSymbolNameIndex]);
        }
        writer.Close();
    }
}
#endif
#endregion