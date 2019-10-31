namespace cylvester
{
    
   public interface IKinectSensor : IUpdater
    {
        IInfraredCamera InfraredCamera { get; }
    }
    
    public class KinectSensor : IKinectSensor
    {
        public IInfraredCamera InfraredCamera { get; }

        public KinectSensor(IInfraredCamera infraredCamera)
        {
            InfraredCamera = infraredCamera;
        }

        public void Update()
        {
            InfraredCamera.Update();
        }
    }
}