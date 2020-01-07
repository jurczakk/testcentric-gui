// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Globalization;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

using StringIndex = System.UInt32;
using BlobIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct AssemblyRefRow
    {
        public Version Version;
        public AssemblyAttributes Attributes;
        public BlobIndex PublicKeyOrToken;
        public String Name;
        public String Culture;
        public BlobIndex HashValue;
    }

    public class AssemblyRefTableReader : TableReader<AssemblyRefRow>
    {
        internal AssemblyRefTableReader(Image image) : base(image, Table.AssemblyRef) { }

        public override AssemblyRefRow GetRow()
        {
            return new AssemblyRefRow()
            {
                Version = new Version(ReadUInt16(), ReadUInt16(), ReadUInt16(), ReadUInt16()),
                Attributes = (AssemblyAttributes)ReadUInt32(),
                PublicKeyOrToken = GetBlobIndex(),
                Name = GetString(),
                Culture = GetString(),
                HashValue = GetBlobIndex()
            };
        }
    }
}
