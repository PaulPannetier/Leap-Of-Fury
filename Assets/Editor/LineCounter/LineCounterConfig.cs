#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;

public class LineCounterConfig : ScriptableObject
{
    [Tooltip("The list of the asset's subfolder where count line of codes")]
    public List<string> subfolderToCount;

    [Tooltip("The list of extension script accepted foe counting (like .cs, .js, .c, .py ect) ")]
    public List<string> fileExtensionsAccepted;
}

#endif