// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.PE;
using NUnit.Framework;

namespace TestCentric.Engine.Metadata
{
    public class TypeRefTableReaderTests : TableReaderTestBase<TypeRefTableReader, TypeRefRow>
    {
        protected override int ExpectedRowCount => 26;
        internal override TypeRefTableReader CreateReader(Image image) => new TypeRefTableReader(image);

        [TestCase("TestAttribute")]
        [TestCase("TestFixtureAttribute")]
        [TestCase("CategoryAttribute")]
        [TestCase("Assert")]
        public void ExpectedTypesArePresent(string expected)
        {
            Assert.That(_allRows.Select(r => r.TypeName).Contains(expected));
        }
    }
}
