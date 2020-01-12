// ***********************************************************************
// Copyright (c) Charlie Poole and TestCentric Engine contributors.
// Licensed under the MIT License. See LICENSE.txt in root directory.
// ***********************************************************************

using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.PE;
using Mono.Cecil.Metadata;

namespace TestCentric.Engine.Metadata
{
    public class CustomAttribute
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public object[] Arguments { get; set; }

        public string FullName { get { return $"{Namespace}.{Name}"; } }
    }

    internal class CustomAttributeBuilder
    {
        private BlobHeap _blobHeap;
        private TypeRefTableReader _typeRefs;
        private TypeDefTableReader _typeDefs;
        private MemberRefTableReader _memberRefs;
        private MethodDefTableReader _methodDefs;

        public CustomAttributeBuilder(Image image)
        {
            _blobHeap = image.BlobHeap;
            _typeRefs = new TypeRefTableReader(image);
            _typeDefs = new TypeDefTableReader(image);
            _memberRefs = new MemberRefTableReader(image);
            _methodDefs = new MethodDefTableReader(image);
        }

        public CustomAttribute BuildCustomAttribute(CustomAttributeRow customAttributeRow)
        {
            var typeTag = customAttributeRow.Type & 7;
            var ctorIndex = customAttributeRow.Type >> 3;

            switch (typeTag)
            {
                case 3: // MemberRef
                    var memberRef = _memberRefs.GetRow(ctorIndex);

                    var classIndex = memberRef.Class >> 3; ;
                    var classTag = memberRef.Class & 7;

                    if (classTag == 1) // This is a TypeRef
                    {
                        var typeRefRow =_typeRefs.GetRow(classIndex);
                        return new CustomAttribute()
                        {
                            Name = typeRefRow.TypeName,
                            Namespace = typeRefRow.TypeNamespace,
                            Arguments = ReadConstructorArguments(customAttributeRow.Value, memberRef.Signature)
                        };

                    }

                    break;

                case 2: // MethodDef
                    var methodDef = _methodDefs.GetRow(ctorIndex);

                    var typeDefIndex = FindTypeDefIndexForMethod(ctorIndex);
                    var typeDef = _typeDefs.GetRow(typeDefIndex);

                    var signature = ReadSignatureBlob(methodDef.Signature);

                    return new CustomAttribute()
                    {
                        Name = typeDef.TypeName,
                        Namespace = typeDef.TypeNamespace,
                        Arguments = new object[0]
                        //Arguments = ReadConstructorArguments(methodDef.ParamList, methodDef.Signature)
                    };
            }

            return null;
        }

        // NOTE: This is not a general implementation of Signatures as encoded
        // in the .NET metadata. The types that are acceptable as arguments to
        // custom attribute constructors are all we care about here.
        private struct Signature
        {
            public byte Flags;
            public uint NumArgs;
            public byte ReturnType;
            public byte[] ParamTypes;

            public Signature(byte flags, uint numArgs, byte returnType, byte[] paramTypes)
            {
                Flags = flags;
                NumArgs = numArgs;
                ReturnType = returnType;
                ParamTypes = paramTypes;
            }
        }
        
        private Signature ReadSignatureBlob(uint index)
        {
            var blob = _blobHeap.Read(index);
            var rdr = new ByteBuffer(blob);

            var flags = rdr.ReadByte();
            var numArgs = rdr.ReadCompressedUInt32();
            var returnType = rdr.ReadByte();
            var paramTypes = numArgs > 0
               ? rdr.ReadBytes((int)numArgs)
               : new byte[0];

            return new Signature(flags, numArgs, returnType, paramTypes);
        }

        const UInt16 PROLOG = 0x0001;
        const byte ELEMENT_TYPE_BOOLEAN = 0x2;
        const byte ELEMENT_TYPE_CHAR = 0x3;
        const byte ELEMENT_TYPE_I1 = 0x4;
        const byte ELEMENT_TYPE_U1 = 0x5;
        const byte ELEMENT_TYPE_I2 = 0x6;
        const byte ELEMENT_TYPE_U2 = 0x7;
        const byte ELEMENT_TYPE_I4 = 0x8;
        const byte ELEMENT_TYPE_U4 = 0x9;
        const byte ELEMENT_TYPE_I8 = 0xa;
        const byte ELEMENT_TYPE_U8 = 0xb;
        const byte ELEMENT_TYPE_R4 = 0xc;
        const byte ELEMENT_TYPE_R8 = 0xd;
        const byte ELEMENT_TYPE_STRING = 0xe;

        private object[] ReadConstructorArguments(uint valueIndex, uint signatureIndex)
        {
            var signature = ReadSignatureBlob(signatureIndex);

            var blob = _blobHeap.Read(valueIndex);
            var rdr = new ByteBuffer(blob);
            var result = new object[signature.NumArgs];

            if (rdr.ReadUInt16() != PROLOG)
                throw new InvalidOperationException();

            for (int i = 0; i < signature.NumArgs; i++)
            {
                object arg = null;

                switch (signature.ParamTypes[i])
                {
                    case ELEMENT_TYPE_BOOLEAN:
                        arg = rdr.ReadByte() == 1;
                        break;
                    case ELEMENT_TYPE_CHAR:
                        arg = (char)rdr.ReadByte();
                        break;
                    case ELEMENT_TYPE_I1:
                        arg = rdr.ReadSByte();
                        break;
                    case ELEMENT_TYPE_U1:
                        arg = rdr.ReadByte();
                        break;
                    case ELEMENT_TYPE_I2:
                        arg = rdr.ReadInt16();
                        break;
                    case ELEMENT_TYPE_U2:
                        arg = rdr.ReadUInt16();
                        break;
                    case ELEMENT_TYPE_I4:
                        arg = rdr.ReadInt32();
                        break;
                    case ELEMENT_TYPE_U4:
                        arg = rdr.ReadUInt32();
                        break;
                    case ELEMENT_TYPE_I8:
                        arg = rdr.ReadInt64();
                        break;
                    case ELEMENT_TYPE_U8:
                        arg = rdr.ReadUInt64();
                        break;
                    case ELEMENT_TYPE_R4:
                        arg = rdr.ReadSingle();
                        break;
                    case ELEMENT_TYPE_R8:
                        arg = rdr.ReadDouble();
                        break;
                    case ELEMENT_TYPE_STRING:
                        var length = rdr.ReadByte();   // String length (compressed?)
                        var sb = new StringBuilder();
                        while (length-- > 0)
                            sb.Append((char)rdr.ReadByte());
                        arg = sb.ToString();
                        break;
                }

                result[i] = arg;
            }

            return result;
        }

        private UInt32 FindTypeDefIndexForMethod(uint methodIndex)
        {
            UInt32 bestRowSoFar = 0;
            UInt32 rowNumber = 0;

            foreach (var row in _typeDefs.GetRows())
            {
                ++rowNumber;

                if (row.MethodList <= methodIndex)
                    bestRowSoFar = rowNumber;

                // Short exit since indices are in order
                if (row.MethodList > methodIndex)
                    break;
            }

            return bestRowSoFar;
        }
    }
}
