using cylvester;
using UnityEngine;

public class TextToPd : MonoBehaviour
{
    [SerializeField] private PdBackend pdBackend;
    
    void Start()
    {
        pdBackend.Message("Hello world\n");
    }
}
