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

namespace TestCentric.Engine.Metadata

{
    /// <summary>
    /// AssemblyInspector knows how to find various things in an assembly header
    /// </summary>
    public class AssemblyView : IDisposable
    {
        private CustomAttributeBuilder _customAttributeBuilder;

        public static AssemblyView ReadAssembly(string assemblyPath)
        {
            return new AssemblyView(assemblyPath);
        }

        private AssemblyView(string assemblyPath)
        {
            AssemblyPath = assemblyPath;

            var fs = new FileStream(AssemblyPath, FileMode.Open, FileAccess.Read);
            Image = ImageReader.ReadImage(new Mono.Disposable<Stream>(fs, true), assemblyPath);

            // Mono.Cecil throws an exception if either of these would be false
            // TODO: Should we catch it?
            IsValidPeFile = true;
            IsDotNetFile = true;

            _customAttributeBuilder = new CustomAttributeBuilder(Image);
        }

        #region General Properties

        public string AssemblyPath { get; private set; }

        public bool IsValidPeFile { get; private set; }
        public bool IsDotNetFile { get; private set; }

        public bool ShouldRun32Bit
        {
            get
            {
                const ModuleAttributes nativeEntryPoint = (ModuleAttributes)16;
                const ModuleAttributes mask = ModuleAttributes.Required32Bit | nativeEntryPoint;

                return Architecture != TargetArchitecture.AMD64 &&
                       Architecture != TargetArchitecture.IA64 &&
                       (Image.Attributes & mask) != 0;
            }
        }

        internal Image Image { get; }

        public string ImageRuntimeVersion => Image.RuntimeVersion;

        public TargetArchitecture Architecture => Image.Architecture;

        #endregion

        #region Assembly References

        private List<AssemblyName> _assemblyReferences;
        public IEnumerable<AssemblyName> AssemblyReferences
        {
            get
            {
                return _assemblyReferences ?? (_assemblyReferences = GetAssemblyReferences());
            }
        }

        private List<AssemblyName> GetAssemblyReferences()
        {
            var rows = new AssemblyRefTableReader(Image).GetRows();
            var refs = new List<AssemblyName>();

            foreach (var row in rows)
            {
                var assemblyName = new AssemblyName()
                {
                    Name = row.Name,
                    Version = row.Version,
                    Flags = (AssemblyNameFlags)row.Attributes
                };

#if !NETSTANDARD1_6
                var keyOrToken = Image.BlobHeap.Read(row.PublicKeyOrToken);
                if (keyOrToken.Length == 8)
                    assemblyName.SetPublicKeyToken(keyOrToken);
                else // Assume full key
                    assemblyName.SetPublicKey(keyOrToken);
                assemblyName.CultureInfo = new CultureInfo(row.Culture);
#endif
                refs.Add(assemblyName);
            }

            return refs;
        }

        #endregion

        #region Custom Attributes

        private IEnumerable<CustomAttribute> _customAttributes;
        public IEnumerable<CustomAttribute> CustomAttributes
        {
            get
            {
                return _customAttributes ?? (_customAttributes = GetCustomAttributes());
            }
        }

        public string TargetFramework
        {
            get
            {
                var attr = GetCustomAttribute("System.Runtime.Versioning.TargetFrameworkAttribute");

                if (attr != null)
                    return (string)attr.Arguments[0];

                if (ImageRuntimeVersion[0] == 'v')
                {
                    var version = new Version(ImageRuntimeVersion.Substring(1));
                    switch (version.Major)
                    {
                        case 4:
                            return ".NETFramework,Version=v4.0";
                        case 2:
                            return ".NETFramework,Version=v2.0";
                    }
                }

                return null;
            }
        }

        public bool HasCustomAttribute(string fullName)
        {
            foreach (var attr in CustomAttributes)
                if (attr.FullName == fullName)
                    return true;
            
            return false;
        }

        public CustomAttribute GetCustomAttribute(string fullName)
        {
            foreach (var attr in CustomAttributes)
                if (attr.FullName == fullName)
                    return attr;

            return null;
        }

        private List<CustomAttributeRow> _customAttributeRows;
        private IEnumerable<CustomAttribute> GetCustomAttributes()
        {
            if (_customAttributeRows == null)
                _customAttributeRows = new List<CustomAttributeRow>(new CustomAttributeTableReader(Image).GetRows());

            foreach (var customAttributeRow in _customAttributeRows)
            {
                if ((customAttributeRow.Parent & 31) != 14)
                    continue;

                var attr = _customAttributeBuilder.BuildCustomAttribute(customAttributeRow);
                if (attr != null)
                    yield return attr;
            }
        }

        #endregion

        public void Dispose()
        {
            if (Image != null)
                Image.Dispose();
        }

        internal bool HasTable(Table table)
        {
            return Image.TableHeap.HasTable(table);
        }
    }
}
