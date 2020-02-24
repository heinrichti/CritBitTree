using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace CritBitTree
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct CritBitTreeNode
    {
        /// <summary>
        /// 0 == External, 1 == Internal
        /// </summary>
        [FieldOffset(0)]
        public byte Type;

        [FieldOffset(1)]
        public CritBitTreeNode* Child1;

        [FieldOffset(9)]
        public CritBitTreeNode* Child2;

        [FieldOffset(17)]
        public int Byte;

        [FieldOffset(21)]
        public byte Otherbits;

        [FieldOffset(1)]
        public int KeyLength;

        [FieldOffset(5)]
        public byte Key;
    }

    public unsafe class CritBitTree : IEnumerable<byte[]>, IDisposable
    {
        private CritBitTreeNode* _rootNode;

        [Pure]
        public bool Contains(in ReadOnlySpan<byte> key)
        {
            if (_rootNode == null)
                return false;

            var node = _rootNode;
            var keyLength = key.Length;

            fixed (byte* ubytes = key)
            {
                while (node->Type == 1)
                {
                    ushort c = 0;
                    if (node->Byte < keyLength)
                        c = ubytes[node->Byte];

                    int direction = (1 + (node->Otherbits | c)) >> 8;

                    node = direction == 0 ? node->Child1 : node->Child2;
                }

                return key.SequenceEqual(new ReadOnlySpan<byte>(&node->Key, node->KeyLength));
            }
        }

        public bool Add(in ReadOnlySpan<byte> key)
        {
            var node = _rootNode;
            var keyLength = key.Length;

            if (node == null)
            {
                var rootNode = (CritBitTreeNode*)Marshal.AllocHGlobal(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength).ToPointer();
                rootNode->Type = 0;
                rootNode->KeyLength = keyLength;
                key.CopyTo(new Span<byte>(&rootNode->Key, keyLength));
                _rootNode = rootNode;
                return true;
            }
            
            byte c;
            while (node->Type == 1)
            {
                c = 0;
                if (node->Byte < keyLength)
                    c = key[node->Byte];

                var direction = (1 + (node->Otherbits | c)) >> 8;
                node = direction == 0 ? node->Child1 : node->Child2;
            }

#region Find the critical bit

            int pValueLength = node->KeyLength;
            byte* pValue = &node->Key;
            
            int newbyte;
            uint newotherbits = 0;
            bool differentByteFound = false;

            for (newbyte = 0; newbyte < keyLength; newbyte++)
            {
                if (newbyte >= pValueLength)
                {
                    newotherbits = key[newbyte];
                    differentByteFound = true;
                    break;
                }

                if (pValue[newbyte] != key[newbyte])
                {
                    newotherbits = (uint) (pValue[newbyte] ^ key[newbyte]);
                    differentByteFound = true;
                    break;
                }
            }

            if (!differentByteFound)
                return false;

            newotherbits |= newotherbits >> 1;
            newotherbits |= newotherbits >> 2;
            newotherbits |= newotherbits >> 4;
            newotherbits = (newotherbits & ~ (newotherbits >> 1)) ^ 255;

            c = pValueLength > newbyte ? pValue[newbyte] : (byte) 0;
            
            uint newdirection = (1 + (newotherbits | c)) >> 8;

#endregion

            fixed (CritBitTreeNode** rootNodeFixed = &_rootNode)
            { 
                var wherep = rootNodeFixed;
                while (true)
                {
                    node = *wherep;
                    if (node->Type == 0)
                        break;
                
                    if (node->Byte > newbyte) break;
                    if (node->Byte == newbyte && node->Otherbits > newotherbits) break;

                    c = 0;
                    if (node->Byte < keyLength)
                        c = key[node->Byte];
                    var direction = (1 + (node->Otherbits | c)) >> 8;
                    wherep = direction == 0 ? &node->Child1 : &node->Child2;
                }

                var newExternalNode = (CritBitTreeNode*) Marshal.AllocHGlobal(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength).ToPointer();
                newExternalNode->Type = 0;
                newExternalNode->KeyLength = keyLength;
                key.CopyTo(new Span<byte>(&newExternalNode->Key, keyLength));

                var newNode = (CritBitTreeNode*) Marshal.AllocHGlobal(sizeof(CritBitTreeNode)).ToPointer();
                newNode->Type = 1;
                if (newdirection == 0)
                { 
                    newNode->Child1 = *wherep;
                    newNode->Child2 = newExternalNode;
                }
                else
                {
                    newNode->Child1 = newExternalNode;
                    newNode->Child2 = *wherep;
                }
                newNode->Byte = newbyte;
                newNode->Otherbits = (byte)newotherbits;

                *wherep = newNode;
            }

            return true;
        }

        public void Dispose()
        {
            DisposeManaged();
            GC.SuppressFinalize(this);
        }

        public void DisposeManaged()
        {
            var node = _rootNode;
            Dispose(node);
        }

        ~CritBitTree()
        {
            DisposeManaged();
        }

        private void Dispose(CritBitTreeNode* node)
        {
            if (node->Type == 1)
            {
                Dispose(node->Child1);
                Dispose(node->Child2);
            }

            Marshal.FreeHGlobal(new IntPtr(node));
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return new CritBitTreeNodeEnumerator(_rootNode);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
