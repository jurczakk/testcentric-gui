// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using TestCentric.Engine.Extensibility;
using NUnit.Framework;

namespace TestCentric.Engine.Runners
{
    using Fakes;

    public class DirectTestRunnerTests
    {
        private IFrameworkDriver _driver;
        private EmptyDirectTestRunner _directTestRunner;
        private readonly TestFilter _testFilter = new TestFilter(string.Empty);

        [SetUp]
        public void Initialize()
        {
            _driver = Substitute.For<IFrameworkDriver>();

            var driverService = Substitute.For<IDriverService>();
            driverService.GetDriver(
#if !NETCOREAPP1_1
                AppDomain.CurrentDomain,
                string.Empty,
#endif 
                string.Empty, 
                false).ReturnsForAnyArgs(_driver);

            var serviceLocator = Substitute.For<IServiceLocator>();
            serviceLocator.GetService<IDriverService>().Returns(driverService);

            _directTestRunner = new EmptyDirectTestRunner(serviceLocator, new TestPackage("mock-assembly.dll"));
        }

        [Test]
        public void Explore_Passes_Along_NUnitEngineException()
        {
            _driver.Explore(Arg.Any<string>()).Throws(new NUnitEngineException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Explore(new TestFilter(string.Empty)));
            Assert.That(ex.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Explore_Throws_NUnitEngineException()
        {
            _driver.Explore(Arg.Any<string>()).Throws(new ArgumentException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Explore(new TestFilter(string.Empty)));
            Assert.That(ex.InnerException is ArgumentException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Load_Passes_Along_NUnitEngineException()
        {
            _driver.Load(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>()).Throws(new NUnitEngineException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Load());
            Assert.That(ex.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Load_Throws_NUnitEngineException()
        {
            _driver.Load(Arg.Any<string>(), Arg.Any<Dictionary<string, object>>()).Throws(new ArgumentException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Load());
            Assert.That(ex.InnerException is ArgumentException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void CountTestCases_Passes_Along_NUnitEngineException()
        {
            _driver.CountTestCases(Arg.Any<string>()).Throws(new NUnitEngineException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.CountTestCases(_testFilter));
            Assert.That(ex.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void CountTestCases_Throws_NUnitEngineException()
        {
            _driver.CountTestCases(Arg.Any<string>()).Throws(new ArgumentException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.CountTestCases(_testFilter));
            Assert.That(ex.InnerException is ArgumentException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Run_Passes_Along_NUnitEngineException()
        {
            _driver.Run(Arg.Any<ITestEventListener>(), Arg.Any<string>()).Throws(new NUnitEngineException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Run(Substitute.For<ITestEventListener>(), _testFilter));
            Assert.That(ex.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void Run_Throws_NUnitEngineException()
        {
            _driver.Run(Arg.Any<ITestEventListener>(), Arg.Any<string>()).Throws(new ArgumentException("Message"));
            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.Run(Substitute.For<ITestEventListener>(), _testFilter));
            Assert.That(ex.InnerException is ArgumentException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void StopRun_Passes_Along_NUnitEngineException()
        {
            _driver.When(x => x.StopRun(Arg.Any<bool>()))
                .Do(x => { throw new NUnitEngineException("Message"); });

            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.StopRun(true));
            Assert.That(ex.Message, Is.EqualTo("Message"));
        }

        [Test]
        public void StopRun_Throws_NUnitEngineException()
        {
            _driver.When(x => x.StopRun(Arg.Any<bool>()))
                .Do(x => { throw new ArgumentException("Message"); });

            var ex = Assert.Throws<NUnitEngineException>(() => _directTestRunner.StopRun(true));
            Assert.That(ex.InnerException is ArgumentException);
            Assert.That(ex.InnerException.Message, Is.EqualTo("Message"));
        }
    }
}
