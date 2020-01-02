// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using Mono.Cecil.Metadata;

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

        [TestCase("TestCentric.Gui.Model.Tests.dll")]
        [TestCase("net35/testcentric.engine.core.tests.exe")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.tests.dll")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.tests.dll")]
        [TestCase("netstandard2.0/testcentric.engine.core.dll")]
        [TestCase("netstandard1.6/testcentric.engine.core.dll")]
        public void CanCreateFromPath(string path)
        {
            path = GetAbsolutePath(path);
            var inspector = AssemblyInspector.ReadAssembly(path);

            Assert.That(inspector.AssemblyPath, Is.EqualTo(path));
            Assert.True(inspector.IsValidPeFile);
            Assert.True(inspector.IsDotNetFile);
        }

        [TestCase("net35/testcentric-agent.exe", false)]
        [TestCase("net35/testcentric-agent-x86.exe", true)]
        public void CanDetectRunAsX96(string path, bool runAsX86)
        {
            var inspector = AssemblyInspector.ReadAssembly(GetAbsolutePath(path));
            Assert.That(inspector.ShouldRun32Bit, Is.EqualTo(runAsX86));
        }

        [TestCase("TestCentric.Gui.Model.Tests.dll", "v4.0.30319")]
        [TestCase("net35/testcentric.engine.core.tests.exe", "v2.0.50727")]
        [TestCase("net35/testcentric.engine.core.dll", "v2.0.50727")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.tests.dll", "v4.0.30319")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.dll", "v4.0.30319")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.tests.dll", "v4.0.30319")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.dll", "v4.0.30319")]
        [TestCase("netstandard2.0/testcentric.engine.core.dll", "v4.0.30319")]
        [TestCase("netstandard1.6/testcentric.engine.core.dll", "v4.0.30319")]
        public void CanAccessRuntimeVersion(string path, string runtimeVersion)
        {
            var inspector = AssemblyInspector.ReadAssembly(GetAbsolutePath(path));
            Assert.That(inspector.ImageRuntimeVersion, Is.EqualTo(runtimeVersion));
        }

        [TestCase("TestCentric.Gui.Model.Tests.dll", "nunit.framework", "3.11.0.0")]
        [TestCase("TestCentric.Gui.Model.dll", "testcentric.engine.api")]
        [TestCase("net35/testcentric.engine.core.tests.exe", "nunit.framework", "3.11.0.0")]
        [TestCase("net35/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.tests.dll", "nunit.framework", "3.11.0.0")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.tests.dll", "nunit.framework", "3.11.0.0")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netstandard2.0/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netstandard1.6/testcentric.engine.core.dll", "testcentric.engine.api")]
        public void CanEnumerateReferences(string path, string refName = null, string refVersion = null)
        {
            var inspector = AssemblyInspector.ReadAssembly(GetAbsolutePath(path));

            Assert.That(inspector.HasTable(Table.AssemblyRef));

            if (refName == null)
                return;

            foreach (var reference in inspector.AssemblyReferences)
                if (reference.Name == refName)
                {
                    if (refVersion != null)
                        Assert.That(reference.Version, Is.EqualTo(new Version(refVersion)));
                    Assert.Pass();
                }

            Assert.Fail($"No reference to {refName} was found");
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
