using NUnit.Framework;

namespace VideoInput.Editor.UnitTest
{
    public class UnitTest_ComponentFactory
    {
        [Test]
        public void CreateKinectSensor()
        {
            var componentFactory =  new ComponentFactory();
            var kinectSensor = componentFactory.CreateKinectSensor();
            Assert.NotNull(kinectSensor);
        }
        
        [Test]
        public void CreateInfraredCamera()
        {
            var componentFactory =  new ComponentFactory();
            var infraredCamera = componentFactory.CreateInfraredCamera();
            Assert.NotNull(infraredCamera);
        }
    }
}

