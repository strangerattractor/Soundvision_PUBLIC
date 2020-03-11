using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using System.Collections.Generic;

public class SecondScreen : MonoBehaviour
{
    Resolution[] resolutions;

    public Dropdown resolultionDropdown;

    void Start()
    {
        Debug.Log("displays connected: " + Display.displays.Length);

        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
        }

        resolutions = Screen.resolutions;

        resolultionDropdown.ClearOptions();

        List <string> options = new List <string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolultionDropdown.AddOptions(options);
        resolultionDropdown.value = currentResolutionIndex;
        resolultionDropdown.RefreshShownValue();
    }

    public void SetResolution (int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, true);
    }
}
