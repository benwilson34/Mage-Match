using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLauncher : MonoBehaviour {

    public Character.Ch testCharacter = Character.Ch.Enfuego;

	void Start () {
        var gameSettings = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        gameSettings.p1name = "Test boi";
        gameSettings.p1char = testCharacter;
        gameSettings.p1loadout = LoadoutData.GetDefaultLoadout(testCharacter).runes;

        gameSettings.p2name = "Training Dummy";
        gameSettings.p2char = Character.Ch.Neutral;
        gameSettings.p2loadout = new string[7] {
            "SampleCharm",
            "SampleCharm",
            "SampleCharm",
            "SampleCharm",
            "SampleCharm",
            "SampleCharm",
            "SampleCharm"
        };

        // TODO other settings
        gameSettings.trainingMode = true;

        SceneManager.LoadScene("Game Screen (Landscape)");
    }

}
