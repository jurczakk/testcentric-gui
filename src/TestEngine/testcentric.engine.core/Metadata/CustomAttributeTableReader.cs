// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

namespace TestCentric.Engine.Metadata
{
    public struct CustomAttributeData
    {
        public UInt32 Parent;
        public UInt32 Type;
        public UInt32 Value;

        public CustomAttributeData(UInt32 parent, UInt32 type, UInt32 value)
        {
            Parent = parent;
            Type = type;
            Value = value;
        }
    }

    internal class CustomAttributeTableReader : TableReader<CustomAttributeData>
    {
        public CustomAttributeTableReader(Image image) : base(image, Table.CustomAttribute) { }

        public override CustomAttributeData GetRow()
        {
            var parent = GetCodedIndex(CodedIndex.HasCustomAttribute);
            var type = GetCodedIndex(CodedIndex.TypeDefOrRef);
            var value = GetBlobIndex();

            var tag = type & 7;
            var index = type >> 3;
            var table = tag == 2 ? Table.TypeRef : Table.TypeRef;

            return new CustomAttributeData(parent, type, value);
        }
    }
}
