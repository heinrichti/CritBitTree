using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;

namespace CritBitTree
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    internal unsafe struct CritBitTreeNode
    {
        /// <summary>
        /// 0 == External, 1 == Internal
        /// </summary>
        public byte Type;

        public CritBitTreeNode* Child1;

        public CritBitTreeNode* Child2;

        public int Byte;

        public byte Otherbits;
    }

    public unsafe class CritBitTree : IDisposable
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

                var externalNode = (byte*) node;
                var b = externalNode + sizeof(byte);

                return key.SequenceEqual(new ReadOnlySpan<byte>(b + sizeof(int), *(int*)b));
            }
        }

        public bool Add(in ReadOnlySpan<byte> key)
        {
            var node = _rootNode;
            var keyLength = key.Length;

            if (node == null)
            {
                var nodeBytes = (byte*)Marshal.AllocHGlobal(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength).ToPointer();
                nodeBytes[0] = 0;
                *(int*)(nodeBytes + sizeof(byte)) = keyLength;
                key.CopyTo(new Span<byte>(nodeBytes + sizeof(byte) + sizeof(int), keyLength));
                _rootNode = (CritBitTreeNode*)nodeBytes;
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

            int pValueLength = *(int*) ((byte*)node + sizeof(byte));
            byte* pValue = (byte*)node + sizeof(byte) + sizeof(int);
            
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

                var direction = 0;
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
                    direction = (1 + (node->Otherbits | c)) >> 8;
                    wherep = direction == 0 ? &node->Child1 : &node->Child2;
                }

                var nodePtr = Marshal.AllocHGlobal(sizeof(byte) + sizeof(int) + sizeof(byte) * keyLength);
                var nodeBytes = (byte*)nodePtr.ToPointer();
                nodeBytes[0] = 0;
                *(int*)(nodeBytes + sizeof(byte)) = keyLength;
                key.CopyTo(new Span<byte>(nodeBytes + sizeof(byte) + sizeof(int), keyLength));
                var newExternalNode = (CritBitTreeNode*)nodeBytes;

                nodePtr = Marshal.AllocHGlobal(sizeof(CritBitTreeNode));
                var newNode = (CritBitTreeNode*)nodePtr.ToPointer();
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
    }
}
