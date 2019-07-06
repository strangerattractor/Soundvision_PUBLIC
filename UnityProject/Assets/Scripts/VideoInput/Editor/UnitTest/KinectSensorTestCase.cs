using NUnit.Framework;
using NSubstitute;

namespace VideoInput.Editor.UnitTest
{
    [TestFixture]
    public class UnitTest_KinectSensor
    {
        private IInfraredCamera infraredCameraMock_;

        [SetUp]
        public void SetUp()
        {
            infraredCameraMock_ = Substitute.For<IInfraredCamera>();
        }

        [Test]
        public void InfraredCamera()
        {
            var kinectSensor = new KinectSensor(infraredCameraMock_);
            Assert.AreSame(infraredCameraMock_, kinectSensor.InfraredCamera);
        }
    }
}