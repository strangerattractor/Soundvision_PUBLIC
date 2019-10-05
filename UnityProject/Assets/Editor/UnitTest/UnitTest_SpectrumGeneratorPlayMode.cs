using NUnit.Framework;
using NSubstitute;
using UnityEngine;

namespace cylvester
{
    [TestFixture]
    public class UnitTest_SpectrumGeneratorPlayMode
    {
        private IPdArraySelector pdArraySelector_;
        private Rect selectionRect_;
        private Rect noSelectionRect_;
        private float[] dummyData_;


        [SetUp]
        public void SetUp()
        {
            dummyData_ = new float[100];
            pdArraySelector_ = Substitute.For<IPdArraySelector>();
            pdArraySelector_.SelectedArray.Returns(dummyData_);
            selectionRect_ = new Rect {x = 9, y = 9, width = 3, height = 3};
            noSelectionRect_ = new Rect {width = 0, height = 0};
        }

        [Test]
        public void Construction()
        {
            var spectrumGenerator = new SpectrumGeneratorPlayMode(100, 101, pdArraySelector_);

            Assert.AreEqual(100, spectrumGenerator.Spectrum.width);
            Assert.AreEqual(101, spectrumGenerator.Spectrum.height);
        }

        [Test]
        public void Update_array_available_loud()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 100f; // loud sound

            var spectrumGenerator = new SpectrumGeneratorPlayMode(100, 100, pdArraySelector_);
            var validPixel = spectrumGenerator.Update(selectionRect_);

            Assert.AreEqual(1, validPixel);
        }

        [Test]
        public void Update_array_available_soft()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 0.001f; // soft sound

            var spectrumGenerator = new SpectrumGeneratorPlayMode(100, 100, pdArraySelector_);
            var validPixel = spectrumGenerator.Update(selectionRect_);

            Assert.AreEqual(0, validPixel);
        }

        [Test]
        public void Update_array_available_loud_no_selection()
        {
            for (var i = 0; i < dummyData_.Length; ++i)
                dummyData_[i] = 100f; // loud sound

            var spectrumGenerator = new SpectrumGeneratorPlayMode(100, 100, pdArraySelector_);
            var validPixel = spectrumGenerator.Update(noSelectionRect_);

            Assert.AreEqual(0, validPixel);
        }
    }
}