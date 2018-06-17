using UnityEngine;
using UnityEditor;
using System;

[CustomEditor(typeof(TestLauncher))]
public class MyScriptEditor : Editor {

    override public void OnInspectorGUI() {
        //EditorGUI.BeginChangeCheck();
        var launcher = target as TestLauncher;
        Undo.RecordObject(launcher, "new settings");


        launcher.testCharacter = (Character.Ch)EditorGUILayout.EnumPopup("Test Character", launcher.testCharacter);

        launcher.trainingMode = (DebugSettings.TrainingMode)EditorGUILayout.EnumPopup("Training Mode", launcher.trainingMode);

        bool showSecondChar = launcher.trainingMode == DebugSettings.TrainingMode.OneCharacter;

        using (var group = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(showSecondChar))) {
            if (group.visible == false) {
                EditorGUI.indentLevel++;
                launcher.secondTestCharacter = (Character.Ch)EditorGUILayout.EnumPopup("Second character", launcher.secondTestCharacter);
                EditorGUI.indentLevel--;
            }
        }


        //if (EditorGUI.EndChangeCheck()) {
        //    // Code to execute if GUI.changed
        //    // was set to true inside the block of code above.
        //    Debug.LogError("Changed.");
        //}
    }
}
