using Microsoft.VisualStudio.TestTools.UnitTesting;
using MainDen.Collections;
using System;
using System.Drawing;

namespace UnitTest_for_SDK_by_MainDen
{
    [TestClass]
    public class Test_Group
    {
        [TestMethod]
        public void Test_Group_Include()
        {
            Group g1 = new Group();
            Obj o1 = new Obj();
            Obj o2 = new Obj();
            Assert.AreEqual(0, g1.Entries.Count);
            Assert.AreEqual(0, o1.Groups.Count);
            Assert.AreEqual(0, o2.Groups.Count);
            g1.Include(o1);
            Assert.AreEqual(1, g1.Entries.Count);
            Assert.AreEqual(1, o1.Groups.Count);
            Assert.AreEqual(0, o2.Groups.Count);
            g1.Include(o2);
            Assert.AreEqual(2, g1.Entries.Count);
            Assert.AreEqual(1, o1.Groups.Count);
            Assert.AreEqual(1, o2.Groups.Count);
            g1.Include(new Rectangle(0, 0, 1, 1));
            Assert.AreEqual(3, g1.Entries.Count);
            g1.Include(new Rectangle(0, 0, 1, 1));
            Assert.AreEqual(3, g1.Entries.Count);
            g1.Include("12");
            Assert.AreEqual(4, g1.Entries.Count);
            g1.Include("12");
            Assert.AreEqual(4, g1.Entries.Count);
        }
        [TestMethod]
        public void Test_Group_Exclude()
        {
            Group g1 = new Group();
            Group g2 = new Group();
            Obj o1 = new Obj();
            Obj o2 = new Obj();
            Assert.AreEqual(0, g1.Entries.Count);
            Assert.AreEqual(0, g2.Entries.Count);
            Assert.AreEqual(0, o1.Groups.Count);
            Assert.AreEqual(0, o2.Groups.Count);
            g1.Include(o1);
            g1.Include(o2);
            g1.Include(g2);
            g1.Include(1);
            Assert.AreEqual(4, g1.Entries.Count);
            Assert.AreEqual(1, o1.Groups.Count);
            Assert.AreEqual(1, o2.Groups.Count);
            g1.Exclude(1);
            Assert.AreEqual(3, g1.Entries.Count);
            g1.Exclude(o1);
            Assert.AreEqual(2, g1.Entries.Count);
            Assert.AreEqual(0, o1.Groups.Count);
            Assert.AreEqual(1, o2.Groups.Count);
            g1.Exclude(o2);
            Assert.AreEqual(1, g1.Entries.Count);
            Assert.AreEqual(0, o1.Groups.Count);
            Assert.AreEqual(0, o2.Groups.Count);
            g1.Exclude(g2);
            Assert.AreEqual(0, g1.Entries.Count);
        }
        [TestMethod]
        public void Test_Group_Contains()
        {
            Group g1 = new Group();
            Group g2 = new Group();
            Obj o1 = new Obj();
            Obj o2 = new Obj();
            Assert.ThrowsException<ArgumentNullException>(() => g1.Contains(null));
            Assert.IsFalse(g1.Contains(o1));
            Assert.IsFalse(g1.Contains(o2));
            Assert.IsFalse(g1.Contains(g1));
            Assert.IsFalse(g1.Contains(g2));
            g1.Include(o1);
            Assert.IsTrue(g1.Contains(o1));
            Assert.IsFalse(g1.Contains(o2));
            Assert.IsFalse(g1.Contains(g1));
            Assert.IsFalse(g1.Contains(g2));
            g1.Include(o2);
            Assert.IsTrue(g1.Contains(o1));
            Assert.IsTrue(g1.Contains(o2));
            Assert.IsFalse(g1.Contains(g1));
            Assert.IsFalse(g1.Contains(g2));
            g1.Include(g1);
            Assert.IsTrue(g1.Contains(o1));
            Assert.IsTrue(g1.Contains(o2));
            Assert.IsTrue(g1.Contains(g1));
            Assert.IsFalse(g1.Contains(g2));
            g1.Include(g2);
            Assert.IsTrue(g1.Contains(o1));
            Assert.IsTrue(g1.Contains(o2));
            Assert.IsTrue(g1.Contains(g1));
            Assert.IsTrue(g1.Contains(g2));
            Assert.IsFalse(g1.Contains(1));
            g1.Include(1);
            Assert.IsTrue(g1.Contains(o1));
            Assert.IsTrue(g1.Contains(o2));
            Assert.IsTrue(g1.Contains(g1));
            Assert.IsTrue(g1.Contains(g2));
            Assert.IsTrue(g1.Contains(1));
        }
        [TestMethod]
        public void Test_Group_IncludedIn()
        {
            Group g1 = new Group();
            Group g2 = new Group();
            Assert.ThrowsException<ArgumentNullException>(() => g1.IncludedIn(null));
            Assert.IsFalse(g1.IncludedIn(g1));
            Assert.IsFalse(g1.IncludedIn(g2));
            Assert.IsFalse(g2.IncludedIn(g1));
            Assert.IsFalse(g2.IncludedIn(g2));
            g1.Include(g1);
            Assert.IsTrue(g1.IncludedIn(g1));
            Assert.IsFalse(g1.IncludedIn(g2));
            Assert.IsFalse(g2.IncludedIn(g1));
            Assert.IsFalse(g2.IncludedIn(g2));
            g1.Include(g2);
            Assert.IsTrue(g1.IncludedIn(g1));
            Assert.IsFalse(g1.IncludedIn(g2));
            Assert.IsTrue(g2.IncludedIn(g1));
            Assert.IsFalse(g2.IncludedIn(g2));
            g2.Include(g1);
            g1.Exclude(g2);
            Assert.IsTrue(g1.IncludedIn(g1));
            Assert.IsTrue(g1.IncludedIn(g2));
            Assert.IsFalse(g2.IncludedIn(g1));
            Assert.IsFalse(g2.IncludedIn(g2));
        }
        [TestMethod]
        public void Test_Group_Dispose()
        {
            Group g1 = new Group();
            Group g2 = new Group();
            Group g3 = new Group();
            Group g4 = new Group();
            Obj o1 = new Obj();
            g1.Include(g1);
            g1.Include(g2);
            g1.Include(g3);
            g2.Include(g1);
            g2.Include(g3);
            g2.Include(g4);
            g1.Include(o1);
            g1.Include("12");
            g1.Include("12");
            g1.Include(1);
            Assert.IsTrue(o1.IncludedIn(g1));
            Assert.IsTrue(g1.IncludedIn(g1));
        }
    }
}
