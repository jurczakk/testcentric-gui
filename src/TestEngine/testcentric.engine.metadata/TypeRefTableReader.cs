// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

using CodedRID = System.UInt32;
using StringIndex = System.UInt32;

namespace TestCentric.Engine.Metadata
{
    public struct TypeRefRow
    {
        public CodedRID ResolutionScope;
        public string TypeName;
        public string TypeNamespace;

        public override string ToString()
        {
            return $"TypeRef: {TypeNamespace}.{TypeName}, Scope={ResolutionScope}";
        }
    }

    public class TypeRefTableReader : TableReader<TypeRefRow>
    {
        internal TypeRefTableReader(Image image) : base(image, Table.TypeRef) { }

        public override TypeRefRow GetRow()
        {
            return new TypeRefRow()
            {
                ResolutionScope = GetCodedIndex(CodedIndex.ResolutionScope),
                TypeName = GetString(),
                TypeNamespace = GetString()
            };
        }
    }
}
