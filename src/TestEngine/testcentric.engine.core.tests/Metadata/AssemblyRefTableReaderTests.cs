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

        [OneTimeSetUp]
        public void SetExpectations()
        {
            ExpectedRowCount = 3;
            foreach (var row in _allRows)
                Console.WriteLine(row.ToString());
        }

        internal override AssemblyRefTableReader CreateReader(Image image)
        {
            return new AssemblyRefTableReader(image);
        }
    }
}
