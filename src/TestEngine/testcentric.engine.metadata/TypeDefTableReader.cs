// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

using CodedRID = System.UInt32;
using StringIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct TypeDefRow
    {
        public UInt32 Flags;
        public string TypeName;
        public string TypeNamespace;
        public CodedRID Extends;
        public UInt32 FieldList;
        public UInt32 MethodList;

        public string FullName => TypeNamespace + "." + TypeName;

        public override string ToString()
        {
            return $"TypeDef: {FullName}, Extends={Extends}, FieldList={FieldList}, MethodList={MethodList}";
        }
    }

    public class TypeDefTableReader : TableReader<TypeDefRow>
    {
        internal TypeDefTableReader(Image image) : base(image, Table.TypeDef) { }

        public override TypeDefRow GetRow()
        {
            return new TypeDefRow()
            {
                Flags = ReadUInt32(),
                TypeName = GetString(),
                TypeNamespace = GetString(),
                Extends = GetCodedIndex(CodedIndex.TypeDefOrRef),
                FieldList = GetTableIndex(),
                MethodList = GetTableIndex()
            };
        }
    }
}
