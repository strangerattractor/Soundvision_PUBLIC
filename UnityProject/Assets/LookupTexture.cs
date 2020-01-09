using System;
using System.Timers;
using UnityEngine;

public class LookupTexture : MonoBehaviour
{
    [SerializeField] private int length_ = 100;
    public Texture2D texture_;

    private float input_;
    private int index_;

    private void Start()
    {
        texture_ = new Texture2D(1, length_, TextureFormat.R8, false);

        var pixels = texture_.GetPixels();
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;
        texture_.SetPixels(pixels);
        texture_.Apply();
    }


    public void OnValueChanged (float value)
    {
            input_ = value/100;
    }

    private void Update()
    {
        for (int i = 0; i < length_; i++)
        {
            texture_.SetPixel(i, index_, new Color(input_, 0f, 0f));
        }

        texture_.Apply();

        index_++;
        index_ %= length_;
    }

    public Texture2D Texture => texture_;
    public int Index => index_;
    public int Length => length_;
}
