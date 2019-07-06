namespace VideoInput
{
   public interface IKinectSensor
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

    }
}