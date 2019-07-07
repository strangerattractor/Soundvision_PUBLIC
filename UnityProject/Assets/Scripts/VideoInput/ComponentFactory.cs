namespace VideoInput
{
    public class ComponentFactory
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