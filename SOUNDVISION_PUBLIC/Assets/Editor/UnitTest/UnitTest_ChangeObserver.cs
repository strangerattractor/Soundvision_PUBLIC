using NUnit.Framework;

namespace cylvester
{
    public class UnitTest_ChangeObserver
    {
        [Test]
        public void Set_Get()
        {
            var called = false;
            var observer = new ChangeObserver<float>(1.0f);
            
            observer.ValueChanged += ()=> { called = true; };
            observer.Value = 1.0001f;

            Assert.IsTrue(called);
        }
        
        [Test]
        public void ValueChanged()
        {
            var callCount = 0;
            var observer = new ChangeObserver<float>(1.0f);
            
            observer.ValueChanged += () => { callCount++; };
            observer.Value = 1.0001f;
            observer.Value = 1.0001f;

            Assert.AreEqual(1, callCount);
        }

    }
}