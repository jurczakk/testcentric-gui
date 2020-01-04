// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

namespace NUnit.Engine.Metadata
{
    internal struct MemberRefData
    {
        uint Class;
        uint Name;
        uint Signature;

        public MemberRefData(uint @class, uint name, uint signature)
        {
            Class = @class;
            Name = name;
            Signature = signature;
        }
    }

    internal class MemberRefTableReader : TableReader<MemberRefData>
    {
        public MemberRefTableReader(Image image) : base(image, Table.MemberRef) { }

        public override MemberRefData GetRow()
        {
            return new MemberRefData(GetCodedIndex(CodedIndex.MemberRefParent), GetStringIndex(), GetBlobIndex());
        }
    }
}
