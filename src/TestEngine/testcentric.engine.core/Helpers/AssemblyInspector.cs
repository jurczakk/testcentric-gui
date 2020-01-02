// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Globalization;
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

        public AssemblyName[] AssemblyReferences
        {
            get
            {
                var tableInfo = GetTableInformation(Table.AssemblyRef);

                var result = new List<AssemblyName>();

                var data = _image.TableHeap.data;
                var rdr = new ByteBuffer(data);
                var offset = tableInfo.Offset;
                rdr.Advance((int)offset);

                for (int i = 0; i < tableInfo.Length; i++)
                    result.Add(GetAssemblyReference(rdr));

                offset += tableInfo.RowSize;

                return result.ToArray();
            }
        }

        private AssemblyName GetAssemblyReference(ByteBuffer rdr)
        {
            var version = new Version(rdr.ReadUInt16(), rdr.ReadUInt16(), rdr.ReadUInt16(), rdr.ReadUInt16());
            var flags = rdr.ReadUInt32();
            var key_idx = rdr.ReadUInt16();
            var name_idx = rdr.ReadUInt16();
            var culture_idx = rdr.ReadUInt16();
            var hash_idx = rdr.ReadUInt16();

            //_image.StringHeap.data
            return new AssemblyName()
            {
                Name = _image.StringHeap.Read(name_idx),
                Version = version,
                Flags = (AssemblyNameFlags)flags,
#if !NETSTANDARD1_6
                KeyPair = new StrongNameKeyPair(_image.BlobHeap.Read(key_idx)),
                CultureInfo = new CultureInfo(_image.StringHeap.Read(culture_idx)),
#endif
            };
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
