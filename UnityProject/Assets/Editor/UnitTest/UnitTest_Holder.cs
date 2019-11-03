using NUnit.Framework;

namespace cylvester
{
    [TestFixture]
    public class UnitTest_Holder
    {
        [Test]
        public void Construction_capacity()
        {
            var holder = new Holder<int>(2);
            
            holder.Add(1);
            holder.Add(2);
            holder.Add(3);

            Assert.AreEqual(0, holder.IndexOf(1));
            Assert.AreEqual(1, holder.IndexOf(2));
            Assert.IsNull(holder.IndexOf(3));
        }

        [Test]
        public void Add()
        {
            var holder = new Holder<int>(2);
            holder.Add(102);
            
            Assert.AreEqual(0, holder.IndexOf(102));
            Assert.IsNull(holder.IndexOf(103));
        }
        
        [Test]
        public void Add_return()
        {
            var holder = new Holder<int>(2);
            Assert.IsTrue(holder.Add(102));
            Assert.IsTrue(holder.Add(103));
            Assert.IsFalse(holder.Add(103));
        }

        [Test]
        public void Add_unique()
        {
            var holder = new Holder<int>(2);
            holder.Add(102);
            holder.Add(102); // doesn't affect
            holder.Add(103);
            
            Assert.IsNull(holder.IndexOf(103));
        }
        
        [Test]
        public void Remove()
        {
            var holder = new Holder<int>(2);
            holder.Add(102);
            holder.Remove(102);

            Assert.IsNull(holder.IndexOf(102));
        }
    }
}