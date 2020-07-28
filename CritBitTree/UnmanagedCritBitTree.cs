using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CritBitTree
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    internal unsafe struct UnmanagedCritBitTreeNode
    {
        /// <summary>
        /// 0 == External, 1 == Internal
        /// </summary>
        [FieldOffset(0)]
        public byte Type;

        [FieldOffset(1)]
        public UnmanagedCritBitTreeNode* Child1;

        [FieldOffset(9)]
        public UnmanagedCritBitTreeNode* Child2;

        [FieldOffset(17)]
        public int Byte;

        [FieldOffset(21)]
        public byte Otherbits;

        [FieldOffset(1)]
        public int KeyLength;

        [FieldOffset(5)]
        public byte Key;
    }

    public unsafe class UnmanagedCritBitTree : IEnumerable<byte[]>, IDisposable
    {
        private readonly UnmanagedCritBitTreeNode** _rootNode;

        private readonly UnmanagedMemoryPool _memoryPool;

        public UnmanagedCritBitTree(int pageSize = 1024)
        {
            _memoryPool = new UnmanagedMemoryPool(pageSize);
            _rootNode = (UnmanagedCritBitTreeNode**) Marshal.AllocHGlobal(sizeof(UnmanagedCritBitTreeNode*));
            *_rootNode = null;
        }

        [Pure]
        public bool Contains(in ReadOnlySpan<byte> key)
        {
            if (*_rootNode == null)
                return false;

            var node = *_rootNode;
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

        public bool Add(in ReadOnlySpan<byte> key2)
        {
            fixed (byte* key = key2)
            {
                var node = *_rootNode;
                var keyLength = key2.Length;

                if (node == null)
                {
                    var memory = _memoryPool.Rent(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength);
                    var rootNode = (UnmanagedCritBitTreeNode*)memory;
                    
                    rootNode->Type = 0;
                    rootNode->KeyLength = keyLength;

                    Unsafe.CopyBlock(&rootNode->Key, key, (uint)keyLength);

                    *_rootNode = rootNode;
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

                var wherep = _rootNode;
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

                var newExternalNode = (UnmanagedCritBitTreeNode*)_memoryPool.Rent(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength);
                newExternalNode->Type = 0;
                newExternalNode->KeyLength = keyLength;
                Unsafe.CopyBlock(&newExternalNode->Key, key, (uint)keyLength);

                var newNode = (UnmanagedCritBitTreeNode*)_memoryPool.Rent(sizeof(UnmanagedCritBitTreeNode));
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
                newNode->Otherbits = (byte) newotherbits;

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
            _memoryPool.Dispose();
            Marshal.FreeHGlobal(new IntPtr(_rootNode));
        }

        ~UnmanagedCritBitTree()
        {
            DisposeManaged();
        }

        public IEnumerator<byte[]> GetEnumerator()
        {
            return new CritBitTreeNodeEnumerator(*_rootNode);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
