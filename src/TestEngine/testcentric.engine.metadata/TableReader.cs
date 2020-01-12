// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

namespace TestCentric.Engine.Metadata
{
    public abstract class TableReader<TRow> : ByteBuffer
    {
        internal Image _image;
        private int _stridx_size;
        private int _blobidx_size;
        private int _guididx_size;

        private Table _table;
        private TableInformation _info;
        private uint _current;
        private uint _eot;

        internal TableReader(Image image, Table table) : base(image.TableHeap.data)
        {
            _image = image;
            _table = table;

            _stridx_size = _image.StringHeap.IndexSize;
            _guididx_size = _image.GuidHeap != null ? _image.GuidHeap.IndexSize : 2;
            _blobidx_size = _image.BlobHeap != null ? _image.BlobHeap.IndexSize : 2;

            _info = _image.TableHeap.Tables[(int)table];
            _current = _info.Offset;
            _eot = _info.Offset + _info.Length * _info.RowSize;

            position = (int)_current;
        }

        public void GoToRow(uint row)
        {
            if (row < 1 || row > _info.Length)
                throw new ArgumentException("Invalid value for row", nameof(row));

            _current = _info.Offset + _info.RowSize * (row - 1);
            position = (int)_current;
        }

        public bool NextRow()
        {
            var next = _current + _info.RowSize;
            if (next >= _eot)
                return false;

            _current = next;
            position = (int)_current;

            return true;
        }

        public TRow GetRow(uint row)
        {
            GoToRow(row);
            return GetRow();
        }

        public IEnumerable<TRow> GetRows(uint start = 1)
        {
            GoToRow(start);

            do { yield return GetRow(); } while (NextRow());
        }

        public abstract TRow GetRow();

        protected string GetString()
        {
            return _image.StringHeap.Read(GetStringIndex());
        }

        protected uint GetStringIndex()
        {
            return GetIndexBySize(_stridx_size);
        }

        protected uint GetBlobIndex()
        {
            return GetIndexBySize(_blobidx_size);
        }

        protected uint GetGuidIndex()
        {
            return GetIndexBySize(_guididx_size);
        }

        protected uint GetCodedIndex(CodedIndex index)
        {
            return GetIndexBySize(_image.GetCodedIndexSize(index));
        }

        internal uint GetTableIndex()
        {
            return GetIndexBySize(_image.TableHeap.IndexSize);
        }

        private uint GetIndexBySize(int size)
        {
            return size == 4 ? ReadUInt32() : ReadUInt16();
        }
    }
}
