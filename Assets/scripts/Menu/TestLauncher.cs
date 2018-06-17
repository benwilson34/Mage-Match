using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLauncher : MonoBehaviour {

    //[SerializeField]
    public Character.Ch testCharacter = Character.Ch.Enfuego;
    //[SerializeField]
    public DebugSettings.TrainingMode trainingMode = DebugSettings.TrainingMode.OneCharacter;
    //[SerializeField]
    public Character.Ch secondTestCharacter = Character.Ch.Gravekeeper;

	void Start () {
        var gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        gameSettings.p1name = "Test boi";
        gameSettings.p1char = testCharacter;
        gameSettings.p1loadout = LoadoutData.GetDefaultLoadout(testCharacter).runes;

        bool oneCharacter = trainingMode == DebugSettings.TrainingMode.OneCharacter;
        gameSettings.p2name = oneCharacter ? "Training Dummy" : "Other test boi";
        gameSettings.p2char = oneCharacter ? Character.Ch.Neutral : secondTestCharacter;
        gameSettings.p2loadout = LoadoutData.GetDefaultLoadout(gameSettings.p2char).runes;

        // TODO other settings
        gameSettings.trainingMode = true;

        var debugSettings = GameObject.Find("DebugSettings").GetComponent<DebugSettings>();
        debugSettings.trainingMode = trainingMode;

        SceneManager.LoadScene("Game Screen (Landscape)");
    }

}
