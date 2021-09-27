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

[System.Serializable]
public class FlagDataLoader : SpriteNameLoader
{
    //------------------------
    //Non-inspector variables.
    //------------------------

    public static string[] flagSymbolNames { get; private set; }
    protected override string[] SpriteNames { get => flagSymbolNames; set => flagSymbolNames = value; }

    #region Editor
    #if UNITY_EDITOR
    private static bool flagSymbolNamesSaved = false;
    protected override bool SpriteNamesSaved { get => flagSymbolNamesSaved; set => flagSymbolNamesSaved = value; }
    protected override string pathFromResourcesFolder => "General/Flag Symbols";
    #endif
    #endregion
}

#region Editor
#if UNITY_EDITOR
public class FlagsDataBuildProcessor : SpriteNameLoaderBuildProcessor, IPreprocessBuildWithReport
{
    protected override string textFilePath => "Assets/Read Only At Runtime Data/FlagSymbolsData.txt";
    protected override SpriteNameLoader spriteLoader => new FlagDataLoader();
}
#endif
#endregion