using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle toggle;
    private bool fullScreenState;

    public void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        volumeSlider.value = PlayerPrefs.GetFloat("music", 0.5f);
        if (PlayerPrefs.GetInt("fullscreen", 0) != 0)
        {
            toggle.enabled = true;
        }
        else
        {
            toggle.enabled = false;
        }
    }
    public void GameVolume()
    {
        myMixer.SetFloat("music", Mathf.Log10(volumeSlider.value) * 20);
        PlayerPrefs.SetFloat("music", volumeSlider.value);
    }

    public void ToggleFullscreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
        if (Screen.fullScreen)
        {
            PlayerPrefs.SetInt("fullscreen", 1);
        }
        else
        {
            PlayerPrefs.SetInt("fullscreen", 0);
        }
        Debug.Log(Screen.fullScreen);
    }
}
