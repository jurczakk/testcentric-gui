// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

using CodedRID = System.UInt32;
using StringIndex = System.UInt32;
using BlobIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct MemberRefRow
    {
        private static readonly string[] TAGS = { "TypeDef", "TypeRef", "ModuleRef", "ModuleDef", "TypeSpec" };

        public CodedRID Class;
        public string Name;
        public BlobIndex Signature;

        public override string ToString()
        {
            return $"MemberRef: Class={TAGS[Class&7]}({Class>>3}) Name={Name}, Signature={Signature}";
        }
    }

    public class MemberRefTableReader : TableReader<MemberRefRow>
    {
        internal MemberRefTableReader(Image image) : base(image, Table.MemberRef) { }

        public override MemberRefRow GetRow()
        {
            return new MemberRefRow()
            {
                Class = GetCodedIndex(CodedIndex.MemberRefParent),
                Name = GetString(),
                Signature = GetBlobIndex()
            };
        }
    }
}
