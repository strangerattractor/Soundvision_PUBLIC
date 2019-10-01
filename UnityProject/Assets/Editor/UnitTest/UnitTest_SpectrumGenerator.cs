using NUnit.Framework;
using NSubstitute;
using UnityEngine;

namespace cylvester
{
    [TestFixture]
    public class UnitTest_SpectrumGenerator
    {
        private IPdArray pdArray_;
        private Rect selectionRect_;
        private float[] dummyData_;
        private Color standardColor_; 
        private Color selectedColor_;
        
        [SetUp]
        public void SetUp()
        {
            pdArray_ = Substitute.For<IPdArray>();
            selectionRect_ = new Rect {x = 9, y = 9, width = 3, height = 3};
            dummyData_ = new float[100];

            for (var i = 0; i < dummyData_.Length; ++i)
            {
                dummyData_[i] = i / (float)dummyData_.Length;
            }
            
            standardColor_ = new Color(0f, 0f, 0f, 0.2f);
            selectedColor_ = new Color(0f, 0f, 0f, 1f);
        }
        
        [Test]
        public void Construction()
        {
            var spectrumGenerator = new SpectrumGenerator(100, 101);
            
            Assert.AreEqual(100, spectrumGenerator.Spectrum.width);
            Assert.AreEqual(101, spectrumGenerator.Spectrum.height);
        }
        
        [Test]
        public void Update_array_available()
        {
            var spectrumGenerator = new SpectrumGenerator(100, 100);
            spectrumGenerator.Update(pdArray_, selectionRect_);
            
            
        }
        
        [Test]
        public void Update_array_unavailable()
        {
            var noSelection = new Rect {width = 0, height = 0};
            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixels = spectrumGenerator.Update(null, noSelection);

            Assert.AreEqual(0, validPixels);
            var texture = spectrumGenerator.Spectrum;
            var pixels = texture.GetPixels();
            
            foreach (var pixel in pixels)
                Assert.AreEqual(standardColor_, pixel);
        }
        
        [Test]
        public void Update_array_unavailable_with_selection()
        {
            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixels = spectrumGenerator.Update(null, selectionRect_);

            Assert.AreEqual(0, validPixels);
            var texture = spectrumGenerator.Spectrum;
            
            for (var x = 0; x < 100; x++)
            {
                for (var y = 0; y < 100; y++)
                {
                    if(x == 10 && y == 90) // because vertically inverted
                        Assert.AreEqual(selectedColor_, texture.GetPixel(x, y));
                    else
                        Assert.AreEqual(standardColor_, texture.GetPixel(x, y));
                    
                }
            }
        }
    }
}