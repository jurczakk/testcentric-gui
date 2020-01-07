// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

using CodedRID = System.UInt32;
using BlobIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct CustomAttributeRow
    {
        public CodedRID Parent;
        public CodedRID Type;
        public BlobIndex Value;

        private static string[] ParentType = new string[]
        {
            "MethodDef", "FieldDef", "TypeRef", "TypeDef", "ParamDef", "InterfaceImpl", "MemberRef",
            "Module", "Permission", "Property", "Event", "StandAloneSig", "ModuleRef", "TypeSpec",
            "Assembly", "AssemblyRef", "File", "ExportedType", "ManifestResource"
        };

        public override string ToString()
        {
            var parent = $"{ParentType[Parent&31]}[{Parent>>5}]";
            var type = $"({Type>>3},{Type&7})";
            return $"CustomAttribute: Parent={parent}, Type={type}, Value={Value}";
        }
    }

    public class CustomAttributeTableReader : TableReader<CustomAttributeRow>
    {
        internal CustomAttributeTableReader(Image image) : base(image, Table.CustomAttribute) { }

        public override CustomAttributeRow GetRow()
        {
            return new CustomAttributeRow()
            {
                Parent = GetCodedIndex(CodedIndex.HasCustomAttribute),
                Type = GetCodedIndex(CodedIndex.TypeDefOrRef),
                Value = GetBlobIndex()
            };
        }
    }
}
