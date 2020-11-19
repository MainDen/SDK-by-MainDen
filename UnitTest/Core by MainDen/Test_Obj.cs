using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainDen.Collections;
using System;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class Test_Obj
    {
        [TestMethod]
        public void Test_Obj_TryGetProperty()
        {
            Obj o1 = new Obj();
            o1.SetProperty("property1", 10);
            object actual;
            o1.TryGetProperty("property1", out actual);
            Assert.AreEqual(10, actual);
            o1.TryGetProperty("property2", out actual);
            Assert.IsNull(actual);
            Assert.ThrowsException<ArgumentNullException>(() => { o1.TryGetProperty(null, out actual); });
        }
        [TestMethod]
        public void Test_Obj_SetProperty()
        {
            Obj o1 = new Obj();
            o1.SetProperty("property1", 10);
            Assert.ThrowsException<ArgumentNullException>(() => { o1.SetProperty("property2", null); });
            Assert.ThrowsException<ArgumentNullException>(() => { o1.SetProperty(null, 1); });
            Assert.AreEqual(1, o1.Properties.Count);
        }
        [TestMethod]
        public void Test_Obj_RemoveProperty()
        {
            Obj o1 = new Obj();
            o1.SetProperty("property1", "value1");
            Assert.ThrowsException<ArgumentNullException>(() => { o1.RemoveProperty(null); });
            Assert.AreEqual(1, o1.Properties.Count);
            Assert.IsTrue(o1.RemoveProperty("property1"));
            Assert.AreEqual(0, o1.Properties.Count);
            Assert.IsFalse(o1.RemoveProperty("property1"));
        }
        [TestMethod]
        public void Test_Obj_ContainsProperty()
        {
            Obj o1 = new Obj();
            o1.SetProperty("property1", "value1");
            Assert.IsTrue(o1.ContainsProperty("property1"));
            Assert.IsFalse(o1.ContainsProperty("property2"));
            Assert.ThrowsException<ArgumentNullException>(() => { o1.ContainsProperty(null); });
        }
        [TestMethod]
        public void Test_Obj_IncludedIn()
        {
            Obj o1 = new Obj();
            IGroup g1 = new Group();
            IGroup g2 = new Group();
            Assert.ThrowsException<ArgumentNullException>(() => { o1.IncludedIn(null); });
            Assert.IsFalse(o1.IncludedIn(g1));
            Assert.IsFalse(o1.IncludedIn(g2));
            g1.Include(o1);
            Assert.IsTrue(o1.IncludedIn(g1));
            Assert.IsFalse(o1.IncludedIn(g2));
        }
        [TestMethod]
        public void Test_Obj_Dispose()
        {
            Obj o1 = new Obj();
            IGroup g1 = new Group();
            IGroup g2 = new Group();
            g1.Include(o1);
            g2.Include(o1);
            Assert.IsTrue(o1.IncludedIn(g1));
            Assert.IsTrue(o1.IncludedIn(g2));
            o1.Dispose();
            Assert.IsFalse(o1.IncludedIn(g1));
            Assert.IsFalse(o1.IncludedIn(g2));
            g1.Include(o1);
            g2.Include(o1);
            using (o1)
            {
                Assert.IsTrue(o1.IncludedIn(g1));
                Assert.IsTrue(o1.IncludedIn(g2));
            }
            Assert.IsFalse(o1.IncludedIn(g1));
            Assert.IsFalse(o1.IncludedIn(g2));
            Assert.AreEqual(0, g1.Entries.Count);
        }
        [TestMethod]
        public void Test_Obj_Clone()
        {
            Obj o1 = new Obj();
            Obj o2 = new Obj();
            Obj o3 = new Obj();
            o1.SetProperty("1", 1);
            o1.SetProperty("o1", o1);
            o1.SetProperty("o2", o2);
            o1.SetProperty("o3", o3);
            o2.SetProperty("2", 2);
            o2.SetProperty("o1", o1);
            o3.SetProperty("3", 3);
            o3.SetProperty("o3", o3);
            Obj c1 = (Obj)o1.Clone();
            Assert.AreEqual(c1["1"], o1["1"]);
            Assert.AreEqual(c1["o1"], c1);
            Assert.AreNotEqual(c1["o2"], o2);
            Assert.AreNotEqual(c1["o3"], o3);
            Assert.AreEqual(((Obj)c1["o2"])["2"], o2["2"]);
            Assert.AreEqual(((Obj)c1["o2"])["o1"], c1);
            Assert.AreEqual(((Obj)c1["o3"])["3"], o3["3"]);
            Assert.AreEqual(((Obj)c1["o3"])["o3"], c1["o3"]);
        }
        [TestMethod]
        public void Test_Obj_CyclicalClone()
        {
            Obj o1 = new Obj();
            Obj o2 = new Obj();
            Obj o3 = new Obj();
            Obj c1;
            o1.SetProperty("1", 1);
            o1.SetProperty("o1", o1);
            o1.SetProperty("o2", o2);
            o1.SetProperty("o3", o3);
            o2.SetProperty("2", 2);
            o2.SetProperty("o1", o1);
            o3.SetProperty("3", 3);
            o3.SetProperty("o3", o3);
            IDictionary<object, object> contract;
            contract = new Dictionary<object, object>();
            contract.Add(o3, o3);
            c1 = (Obj)o1.CyclicalClone(ref contract);
            Assert.AreEqual(c1["o3"], o3);
            contract = new Dictionary<object, object>();
            contract.Add(o2, o2);
            c1 = (Obj)o1.CyclicalClone(ref contract);
            Assert.AreEqual(c1["o2"], o2);
            Assert.AreNotEqual(c1["o3"], o3);
            Assert.AreEqual(((Obj)c1["o2"])["o1"], o1);
            contract = new Dictionary<object, object>();
            contract.Add(o1, o1);
            c1 = (Obj)o1.CyclicalClone(ref contract);
            Assert.AreNotEqual(c1, o1);
            Assert.AreEqual(c1["o1"], o1);
        }
    }
}