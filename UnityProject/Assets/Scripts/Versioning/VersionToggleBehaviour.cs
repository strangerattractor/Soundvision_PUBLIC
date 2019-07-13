using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class VersionToggleBehaviour : MonoBehaviour
{
    #pragma warning disable 0649
    [SerializeField] private Canvas versionCanvas;
    [SerializeField] private Text versionText;
    [SerializeField] private TextAsset buildNumberFile;
    #pragma warning restore 0649

    void Start()
    {
        versionText.text = "Build: " + buildNumberFile.text;
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            versionCanvas.enabled = !versionCanvas.enabled;
        }
    }
}
