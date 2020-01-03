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
using TestCentric.Engine.Helpers;

namespace TestCentric.Engine.Metadata
{
    [TestFixture]
    public class AssemblyViewTests
    {
#if !NETCOREAPP1_1
        [Test]
        public void CanCreateFromAssembly()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var expectedPath = AssemblyHelper.GetAssemblyPath(assembly);
            var rdr = AssemblyView.ReadAssembly(assembly);
            Assert.That(rdr.AssemblyPath, Is.SamePath(expectedPath));
        }
#endif

        [TestCase("testcentric.engine.api.xml", typeof(BadImageFormatException))]
        [TestCase("missing.assembly.dll", typeof(FileNotFoundException))]
        public void InvalidAssemblyPathThrowsException(string path, Type exceptionType)
        {
            path = GetAbsolutePath(path);
            Assert.That(() => AssemblyView.ReadAssembly(path), Throws.TypeOf(exceptionType));
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
            var assembly = AssemblyView.ReadAssembly(path);

            Assert.That(assembly.AssemblyPath, Is.EqualTo(path));
            Assert.True(assembly.IsValidPeFile);
            Assert.True(assembly.IsDotNetFile);
        }

        [TestCase("net35/testcentric-agent.exe", false)]
        [TestCase("net35/testcentric-agent-x86.exe", true)]
        public void CanDetectRunAsX96(string path, bool runAsX86)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            Assert.That(assembly.ShouldRun32Bit, Is.EqualTo(runAsX86));
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
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            Assert.That(assembly.ImageRuntimeVersion, Is.EqualTo(runtimeVersion));
        }

        [TestCase("TestCentric.Gui.Model.Tests.dll", "nunit.framework")]
        [TestCase("TestCentric.Gui.Model.dll", "testcentric.engine.api")]
        [TestCase("net35/testcentric.engine.core.tests.exe", "nunit.framework")]
        [TestCase("net35/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.tests.dll", "nunit.framework")]
        [TestCase("netcoreapp2.1/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.tests.dll", "nunit.framework")]
        [TestCase("netcoreapp1.1/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netstandard2.0/testcentric.engine.core.dll", "testcentric.engine.api")]
        [TestCase("netstandard1.6/testcentric.engine.core.dll", "testcentric.engine.api")]
        public void CanEnumerateReferences(string path, string refName = null)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));

            Assert.That(assembly.HasTable(Table.AssemblyRef));

            if (refName == null)
                return;

            foreach (var reference in assembly.AssemblyReferences)
                if (reference.Name == refName)
                {
                    // See if our constructed AssemblyName matches one created by .NET.
                    // We can only do this readily when the referenced assembly targets
                    // .NET 3.5 and we are running under .NET 35, but it's still useful.
#if NET35
                    if (path.StartsWith("net35"))
                    {
                        var assumedFilePath = GetAbsolutePath(Path.Combine("net35", refName + ".dll"));
                        if (File.Exists(assumedFilePath))
                        {
                            var expectedAssemblyName = AssemblyName.GetAssemblyName(assumedFilePath);
                            Assert.That(reference.FullName, Is.EqualTo(expectedAssemblyName.FullName));
                        }
                    }
#endif
                    Assert.Pass();
                }

            Assert.Fail($"No reference to {refName} was found");
        }

        [Test]
        public void CanEnumerateAssemblyAttributes()
        {
            var path = GetAbsolutePath("TestCentric.Gui.Model.Tests.dll");
            var assembly = AssemblyView.ReadAssembly(path);

            Assert.That(assembly.HasTable(Table.CustomAttribute));
            Assert.NotNull(assembly.CustomAttributes);
            Console.WriteLine($"Found {assembly.CustomAttributes.Length} Attributes on the Assembly");

            foreach (var attr in assembly.CustomAttributes)
            {
                Console.WriteLine($"Tag = {attr.Type & 7}");
                Console.WriteLine($"Offset = {attr.Type >> 3}");
                Console.WriteLine($"Blob = {attr.Value}");
            }
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
