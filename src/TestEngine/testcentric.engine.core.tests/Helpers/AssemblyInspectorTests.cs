// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace TestCentric.Engine.Helpers
{
    [TestFixture]
    public class AssemblyInspectorTests
    {
#if !NETCOREAPP1_1
        [Test]
        public void CanCreateFromAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var expectedPath = AssemblyHelper.GetAssemblyPath(assembly);
            var rdr = AssemblyInspector.ReadAssembly(assembly);
            Assert.That(rdr.AssemblyPath, Is.SamePath(expectedPath));
        }
#endif

        [TestCase("testcentric.engine.api.xml", typeof(BadImageFormatException))]
        [TestCase("missing.assembly.dll", typeof(FileNotFoundException))]
        public void InvalidAssemblyPathThrowsException(string path, Type exceptionType)
        {
            path = GetAbsolutePath(path);
            Assert.That(() => AssemblyInspector.ReadAssembly(path), Throws.TypeOf(exceptionType));
        }

        [TestCase("TestCentric.Gui.Model.Tests.dll", true, true, false, "v4.0.30319")]
        [TestCase("net35/testcentric.engine.core.tests.exe", true, true, false, "v2.0.50727")]
        [TestCase("net35/testcentric.engine.core.dll", true, true, false, "v2.0.50727")]
        [TestCase("net35/testcentric-agent.exe", true, true, false, "v2.0.50727")]
        [TestCase("net35/testcentric-agent-x86.exe", true, true, true, "v2.0.50727")]
        //[TestCase("net35/mock-cpp-clr-x64.dll", true, true, false, "v2.0.50727")]
        //[TestCase("net35/mock-cpp-clr-x86.dll", true, true, true, "v2.0.50727")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.tests.dll", true, true, false, "v4.0.30319")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.dll", true, true, false, "v4.0.30319")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.tests.dll", true, true, false, "v4.0.30319")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.dll", true, true, false, "v4.0.30319")]
        [TestCase("netstandard2.0/testcentric.engine.core.dll", true, true, false, "v4.0.30319")]
        [TestCase("netstandard1.6/testcentric.engine.core.dll", true, true, false, "v4.0.30319")]
        public void CanCreateFromPath(string path, bool isPeFile, bool isDotNetFile, bool runAsX86, string runTimeVersion)
        {
            path = GetAbsolutePath(path);

            //Assume.That(path, Does.Exist);

            var rdr = AssemblyInspector.ReadAssembly(path);

            Assert.Multiple(() =>
            {
                Assert.That(rdr.AssemblyPath, Is.EqualTo(path));
                Assert.That(rdr.IsValidPeFile, Is.EqualTo(isPeFile));
                Assert.That(rdr.IsDotNetFile, Is.EqualTo(isDotNetFile));
                Assert.That(rdr.ShouldRun32Bit, Is.EqualTo(runAsX86));
                Assert.That(rdr.ImageRuntimeVersion, Is.EqualTo(runTimeVersion));
            });
        }

        // Convert relative paths to absolute based on the parent directory
        private static string GetAbsolutePath(string path)
        {
            if (!Path.IsPathRooted(path))
            {
                var baseDir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory).Parent;
                path = Path.Combine(baseDir.FullName, path);
            }

            return path;
        }
    }
}
