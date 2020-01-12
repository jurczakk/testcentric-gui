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
using Mono.Cecil.PE;

namespace TestCentric.Engine.Metadata
{
    public class CustomAttributeTableReaderTests : TableReaderTestBase<CustomAttributeTableReader, CustomAttributeRow>
    {
        protected override int ExpectedRowCount => 61;
        internal override CustomAttributeTableReader CreateReader(Image image) => new CustomAttributeTableReader(image);

        //[Test]
        //public void ExpectedAttributesArePresent()
        //{

        //}

    }
}
