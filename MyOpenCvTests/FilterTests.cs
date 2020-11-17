using System;
using System.Drawing;
using System.Drawing.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyOpenCvTests
{
    using MyOpenCv;
    using MyOpenCv.ImageProcessing;

    [TestClass]
    public class FilterTests
    {
        [TestMethod]
        public void UnsetParameterAccess()
        {
            var f = new RgbSplitFilter();
            var b = new Bitmap(100, 100);
            Assert.ThrowsException<ArgumentException>(() => f.Process(b));
        }

        [FilterParameter(typeof(double), "a")]
        class Filter1 : InPlaceFilter
        {
            public override string Name { get { return "name"; } }
            public override bool BitmapCondition(Bitmap bitmap)
            {
                return true;
            }
            public override void Process(Bitmap bitmap)
            {
                GetParameter<double>("b");
            }
        }

        [FilterParameter(typeof(double), "a")]
        class Filter2 : InPlaceFilter
        {
            public override string Name { get { return "name"; } }
            public override bool BitmapCondition(Bitmap bitmap)
            {
                return true;
            }
            public override void Process(Bitmap bitmap)
            {
                GetParameter<int>("a");
            }
        }

        [TestMethod]
        public void WrongNameParameterAccess()
        {
            var f = new Filter1();
            var b = new Bitmap(100, 100);
            Assert.ThrowsException<ParameterNameException>(() => f.Process(b));
        }

        [TestMethod]
        public void WrongTypeParameterAccess()
        {
            var f = new Filter2();
            var b = new Bitmap(100, 100);
            Assert.ThrowsException<ParameterTypeException>(() => f.Process(b));
        }

        [TestMethod]
        public void WrongNameParameterAttribution()
        {
            var f = new Filter1();
            Assert.ThrowsException<ParameterNameException>(() => f.SetParameter("b", 1));
        }

        [TestMethod]
        public void WrongTypeParameterAttribution()
        {
            var f = new Filter1();
            var b = new Bitmap(100, 100);
            Assert.ThrowsException<ParameterTypeException>(() => f.SetParameter("a", 1));
        }
    }
}
