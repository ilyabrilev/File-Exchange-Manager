using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace File_Exchange_Manager.UnitTest
{
    [TestFixture]
    class UtilsTests
    {        
        [TestCase(eWorkInterval._Other, "Day")]
        [TestCase(eWorkInterval._Other, "abrvalg")]
        [TestCase(eWorkInterval.Days, "Days")]
        [TestCase(eWorkInterval.Months, "Months")]
        [TestCase(eWorkInterval.Years, "Years")]
        [TestCase(eWorkInterval.Years, "years")]
        public void ParseWorkIntervalEnum_BadEnterString_EqualFirstPar(eWorkInterval itn, string _s)
        {
            Assert.AreEqual(itn, GlobalUtils.ParseWorkIntervalEnum(_s));
        }
    }
}
