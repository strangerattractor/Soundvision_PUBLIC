using UnityEngine;
using UnityEngine.UI;

public class IdToText : MonoBehaviour
{
    [SerializeField] private Text text;

    public void OnTimbreIdReceived(int id, int distance)
    {
        switch (id)
        {
            case 0:
                text.text = "Snare";
                break;
            case 1:
                text.text = "Hi-hat";
                break;
            case 2:
                text.text = "Kick";
                break;
            default:
                return;
        }
    }
}
