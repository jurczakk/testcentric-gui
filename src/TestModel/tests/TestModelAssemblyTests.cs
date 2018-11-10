﻿// ***********************************************************************
// Copyright (c) 2018 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System.IO;
using NUnit.Engine;
using NUnit.Framework;
using NUnit.Tests.Assemblies;

namespace TestCentric.Gui.Model
{
    public class TestModelAssemblyTests
    {
        private const string MOCK_ASSEMBLY = "mock-assembly.dll";

        private ITestModel _model;

        [SetUp]
        public void CreateTestModel()
        {
            var engine = TestEngineActivator.CreateInstance();
            Assert.NotNull(engine, "Unable to create engine instance for testing");

            _model = new TestModel(engine);

            _model.LoadTests(new[] { Path.Combine(TestContext.CurrentContext.TestDirectory, MOCK_ASSEMBLY) });
        }

        [TearDown]
        public void ReleaseTestModel()
        {
            _model.Dispose();
        }

        [Test]
        public void CheckStateAfterLoading()
        {
            Assert.That(_model.HasTests, "HasTests");
            Assert.NotNull(_model.Tests, "Tests");
            Assert.False(_model.HasResults, "HasResults");

            var testRun = _model.Tests;
            Assert.That(testRun.RunState, Is.EqualTo(RunState.Runnable), "RunState of test-run");
            Assert.That(testRun.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of test-run");
            Assert.That(testRun.Children.Count, Is.EqualTo(1), "Child count of test-run");

            var testAssembly = testRun.Children[0];
            Assert.That(testAssembly.RunState, Is.EqualTo(RunState.Runnable), "RunState of assembly");
            Assert.That(testAssembly.TestCount, Is.EqualTo(MockAssembly.Tests), "TestCount of assembly");
        }

        [Test]
        public void CheckStateAfterRunningTests()
        {
            RunAllTestsAndWaitForCompletion();

            Assert.That(_model.HasTests, "HasTests");
            Assert.NotNull(_model.Tests, "Tests");
            Assert.That(_model.HasResults, "HasResults");
        }

        [Test]
        public void CheckStateAfterUnloading()
        {
            _model.UnloadTests();

            Assert.False(_model.HasTests, "HasTests");
            Assert.Null(_model.Tests, "Tests");
            Assert.False(_model.HasResults, "HasResults");
        }

        [Test]
        public void CheckStateAfterReloading()
        {
            _model.ReloadTests();
 
            Assert.That(_model.HasTests, "HasTests");
            Assert.NotNull(_model.Tests, "Tests");
            Assert.False(_model.HasResults, "HasResults");
        }

        [Test]
        public void TestTreeIsUnchangedByReload()
        {
            var originalTests = _model.Tests;

            _model.ReloadTests();

            Assert.Multiple(() => CheckNodesAreEqual(originalTests, _model.Tests));
        }

        private void RunAllTestsAndWaitForCompletion()
        {
            bool runComplete = false;
            _model.Events.RunFinished += (r) => runComplete = true;

            _model.RunAllTests();

            while (!runComplete)
                System.Threading.Thread.Sleep(1);
        }

        private void CheckNodesAreEqual(TestNode beforeReload, TestNode afterReload)
        {
            Assert.That(afterReload.Name, Is.EqualTo(beforeReload.Name));
            Assert.That(afterReload.FullName, Is.EqualTo(beforeReload.FullName));
            Assert.That(afterReload.Id, Is.EqualTo(beforeReload.Id), $"Different IDs for {beforeReload.Name}");
            Assert.That(afterReload.IsSuite, Is.EqualTo(beforeReload.IsSuite));

            if (afterReload.IsSuite)
            {
                Assert.That(afterReload.Children.Count, Is.EqualTo(beforeReload.Children.Count), $"Different number of children for {beforeReload.Name}");
                for (int i = 0; i < afterReload.Children.Count; i++)
                    CheckNodesAreEqual(beforeReload.Children[i], afterReload.Children[i]);
            }
        }
    }
}
