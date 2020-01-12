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
    public class MethodDefTableReaderTests : TableReaderTestBase<MethodDefTableReader, MethodDefRow>
    {
        protected override int ExpectedRowCount => 47;
        internal override MethodDefTableReader CreateReader(Image image) => new MethodDefTableReader(image);

        [TestCase(".ctor")]
        [TestCase("Test1")]
        [TestCase("FailingTest")]
        public void ExpectedMethodsArePresent(string expected)
        {
            Assert.That(_allRows.Select(r => r.Name).Contains(expected));
        }

        [Test]
        public void ParamListSanityCheck()
        {
            var items = _allRows.Select(r => r.ParamList);

            Assert.That(items, Is.Ordered.Ascending);
            //Assert.That(items, Is.All.InRange(1, _image.TableHeap.Tables[);
        }
    }
}
