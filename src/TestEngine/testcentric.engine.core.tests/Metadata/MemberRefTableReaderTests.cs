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
    public class MemberRefTableReaderTests : TableReaderTestBase<MemberRefTableReader, MemberRefRow>
    {
        protected override int ExpectedRowCount => 33;
        internal override MemberRefTableReader CreateReader(Image image) => new MemberRefTableReader(image);

        [TestCase("Pass")]
        [TestCase("Fail")]
        [TestCase("Inconclusive")]
        public void ExpectedMethodsArePresent(string expected)
        {
            Assert.That(_allRows.Select(r => r.Name).Contains(expected));
        }
    }
}
