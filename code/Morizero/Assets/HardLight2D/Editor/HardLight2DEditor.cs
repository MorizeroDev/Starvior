using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

public class HardLight2DEditor : MonoBehaviour
{

    [MenuItem ("HardLight2D/Refresh collider references")]
    static void RefreshAll ()
    {
        HardLight2DManager.RefreshAllCollidersReferences ();
    }
}

[CustomPropertyDrawer (typeof (SortingLayerPopup))]
public class SortingLayerDrawer : PropertyDrawer
{
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label)
    {
        string[] options = GetSortingLayerNames ();
        int[] picks = new int[options.Length];
        var name = property.stringValue;
        var choice = -1;
        for (int i = 0; i < options.Length; i++)
        {
            picks[i] = i;
            if (name == options[i]) choice = i;
        }
        choice = EditorGUI.IntPopup (position, label.text, choice, options, picks);
        property.stringValue = options[choice];
    }

    public string[] GetSortingLayerNames ()
    {
        Type internalEditorUtilityType = typeof (InternalEditorUtility);
        PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty ("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
        return (string[]) sortingLayersProperty.GetValue (null, new object[0]);
    }
}