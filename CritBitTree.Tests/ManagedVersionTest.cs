using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CritBitTree.Tests
{
    [TestClass]
    public class ManagedVersionTest
    {
        private const string HelloWorldString = "Hello world";
        private const string TestString = "Test";
        private const string HelloTestString = "Hello test";

        [TestMethod]
        public void TestMethod1()
        {
            var critBitTree = new CritBitTree<string>();

            var helloWorldBytes = Encoding.ASCII.GetBytes(HelloWorldString);
            var result = critBitTree.Add(helloWorldBytes, HelloWorldString);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.ContainsKey(helloWorldBytes));

            result = critBitTree.Add(helloWorldBytes, HelloWorldString);
            Assert.IsFalse(result);

            var testBytes = Encoding.ASCII.GetBytes(TestString);
            result = critBitTree.Add(testBytes, TestString);
            Assert.IsTrue(result);
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));
            Assert.IsFalse(critBitTree.ContainsKey(Encoding.ASCII.GetBytes("alsdjfaösfdölkjsa")));

            result = critBitTree.Add(testBytes, null);
            Assert.IsFalse(result);
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));

            var helloTestBytes = Encoding.ASCII.GetBytes(HelloTestString);
            result = critBitTree.Add(helloTestBytes, HelloTestString);
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

            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu"), "ulululu"));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu1"), "ulululu1"));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu2"), "ulululu2"));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu3"), "ulululu3"));
            Assert.IsTrue(critBitTree.Add(Encoding.ASCII.GetBytes("ulululu4"), "ulululu4"));

            var longBytes = new byte[1000];
            Assert.IsTrue(critBitTree.Add(longBytes, "Long bytes"));
            Assert.IsTrue(critBitTree.ContainsKey(longBytes));            

            Assert.IsFalse(critBitTree.Remove(Encoding.ASCII.GetBytes("asdf")));
            var count = critBitTree.Count();

            Assert.IsTrue(critBitTree.Remove(helloWorldBytes));

            Assert.IsTrue(critBitTree.Count() == count -1);
            Assert.IsFalse(critBitTree.ContainsKey(helloWorldBytes));
            Assert.IsTrue(critBitTree.ContainsKey(testBytes));
        }
    }
}
