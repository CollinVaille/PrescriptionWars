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

abstract public class SpriteNameLoader : MonoBehaviour
{
    [Header("Base Components")]

    [SerializeField] private TextAsset textAsset = null;

    //------------------------
    //Non-inspector variables.
    //------------------------

    /// <summary>
    /// Holds all of the names of the sprites.
    /// </summary>
    protected virtual string[] SpriteNames { get; set; }

    #region Editor
    #if UNITY_EDITOR
    /// <summary>
    /// Indicates whether the names of the sprites have been saved in the text asset.
    /// </summary>
    protected virtual bool SpriteNamesSaved { get; set; }

    /// <summary>
    /// Indicates the path to the folder that the sprites themselves are actually held in from the resources folder.
    /// </summary>
    protected virtual string pathFromResourcesFolder { get; }
    #endif
    #endregion

    private void Awake()
    {
        #region Editor
        #if UNITY_EDITOR
        if (!SpriteNamesSaved)
        {
            string[] spriteNamesFromDirectory = GetSpriteNamesFromDirectory();
            SaveSpriteNamesInTextAsset(spriteNamesFromDirectory);
            SpriteNames = spriteNamesFromDirectory;
        }
        #endif
        #endregion
        if (!Application.isEditor && SpriteNames == null)
            LoadSpriteNames();
    }

    #region Editor
    #if UNITY_EDITOR
    public string[] GetSpriteNamesFromDirectory()
    {
        List<string> spriteNamesFromDirectoryList = new List<string>();
        string myPath = "Assets/Resources/" + pathFromResourcesFolder;
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
                spriteNamesFromDirectoryList.Add(strippedName);
            }
        }
        string[] spriteNamesFromDirectory = new string[spriteNamesFromDirectoryList.Count];
        for (int i = 0; i < spriteNamesFromDirectoryList.Count; i++)
        {
            spriteNamesFromDirectory[i] = spriteNamesFromDirectoryList[i];
        }
        return spriteNamesFromDirectory;
    }

    private void SaveSpriteNamesInTextAsset(string[] spriteNamesToSaveInTextAsset)
    {
        string textToSave = "";
        for (int spriteNameIndex = 0; spriteNameIndex < spriteNamesToSaveInTextAsset.Length; spriteNameIndex++)
        {
            if (spriteNameIndex == 0)
                textToSave = spriteNamesToSaveInTextAsset[spriteNameIndex];
            else
                textToSave += "\n" + spriteNamesToSaveInTextAsset[spriteNameIndex];
        }
        File.WriteAllText(AssetDatabase.GetAssetPath(textAsset), textToSave);
        EditorUtility.SetDirty(textAsset);

        SpriteNamesSaved = true;
    }
    #endif
    #endregion

    private void LoadSpriteNames()
    {
        SpriteNames = textAsset.text.Split('\n');
    }
}

#region Editor
#if UNITY_EDITOR
public class SpriteNameLoaderBuildProcessorBehaviour
{
    //----------------------
    //Overridable variables.
    //----------------------
    protected virtual string textFilePath { get; }
    protected virtual SpriteNameLoader spriteLoader { get; }

    //-------------------------------------
    //IPreprocessBuildWithReport interface.
    //-------------------------------------
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(UnityEditor.Build.Reporting.BuildReport report)
    {
        StreamWriter writer = new StreamWriter(textFilePath, false);
        string[] spriteNames = spriteLoader.GetSpriteNamesFromDirectory();
        for(int spriteNameIndex = 0; spriteNameIndex < spriteNames.Length; spriteNameIndex++)
        {
            if (spriteNameIndex == 0)
                writer.Write(spriteNames[spriteNameIndex]);
            else
                writer.Write("\n" + spriteNames[spriteNameIndex]);
        }
        writer.Close();
    }
}
#endif
#endregion
