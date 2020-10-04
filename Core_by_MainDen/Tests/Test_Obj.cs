using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainDen.Collections;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnitTest_for_SDK_by_MainDen
{
    [TestClass]
    public class Test_Obj
    {
        [TestMethod]
        public void Test_Obj_TryGet()
        {
            Obj o1 = new Obj();
            o1.Set("property1", 10);
            object actual;
            o1.TryGet("property1", out actual);
            Assert.AreEqual(10, actual);
            o1.TryGet("property2", out actual);
            Assert.IsNull(actual);
            Assert.ThrowsException<ArgumentNullException>(() => { o1.TryGet(null, out actual); });
        }
        [TestMethod]
        public void Test_Obj_Set()
        {
            Obj o1 = new Obj();
            o1.Set("property1", 10);
            Assert.ThrowsException<ArgumentNullException>(() => { o1.Set("property2", null); });
            Assert.ThrowsException<ArgumentNullException>(() => { o1.Set(null, 1); });
            Assert.AreEqual(1, o1.Properties.Count);
        }
        [TestMethod]
        public void Test_Obj_Remove()
        {
            Obj o1 = new Obj();
            o1.Set("property1", "value1");
            Assert.ThrowsException<ArgumentNullException>(() => { o1.Remove(null); });
            Assert.AreEqual(1, o1.Properties.Count);
            Assert.IsTrue(o1.Remove("property1"));
            Assert.AreEqual(0, o1.Properties.Count);
            Assert.IsFalse(o1.Remove("property1"));
        }
        [TestMethod]
        public void Test_Obj_Has()
        {
            Obj o1 = new Obj();
            o1.Set("property1", "value1");
            Assert.IsTrue(o1.Has("property1"));
            Assert.IsFalse(o1.Has("property2"));
            Assert.ThrowsException<ArgumentNullException>(() => { o1.Has(null); });
        }
        [TestMethod]
        public void Test_Obj_Empty()
        {
            Obj o1 = new Obj();
            o1.Set("property1", "value1");
            Assert.IsFalse(o1.Empty("property1"));
            Assert.IsTrue(o1.Empty("property2"));
            Assert.ThrowsException<ArgumentNullException>(() => { o1.Empty(null); });
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
            o1.Set("1", 1);
            o1.Set("o1", o1);
            o1.Set("o2", o2);
            o1.Set("o3", o3);
            o2.Set("2", 2);
            o2.Set("o1", o1);
            o3.Set("3", 3);
            o3.Set("o3", o3);
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
            o1.Set("1", 1);
            o1.Set("o1", o1);
            o1.Set("o2", o2);
            o1.Set("o3", o3);
            o2.Set("2", 2);
            o2.Set("o1", o1);
            o3.Set("3", 3);
            o3.Set("o3", o3);
            IDictionary contract;
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
