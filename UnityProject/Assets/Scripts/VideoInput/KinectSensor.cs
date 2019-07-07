namespace VideoInput
{
   public interface IKinectSensor
    {
        IInfraredCamera InfraredCamera { get; }
        void Update();
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