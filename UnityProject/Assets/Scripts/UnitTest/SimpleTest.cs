using NUnit.Framework;
using NSubstitute;
    
namespace UnitTests
{
    public class SimpleTest
    {
        [Test]
        public void FirstTest()
        {
            Assert.AreEqual(10, 10);
        }

        public interface ISomeInterface
        {
            void doSomething();
        }
        
        [Test]
        public void FirstMock()
        {           
            var mock = Substitute.For<ISomeInterface>();
            mock.doSomething();
            
            mock.Received(1).doSomething();
        }
        
  
    }

}

