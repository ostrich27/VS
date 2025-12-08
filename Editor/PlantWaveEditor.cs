using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MobEventData))]
public class MobEventDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        serializedObject.Update();

        SerializedProperty possibleAngles = serializedObject.FindProperty("possibleAngles");
        SerializedProperty spawnRadius = serializedObject.FindProperty("spawnRadius");
        SerializedProperty spawnDistance = serializedObject.FindProperty("spawnDistance");
        SerializedProperty isPlantWave = serializedObject.FindProperty("isPlantWave");
        SerializedProperty plantWaveDuration = serializedObject.FindProperty("plantWaveDuration");

        EditorGUILayout.PropertyField(possibleAngles);
        EditorGUILayout.PropertyField(spawnRadius);
        EditorGUILayout.PropertyField(spawnDistance);

        EditorGUILayout.PropertyField(isPlantWave);

        // only show when ticked
        if (isPlantWave.boolValue)
            EditorGUILayout.PropertyField(plantWaveDuration);

        serializedObject.ApplyModifiedProperties();
    }
}
