// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

internal abstract class TableReader<TRow> : ByteBuffer
{
    protected Image _image;
    private int _stridx_size;
    private int _blobidx_size;
    private int _guididx_size;

    private Table _table;
    private TableInformation _info;
    private uint _current;
    private uint _eot;

    public TableReader(Image image, Table table) : base(image.TableHeap.data)
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
        if (row >= _info.Length)
            throw new ArgumentException("Invalid value for row", nameof(row));

        _current = _info.Offset + _info.RowSize * row;
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

    public abstract TRow GetRow();

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

    private uint GetIndexBySize(int size)
    {
        return size == 4 ? ReadUInt32() : ReadUInt16();
    }
}
