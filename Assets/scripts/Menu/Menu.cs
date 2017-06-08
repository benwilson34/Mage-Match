using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Menu : MonoBehaviour
{
    public Canvas StartMenuCanvas;
    public Canvas MainMenuCanvas;
    public Canvas MultiplayerMenuCanvas;
    public Canvas OptionsMenuCanvas;
    public Canvas SoundMenuCanvas;

    void Awake()
    {
        MainMenuCanvas.enabled = false;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = false;
        SoundMenuCanvas.enabled = false;
    }

    public void StartOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = true;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = false;
        SoundMenuCanvas.enabled = false;
    }

    public void MulitplayerOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = false;
        MultiplayerMenuCanvas.enabled = true;
        OptionsMenuCanvas.enabled = false;
        SoundMenuCanvas.enabled = false;
    }

    public void OptionsOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = false;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = true;
        SoundMenuCanvas.enabled = false;
    }

    public void SoundOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = false;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = false;
        SoundMenuCanvas.enabled = true;
    }

    public void ReturnOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = true;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = false;
        SoundMenuCanvas.enabled = false;
    }

    public void OptionsReturnOn()
    {
        StartMenuCanvas.enabled = false;
        MainMenuCanvas.enabled = false;
        MultiplayerMenuCanvas.enabled = false;
        OptionsMenuCanvas.enabled = true;
        SoundMenuCanvas.enabled = false;
    }

}
