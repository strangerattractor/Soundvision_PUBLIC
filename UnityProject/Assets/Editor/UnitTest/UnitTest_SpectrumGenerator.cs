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
        private Rect noSelectionRect_;
        private float[] dummyData_;
        private Color standardColor_;
        private Color selectedColor_;

        [SetUp]
        public void SetUp()
        {
            dummyData_ = new float[100];
            pdArray_ = Substitute.For<IPdArray>();
            pdArray_.Data.Returns(dummyData_);
            selectionRect_ = new Rect {x = 9, y = 9, width = 3, height = 3};
            noSelectionRect_ = new Rect {width = 0, height = 0};

            standardColor_ = new Color(0f, 0f, 0f, 0.2f);
            selectedColor_ = new Color(0f, 0f, 0f, 1f);
        }
/*
        [Test]
        public void Construction()
        {
            var spectrumGenerator = new SpectrumGenerator(100, 101);

            Assert.AreEqual(100, spectrumGenerator.Spectrum.width);
            Assert.AreEqual(101, spectrumGenerator.Spectrum.height);
        }

        [Test]
        public void Update_array_available_loud()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 100f; // loud sound

            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixel = spectrumGenerator.Update(pdArray_, selectionRect_);

            Assert.AreEqual(1, validPixel);
        }

        [Test]
        public void Update_array_available_soft()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 0.001f; // soft sound

            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixel = spectrumGenerator.Update(pdArray_, selectionRect_);

            Assert.AreEqual(0, validPixel);
        }

        [Test]
        public void Update_array_available_loud_no_selection()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 100f; // loud sound

            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixel = spectrumGenerator.Update(pdArray_, noSelectionRect_);

            Assert.AreEqual(0, validPixel);
        }


        [Test]
        public void Update_array_unavailable()
        {
            var spectrumGenerator = new SpectrumGenerator(100, 100);
            var validPixels = spectrumGenerator.Update(null, noSelectionRect_);

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
                    if (x == 10 && y == 90) // because vertically inverted
                        Assert.AreEqual(selectedColor_, texture.GetPixel(x, y));
                    else
                        Assert.AreEqual(standardColor_, texture.GetPixel(x, y));
                }
            }
        }
            */

    }
}