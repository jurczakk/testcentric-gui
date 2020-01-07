// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using Mono.Cecil.PE;

namespace TestCentric.Engine.Metadata
{
    public abstract class TableReaderTestBase<TReader, TRow> where TReader : TableReader<TRow>
    {
        // All builds (Net 3.5, NetCoreApp 1.1, NetCoreApp 2.2) use the same mock assembly for these tests (.NET 4.5).
        // This accomplishes two things:
        //  1. We can use the same expected values for all builds
        //  2. We are verifying the abliltiy for all builds to view assemblies using a different framework or version.
        static public readonly string MOCK_PATH = Path.Combine(TestContext.CurrentContext.TestDirectory, "../mock-assembly.dll");

        internal Image _image;
        protected TReader _reader;
        protected List<TRow> _allRows;
        protected List<TRow> _enumRows;

        // Derived classes must set this
        protected static int ExpectedRowCount;

        [OneTimeSetUp]
        public void Initialize()
        {
            _image = AssemblyView.ReadAssembly(MOCK_PATH).Image;
            _reader = CreateReader(_image);

            // NOTE: Order is important here.  The enumeration starts at current
            // position, initially tablestart, while GetRows() restarts at row 1.
            _enumRows = new List<TRow>();
            do { _enumRows.Add(_reader.GetRow()); } while (_reader.NextRow());

            _allRows = new List<TRow>(_reader.GetRows());
        }

        // Derived class overrides to supply the reader
        internal abstract TReader CreateReader(Image image);

        [Test]
        public void EnumeratedRowsAndGetRowsGiveSameResults()
        {
            Assert.That(_enumRows, Is.EqualTo(_allRows));
        }

        [Test]
        public void VerifyCounts()
        {
            Assert.Multiple((TestDelegate)(() =>
            {
                Assert.That(_allRows.Count, Is.EqualTo(ExpectedRowCount), "All Rows");
                Assert.That(_enumRows.Count, Is.EqualTo(ExpectedRowCount), "Enumerated Rows");
            }));
        }

        static IEnumerable<TestCaseData> RandomRowNumbers()
        {
                yield return new TestCaseData(TestContext.CurrentContext.Random.Next(1, ExpectedRowCount));
        }

        [Test]
        public void GetRowByIndex()
        {
            int count = Math.Max(1, Math.Min(ExpectedRowCount / 5, 5));

            for (int i = 0; i < count; i++)
            {
                int index = TestContext.CurrentContext.Random.Next(1, ExpectedRowCount);
                var row = _reader.GetRow((uint)index);
                Assert.That(row, Is.EqualTo(_allRows[index - 1]));
            }
        }

        [Test]
        public void RowIndexZeroThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _reader.GetRow(0));
        }

        [Test]
        public void RowIndexThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => _reader.GetRow((uint)_allRows.Count + 1));
        }
    }
}
