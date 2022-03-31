using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace StripeIntegration.Tests
{
    public abstract class TestBase
    {
        protected List<IDisposable> _transientResources;

        [TestInitialize]
        public virtual void TestInitialize()
        {
            _transientResources = new List<IDisposable>();

        }

        [TestCleanup]
        public void TestCleanUp()
        {
            _transientResources.ForEach(x => x.Dispose());
            _transientResources = new List<IDisposable>();
        }
    }
}
