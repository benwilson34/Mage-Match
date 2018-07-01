using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

[CustomEditor(typeof(TestLauncher))]
public class MyScriptEditor : Editor {

    override public void OnInspectorGUI() {
        var launcher = target as TestLauncher;
        Undo.RecordObject(launcher, "new settings");

        string dirPath = Application.persistentDataPath + "/Replays";
        if (!Directory.Exists(dirPath)) {
            Directory.CreateDirectory(dirPath);
            Debug.LogWarning("Created Replays folder at " + dirPath);
        }

        var replays = Directory.GetDirectories(dirPath);
        if (replays.Length == 0) {
            EditorGUILayout.LabelField("You don't have any saved games. You can save a report from a game by playing, then going to the Debug menu in the top-left corner and clicking the \"Save Files\" button.", new GUIStyle() { wordWrap = true });
            launcher.playFromFile = false;
            launcher.replayChoice = 0; //?
        } else {
            launcher.playFromFile = EditorGUILayout.Toggle("Play from file", launcher.playFromFile);
        }

        using (var fileGroup = new EditorGUILayout.FadeGroupScope(Convert.ToSingle(launcher.playFromFile))) {
            if (fileGroup.visible) {
                var replayNames = replays
                    .Select(dir => new DirectoryInfo(dir).Name).ToArray();
                launcher.replayChoice = EditorGUILayout.Popup(launcher.replayChoice, replayNames);
                launcher.replayFile = replays[launcher.replayChoice] + "/MageMatch_" + replayNames[launcher.replayChoice] + "_Report.txt";
                Debug.Log("Going to load "+launcher.replayFile);

                launcher.fastForward = EditorGUILayout.Toggle("Fast forward", launcher.fastForward);
            } else {
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
            }
        }
    }
}
