using NUnit.Framework;

namespace cylvester
{
    public class UnitTest_ParameterResponder
    {
        [Test]
        public void Set_Get()
        {
            var responder = new Parameter<float>(1.0f);
            responder.Value = 3.2f;

            Assert.AreEqual(3.2f, responder.Value);
        }
        
        [Test]
        public void ValueChanged()
        {
            var responder = new Parameter<float>(1.0f);
            responder.Value = 0f;

            void OnValueChanged()
            {
                Assert.AreEqual(3.2f, responder.Value);
            }

            responder.ValueChanged += OnValueChanged;

            responder.Value = 3.2f;

            responder.ValueChanged -= OnValueChanged;
        }

    }
}