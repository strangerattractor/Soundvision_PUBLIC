namespace VideoInput
{
    public interface IComponentFactory
    {
        IKinectSensor CreateKinectSensor();
        IInfraredCamera CreateInfraredCamera();
    }
    
    public class ComponentFactory : IComponentFactory
    {
        public IKinectSensor CreateKinectSensor()
        {
            return new KinectSensor(CreateInfraredCamera());
        }

        public IInfraredCamera CreateInfraredCamera()
        {
            return new InfraredCamera();
        }
    }
}