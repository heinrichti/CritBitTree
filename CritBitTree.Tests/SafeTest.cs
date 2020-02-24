using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CritBitTree.Tests
{
    [TestClass]
    public class SafeTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var critBitTree = new CritBitTreeSafe<object>();

            var helloWorldBytes = Encoding.ASCII.GetBytes("Hello world");
            var result = critBitTree.Add(helloWorldBytes, null);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.ContainsKey(helloWorldBytes));

            result = critBitTree.Add(helloWorldBytes, null);
            Assert.IsFalse(result);

            var testBytes = Encoding.ASCII.GetBytes("Test");
            result = critBitTree.Add(testBytes, null);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));
            Assert.IsFalse(critBitTree.ContainsKey(Encoding.ASCII.GetBytes("alsdjfaösfdölkjsa")));

            result = critBitTree.Add(testBytes, null);
            Assert.IsFalse(result);
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));

            var helloTestBytes = Encoding.ASCII.GetBytes("Hello test");
            result = critBitTree.Add(helloTestBytes, null);
            Assert.IsTrue(result);

            result = critBitTree.Add(helloTestBytes, null);
            Assert.IsFalse(result);

            result = critBitTree.Add(helloTestBytes, null);
            Assert.IsFalse(result);
            result = critBitTree.Add(helloTestBytes, null);
            Assert.IsFalse(result);
            result = critBitTree.Add(helloTestBytes, null);
            Assert.IsFalse(result);

            Assert.IsTrue(critBitTree.ContainsKey(helloWorldBytes));
            Assert.IsTrue(critBitTree.ContainsKey(helloTestBytes));
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));

            Assert.IsFalse(critBitTree.ContainsKey(Encoding.ASCII.GetBytes("Hello my test")));
            Assert.IsFalse(critBitTree.ContainsKey(Encoding.ASCII.GetBytes("asf")));
            Assert.IsFalse(critBitTree.ContainsKey(Encoding.ASCII.GetBytes("ulululu")));

            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu"), null));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu1"), null));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu2"), null));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu3"), null));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu4"), null));

            var longBytes = new byte[1000];
            Assert.IsTrue(critBitTree.Add(longBytes, null));
            Assert.IsTrue(critBitTree.ContainsKey(longBytes));
        }
    }
}
