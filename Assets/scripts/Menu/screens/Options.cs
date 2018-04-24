using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour, MenuScreen {

    private Slider _slMaster, _slSoundFX;
    private Text _tMasterAmt, _tSoundFXAmt;

	public void OnLoad() {
        _slMaster = transform.Find("slider_master").GetComponent<Slider>();
        _slSoundFX = transform.Find("slider_soundFX").GetComponent<Slider>();
        _tMasterAmt = transform.Find("t_masterAmt").GetComponent<Text>();
        _tSoundFXAmt = transform.Find("t_soundFXAmt").GetComponent<Text>();
        Debug.Log("Options done Start()");
    }

    public void OnShowScreen() {
        UserData data = UserData.GetData();
        if (_slMaster == null)
            Debug.Log("Slider is null??");
        _slMaster.value = data.masterVolume;
        _tMasterAmt.text = data.masterVolume + "";

        _slSoundFX.value = data.soundFXVolume;
        _tSoundFXAmt.text = data.soundFXVolume + "";
    }

    public void OnMasterSliderChange() {
        _tMasterAmt.text = _slMaster.value + "";
    }

    public void OnSoundFXSliderChange() {
        _tSoundFXAmt.text = _slSoundFX.value + "";
    }

    public void SaveOptions() {
        UserData data = UserData.GetData();
        data.masterVolume = (int)_slMaster.value;
        data.soundFXVolume = (int)_slSoundFX.value;
        UserData.SaveData(data);
    }
}
