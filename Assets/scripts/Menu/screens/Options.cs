using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MenuScreen {

    private Slider _slMaster, _slSoundFX, _slMusic;
    private Text _tMasterAmt, _tSoundFXAmt, _tMusicAmt;
    private MenuController _menus;

	public override void OnLoad() {
        _slMaster = transform.Find("slider_master").GetComponent<Slider>();
        _slSoundFX = transform.Find("slider_soundFX").GetComponent<Slider>();
        _slMusic = transform.Find("slider_music").GetComponent<Slider>();
        _tMasterAmt = transform.Find("t_masterAmt").GetComponent<Text>();
        _tSoundFXAmt = transform.Find("t_soundFXAmt").GetComponent<Text>();
        _tMusicAmt = transform.Find("t_musicAmt").GetComponent<Text>();

        _menus = GameObject.Find("world ui").GetComponent<MenuController>();
    }

    public override void OnShowScreen() {
        _slMaster.value = UserData.MasterVolume;
        OnMasterSliderChange();

        _slSoundFX.value = UserData.SFXVolume;
        OnSoundFXSliderChange();

        _slMusic.value = UserData.MusicVolume;
        OnMusicSliderChange();
        
    }

    public void OnMasterSliderChange() {
        _tMasterAmt.text = (int)(_slMaster.value * 100) + "";
    }

    public void OnSoundFXSliderChange() {
        _tSoundFXAmt.text = (int)(_slSoundFX.value * 100) + "";
    }

    public void OnMusicSliderChange() {
        _tMusicAmt.text = (int)(_slMusic.value * 100) + "";
    }

    public void SaveOptions() {
        UserData.MasterVolume = _slMaster.value;
        UserData.SFXVolume = _slSoundFX.value;
        UserData.MusicVolume = _slMusic.value;
        _menus.GoBack();
    }
}
