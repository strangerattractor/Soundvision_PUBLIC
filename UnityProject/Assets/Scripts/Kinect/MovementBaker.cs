using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{
    [Serializable]
    class UnityMovementTextureEvent : UnityEvent<Texture2D> { }

    public class MovementBaker : MonoBehaviour
    {
        [SerializeField] private UnityMovementTextureEvent DifferenceTextureReceived;
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private float factor = 1;
        
        private RenderTexture resultTexture_;
        private RenderTexture previousFrameTexture_;
        private Texture2D texture_;
        private int kernelHandle_;

        private void Start()
        {
            resultTexture_ = new RenderTexture(512, 424, 0, RenderTextureFormat.R16) {enableRandomWrite = true};
            resultTexture_.Create();
            
            texture_ = new Texture2D(512, 424, TextureFormat.R16, false);
            
            previousFrameTexture_ = new RenderTexture(512, 424, 0, RenderTextureFormat.R16) {enableRandomWrite = true};
            previousFrameTexture_.Create();
            
            kernelHandle_ = computeShader.FindKernel("CSMain");
        }
        
        public void OnInfraredFrameReceived(Texture2D texture)
        {
            computeShader.SetTexture(kernelHandle_, "Input", texture);
            computeShader.SetTexture(kernelHandle_, "Previous", previousFrameTexture_);
            computeShader.SetTexture(kernelHandle_, "Result", resultTexture_);
            computeShader.SetFloat("Factor", factor);
            
            computeShader.Dispatch(kernelHandle_, 1, 1, 1);

            RenderTexture.active = resultTexture_;
            texture_.ReadPixels(new Rect(0, 0, resultTexture_.width, resultTexture_.height), 0 ,0 );
            texture_.Apply();
            
            DifferenceTextureReceived.Invoke(texture_);
        }
    }

}


