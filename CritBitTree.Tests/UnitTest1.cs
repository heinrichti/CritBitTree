using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CritBitTree.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            using var critBitTree = new CritBitTree();

            var helloWorldBytes = Encoding.ASCII.GetBytes("Hello world");
            var result = critBitTree.Add(helloWorldBytes);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.Contains(helloWorldBytes));

            result = critBitTree.Add(helloWorldBytes);
            Assert.IsFalse(result);

            var testBytes = Encoding.ASCII.GetBytes("Test");
            result = critBitTree.Add(testBytes);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.Contains(testBytes));
            Assert.IsFalse(critBitTree.Contains(Encoding.ASCII.GetBytes("alsdjfaösfdölkjsa")));

            result = critBitTree.Add(testBytes);
            Assert.IsFalse(result);
            Assert.IsTrue(critBitTree.Contains(testBytes));

            var helloTestBytes = Encoding.ASCII.GetBytes("Hello test");
            result = critBitTree.Add(helloTestBytes);
            Assert.IsTrue(result);

            result = critBitTree.Add(helloTestBytes);
            Assert.IsFalse(result);

            result = critBitTree.Add(helloTestBytes);
            Assert.IsFalse(result);
            result = critBitTree.Add(helloTestBytes);
            Assert.IsFalse(result);
            result = critBitTree.Add(helloTestBytes);
            Assert.IsFalse(result);

            Assert.IsTrue(critBitTree.Contains(helloWorldBytes));
            Assert.IsTrue(critBitTree.Contains(helloTestBytes));
            Assert.IsTrue(critBitTree.Contains(testBytes));

            Assert.IsFalse(critBitTree.Contains(Encoding.ASCII.GetBytes("Hello my test")));
            Assert.IsFalse(critBitTree.Contains(Encoding.ASCII.GetBytes("asf")));
            Assert.IsFalse(critBitTree.Contains(Encoding.ASCII.GetBytes("ulululu")));

            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu")));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu1")));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu2")));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu3")));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu4")));
        }
    }
}
