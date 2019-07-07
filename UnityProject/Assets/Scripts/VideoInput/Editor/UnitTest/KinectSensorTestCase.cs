using NUnit.Framework;
using NSubstitute;

namespace VideoInput.Editor.UnitTest
{
    [TestFixture]
    public class UnitTestKinectSensor
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

        [Test]
        public void Update()
        {
            var kinectSensor = new KinectSensor(infraredCameraMock_);
            kinectSensor.Update();

            infraredCameraMock_.Received(1).Update();
        }
    }
}