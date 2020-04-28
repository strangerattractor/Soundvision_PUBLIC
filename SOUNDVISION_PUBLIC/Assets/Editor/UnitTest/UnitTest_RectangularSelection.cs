// using NUnit.Framework;
// using UnityEngine;

// namespace cylvester
// {
//     [TestFixture]
//     public class UnitTest_RectangularSelection
//     {
//         [Test]
//         public void Update()
//         {
//             var paintSpace = new Rect(0, 0, 100, 100); // GUI
//             var rectangularSelection =  new RectangularSelection(1000, 1000); // texture 10 times larger
//             rectangularSelection.Start(new Vector2(10, 10), ref paintSpace);
//             var selectionInTexture = rectangularSelection.Update(new Vector2(20, 20), ref paintSpace);
            
//             var expected = new Rect(100, 100, 100, 100);
//             Assert.AreEqual(expected, selectionInTexture);
//         }
//     }
// }