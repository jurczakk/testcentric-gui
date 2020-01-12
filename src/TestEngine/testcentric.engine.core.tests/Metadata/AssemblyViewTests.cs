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

            foreach (var attr in assembly.CustomAttributes)
            {
                //Console.WriteLine($"ResolutionScope = {attr.ResolutionScope}");
                //Console.WriteLine($"TypeName = {attr.TypeName}");asa
                //Console.WriteLine($"TypeNamespace = {attr.TypeNamespace}");
                Console.WriteLine(attr);
            }
        }

        [TestCase("mock-assembly.dll", "NUnit.Framework.NonTestAssemblyAttribute", ExpectedResult = true)]
        [TestCase("net35/mock-assembly.dll", "NUnit.Framework.NonTestAssemblyAttribute", ExpectedResult = false)]
        [TestCase("netcoreapp2.1/mock-assembly.dll", "System.Reflection.AssemblyCopyrightAttribute", ExpectedResult = true)]
        [TestCase("netcoreapp1.1/mock-assembly.dll", "System.Reflection.AssemblyTitleAttribute", ExpectedResult = true)]
        public bool HasCustomAttribute(string path, string attrName)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            return assembly.HasCustomAttribute(attrName);
        }

        [TestCase("mock-assembly.dll", "NUnit.Framework.NonTestAssemblyAttribute")]
        [TestCase("net35/mock-assembly.dll", "System.Reflection.AssemblyCopyrightAttribute")]
        [TestCase("netcoreapp2.1/mock-assembly.dll", "System.Reflection.AssemblyCopyrightAttribute")]
        [TestCase("netcoreapp1.1/mock-assembly.dll", "System.Reflection.AssemblyTitleAttribute")]
        public void GetCustomAttribute(string path, string attrName)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            var attr = assembly.GetCustomAttribute(attrName);
            Assert.NotNull(attr, "Attribute not found");
            Assert.That(attr.FullName, Is.EqualTo(attrName));
        }

        [TestCase("NUnit.Framework.NonTestAssemblyAttribute")]
        [TestCase("NUnit.Framework.LevelOfParallelismAttribute", 3)]
        [TestCase("Test.Attributes.NoArgsAttribute")]
        public void GetCustomAttributeAndArguments(string attrName, params object[] expectedArgs)
        {
            // For this test, we want the current build, so we use TestDirectory
            var assembly = AssemblyView.ReadAssembly(Path.Combine(TestContext.CurrentContext.TestDirectory, "notest-assembly.dll"));
            var attr = assembly.GetCustomAttribute(attrName);
            Assert.NotNull(attr, "Attribute not found");
            Assert.That(attr.FullName, Is.EqualTo(attrName));
            Assert.That(attr.Arguments, Is.EqualTo(expectedArgs));
        }

        public enum TestAssemblyType
        {
            None,
            NUnit2,
            NUnit3
        }

        [TestCase("mock-assembly.dll", TestAssemblyType.NUnit3)]
        [TestCase("testcentric.engine.api.dll", TestAssemblyType.None)]
        [TestCase("v2-tests/mock-assembly.dll", TestAssemblyType.NUnit2)]
        public void CanRecognizeTestAssemblies(string path, TestAssemblyType expectedType)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            var actualType = TestAssemblyType.None;

            foreach (var reference in assembly.AssemblyReferences)
            {
                if (reference.Name == "nunit.framework")
                {
                    actualType = reference.Version.Major >= 3 ? TestAssemblyType.NUnit3 : TestAssemblyType.NUnit2;
                    break;
                }
            }

            Assert.That(actualType, Is.EqualTo(expectedType));
        }

        [TestCase("mock-assembly.dll", ".NETFramework,Version=v4.5")]
        [TestCase("net35/mock-assembly.dll", ".NETFramework,Version=v2.0")] // should be 3.5
        [TestCase("netcoreapp2.1/mock-assembly.dll", ".NETCoreApp,Version=v2.1")]
        [TestCase("netcoreapp1.1/mock-assembly.dll", ".NETCoreApp,Version=v1.1")]
        public void CanDetectTargetRuntime(string path, string frameworkName)
        {
            var assembly = AssemblyView.ReadAssembly(GetAbsolutePath(path));
            Assert.That(assembly.TargetFramework, Is.EqualTo(frameworkName));
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
