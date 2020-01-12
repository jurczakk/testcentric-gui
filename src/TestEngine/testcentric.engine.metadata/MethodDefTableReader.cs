// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

using CodedRID = System.UInt32;
using StringIndex = System.UInt32;
using BlobIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct MethodDefRow
    {
        private static readonly string[] TAGS = { "TypeDef", "TypeRef", "ModuleRef", "ModuleDef", "TypeSpec" };

        public UInt32 RVA;
        public UInt16 ImplFlags;
        public UInt16 Flags;
        public string Name;
        public BlobIndex Signature;
        public UInt32 ParamList;

        public override string ToString()
        {
            return $"MethodDef: {Name}";
        }
    }

    public class MethodDefTableReader : TableReader<MethodDefRow>
    {
        internal MethodDefTableReader(Image image) : base(image, Table.Method) { }

        public override MethodDefRow GetRow()
        {
            return new MethodDefRow()
            {
                RVA = ReadUInt32(),
                ImplFlags = ReadUInt16(),
                Flags = ReadUInt16(),
                Name = GetString(),
                Signature = GetBlobIndex(),
                ParamList = GetTableIndex()
            };
        }
    }
}
