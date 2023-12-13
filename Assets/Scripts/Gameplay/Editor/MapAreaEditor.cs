using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapArea))]
public class MapAreaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        int totalChanceInGrass = serializedObject.FindProperty("totalChance").intValue;
        int totalChanceInWater = serializedObject.FindProperty("totalChanceWater").intValue;

        if (totalChanceInGrass != 100 && totalChanceInGrass != -1)
            EditorGUILayout.HelpBox($"The total chance percentage of fighter in grass is {totalChanceInGrass} and not 100", MessageType.Error);

        if (totalChanceInWater != 100 && totalChanceInWater != -1)
            EditorGUILayout.HelpBox($"The total chance percentage of fighter in water is {totalChanceInWater} and not 100", MessageType.Error);
    }
}
