// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;
using System.Diagnostics;
using System.IO;
using Mono.Cecil;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;
using TestCentric.Engine.Helpers;

namespace TestCentric.Engine.Metadata

{
    /// <summary>
    /// AssemblyInspector knows how to find various things in an assembly header
    /// </summary>
    public class AssemblyView : IDisposable
    {
        private Image _image;

        public static AssemblyView ReadAssembly(string assemblyPath)
        {
            return new AssemblyView(assemblyPath);
        }

        public static AssemblyView ReadAssembly(Assembly assembly)
        {
            return new AssemblyView(AssemblyHelper.GetAssemblyPath(assembly));
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

        public AssemblyName[] AssemblyReferences
        {
            get
            {
                var result = new List<AssemblyName>();
                var rdr = new AssemblyRefTableReader(_image);

                do
                {
                    result.Add(rdr.GetRow());
                } while (rdr.NextRow());

                return result.ToArray();
            }
        }

        public CustomAttributeData[] CustomAttributes
        {
            get
            {
                var result = new List<CustomAttributeData>();
                var rdr = new CustomAttributeTableReader(_image);

                do
                {
                    var info = rdr.GetRow();
                    // Only return attributes on the assembly itself
                    if ((info.Parent & 31) == 14)
                    {
                        result.Add(info);
                    }
                } while (rdr.NextRow());

                return result.ToArray();
            }
        }

        public void Dispose()
        {
            if (_image != null)
                _image.Dispose();
        }

        private AssemblyView(string assemblyPath)
        {
            AssemblyPath = assemblyPath;

            var fs = new FileStream(AssemblyPath, FileMode.Open, FileAccess.Read);
            _image = ImageReader.ReadImage(new Mono.Disposable<Stream>(fs, true), assemblyPath);

            // Mono.Cecil throws an exception if either of these would be false
            // TODO: Should we catch it?
            IsValidPeFile = true;
            IsDotNetFile = true;
        }

        internal bool HasTable(Table table)
        {
            return _image.TableHeap.HasTable(table);
        }

        private TableInformation GetTableInformation(Table table)
        {
            return _image.TableHeap.Tables[(int)table];
        }

    }
}
