// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil.Metadata;
using Mono.Cecil.PE;

namespace TestCentric.Engine.Metadata
{
    internal class AssemblyRefTableReaderTests : TableReaderTestBase<AssemblyRefTableReader, AssemblyRefRow>
    {
        static readonly string[] EXPECTED_REFERENCES = new[] { "mscorlib", "nunit.framework", "System" };

        protected override int ExpectedRowCount => 3;
        internal override AssemblyRefTableReader CreateReader(Image image) => new AssemblyRefTableReader(image);
    }
}
