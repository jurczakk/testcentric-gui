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
        const int EXPECTED_ROW_COUNT = 61;

        [OneTimeSetUp]
        public void SetExpectations()
        {
            ExpectedRowCount = 61;
            foreach (var row in _allRows)
                Console.WriteLine(row.ToString());
        }

        internal override CustomAttributeTableReader CreateReader(Image image)
        {
            return new CustomAttributeTableReader(image);
        }


        //[Test]
        //public void ExpectedAttributesArePresent()
        //{

        //}

    }
}
