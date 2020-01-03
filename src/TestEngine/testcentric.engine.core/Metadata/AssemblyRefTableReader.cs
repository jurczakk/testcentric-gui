// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Globalization;
using System.Reflection;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

namespace TestCentric.Engine.Metadata
{
    internal class AssemblyRefTableReader : TableReader<AssemblyName>
    {
        public AssemblyRefTableReader(Image image) : base(image, Table.AssemblyRef) { }

        public override AssemblyName GetRow()
        {
            var version = new Version(ReadUInt16(), ReadUInt16(), ReadUInt16(), ReadUInt16());
            var flags = ReadUInt32();
            var keyidx = GetBlobIndex();
            uint nameidx = GetStringIndex();
            var cultureidx = GetStringIndex();
            var hashidx = GetBlobIndex();

            var assemblyName = new AssemblyName();
            assemblyName.Name = _image.StringHeap.Read(nameidx);
            assemblyName.Version = version;
            assemblyName.Flags = (AssemblyNameFlags)flags;

#if !NETSTANDARD1_6
            var keyOrToken = _image.BlobHeap.Read(keyidx);
            if (keyOrToken.Length == 8)
                assemblyName.SetPublicKeyToken(keyOrToken);
            else // Assume full key
                assemblyName.SetPublicKey(keyOrToken);
            assemblyName.CultureInfo = new CultureInfo(_image.StringHeap.Read(cultureidx));
#endif

            return assemblyName;
        }
    }
}
