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
    public class TypeDefTableReaderTests : TableReaderTestBase<TypeDefTableReader, TypeDefRow>
    {
        protected override int ExpectedRowCount => 12;
        internal override TypeDefTableReader CreateReader(Image image) => new TypeDefTableReader(image);

        [TestCase("MockTestFixture")]
        [TestCase("MockAssembly")]
        [TestCase("BadFixture")]
        [TestCase("GenericFixture`1")]
        public void ExpectedTypesArePresent(string expected)
        {
            Assert.That(_allRows.Select(r => r.TypeName).Contains(expected));
        }

        [Test]
        public void TypeNameSanityCheck()
        {
            foreach (var row in _allRows)
                CheckTypeName(row.FullName);
        }

        [Test]
        public void FieldListSanityCheck()
        {
            var items = _allRows.Select(r => r.FieldList);

            Assert.That(items, Is.Ordered.Ascending);
        }

        [Test]
        public void MethodListSanityCheck()
        {
            var items = _allRows.Select(r => r.MethodList);

            Assert.That(items, Is.Ordered.Ascending);
        }
    }
}
