// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

namespace TestCentric.Engine.Helpers
{
    /// <summary>
    /// AssemblyInspector knows how to find various things in an assembly header
    /// </summary>
    public class AssemblyInspector : IDisposable
    {
        private Image _image;

        public static AssemblyInspector ReadAssembly(string assemblyPath)
        {
            return new AssemblyInspector(assemblyPath);
        }

        public static AssemblyInspector ReadAssembly(Assembly assembly)
        {
            return new AssemblyInspector(AssemblyHelper.GetAssemblyPath(assembly));
        }

        public string AssemblyPath { get; private set; }

        public bool IsValidPeFile { get; private set; }
        public bool IsDotNetFile { get; private set; }

        public bool ShouldRun32Bit
        {
            get
            {
                const ModuleAttributes nativeEntryPoint = (ModuleAttributes)16;
                const ModuleAttributes mask = ModuleAttributes.Required32Bit | nativeEntryPoint;

                return _image.Architecture != TargetArchitecture.AMD64 &&
                       _image.Architecture != TargetArchitecture.IA64 &&
                       (_image.Attributes & mask) != 0;
            }
        }

        public string ImageRuntimeVersion
        {
            get { return _image.RuntimeVersion; }
        }

        public void Dispose()
        {
            if (_image != null)
                _image.Dispose();
        }

        private AssemblyInspector(string assemblyPath)
        {
            AssemblyPath = assemblyPath;

            var fs = new FileStream(AssemblyPath, FileMode.Open, FileAccess.Read);
            _image = ImageReader.ReadImage(new Mono.Disposable<Stream>(fs, true), assemblyPath);

            // Mono.Cecil throws an exception if either of these would be false
            // TODO: Should we catch it?
            IsValidPeFile = true;
            IsDotNetFile = true;
        }
    }
}
