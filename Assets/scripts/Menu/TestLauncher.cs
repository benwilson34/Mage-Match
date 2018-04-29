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
        gameSettings.p2name = "Training Dummy";
        gameSettings.p2char = Character.Ch.Sample;

        // TODO settings

        SceneManager.LoadScene("Game Screen (Landscape)");
    }

}
