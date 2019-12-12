using System;
using UnityEngine;
using UnityEngine.Events;

namespace cylvester
{

    public class MovementBakerRenderTexture : MonoBehaviour
    {
        [SerializeField] private RenderTexture input;
        [SerializeField] private ComputeShader computeShader;
        [SerializeField] private float factor = 1;

        private RenderTexture resultTexture_;
        private RenderTexture previousFrameTexture_;
        private int kernelHandle_;
        private Renderer renderer_;

        private void Start()
        {
            resultTexture_ = new RenderTexture(input.width, input.height, 0, input.graphicsFormat) { enableRandomWrite = true };
            resultTexture_.Create();

            previousFrameTexture_ = new RenderTexture(input.width, input.height, 0, input.graphicsFormat) { enableRandomWrite = true };
            previousFrameTexture_.Create();

            kernelHandle_ = computeShader.FindKernel("CSMain");

            renderer_ = GetComponent<Renderer>();
        }

        private void Update()
        {
            computeShader.SetTexture(kernelHandle_, "Input", input);
            computeShader.SetTexture(kernelHandle_, "Previous", previousFrameTexture_);
            computeShader.SetTexture(kernelHandle_, "Result", resultTexture_);
            computeShader.SetFloat("Factor", factor);

            computeShader.Dispatch(kernelHandle_, 1, 1, 1);

            renderer_.material.SetTexture("_differenceTexture", resultTexture_);

        }
    }

}


