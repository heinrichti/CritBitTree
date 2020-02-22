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
        public bool Contains(Span<byte> bytes)
        {
            if (_rootNode == null)
                return false;

            var node = _rootNode;

            var ulen = bytes.Length;

            fixed (byte* ubytes = bytes)
            {
                while (node->Type == 1)
                {
                    ushort c = 0;
                    if (node->Byte < ulen)
                        c = ubytes[node->Byte];

                    int direction = (1 + (node->Otherbits | c)) >> 8;

                    node = direction == 0 ? node->Child1 : node->Child2;
                }

                var externalNode = (byte*) node;
                var b = externalNode + sizeof(byte);

                return bytes.SequenceEqual(new ReadOnlySpan<byte>(b + sizeof(int), *(int*)b));
            }
        }

        public bool Add(Span<byte> u)
        {
            var ubytes = u;
            var ulen = u.Length;

            var p = _rootNode;

            #region Deal with inserting into an empty tree

            if (p == null)
            {
                var rootNode = Marshal.AllocHGlobal(sizeof(int) + sizeof(byte) * (u.Length + 1));
                byte* rootNodePointer = (byte*) rootNode.ToPointer();
                rootNodePointer[0] = 0;
                *(int*)(rootNodePointer + sizeof(byte)) = u.Length;

                u.CopyTo(new Span<byte>(rootNodePointer + sizeof(byte) + sizeof(int), u.Length));

                _rootNode = (CritBitTreeNode*) rootNodePointer;
                return true;
            }

            #endregion

            byte c;
            while (p->Type == 1)
            {
                var q = p;
                c = 0;
                if (q->Byte < ulen)
                    c = ubytes[q->Byte];

                var direction = (1 + (q->Otherbits | c)) >> 8;
                p = direction == 0 ? q->Child1 : q->Child2;
            }

            var bestMember = p;

            #region Find the critical bit

            int pValueLength = *(int*) ((byte*)bestMember + sizeof(byte));
            byte* pValue = (byte*)bestMember + sizeof(byte) + sizeof(int);
            
            int newbyte;
            uint newotherbits;

            for (newbyte = 0; newbyte < ulen; newbyte++)
            {
                if (newbyte >= pValueLength)
                {
                    newotherbits = ubytes[newbyte];
                    goto different_byte_found;
                }

                if (pValue[newbyte] != ubytes[newbyte])
                {
                    newotherbits = (uint) (pValue[newbyte] ^ ubytes[newbyte]);
                    goto different_byte_found;
                }
            }

            if (pValueLength > newbyte)
            {
                newotherbits = pValue[newbyte];
                goto different_byte_found;
            }

            return false;

            different_byte_found:
            
            newotherbits |= newotherbits >> 1;
            newotherbits |= newotherbits >> 2;
            newotherbits |= newotherbits >> 4;
            newotherbits = (newotherbits & ~ (newotherbits >> 1)) ^ 255;

            c = pValueLength > newbyte ? pValue[newbyte] : (byte) 0;
            
            uint newdirection = (1 + (newotherbits | c)) >> 8;

            #endregion

            var newNodeIntPtr = Marshal.AllocHGlobal(sizeof(CritBitTreeNode));
            var newNode = (CritBitTreeNode*)newNodeIntPtr.ToPointer();
            newNode->Type = 1;
            newNode->Byte = newbyte;
            newNode->Otherbits = (byte) newotherbits;
            newNode->Child1 = null;
            newNode->Child2 = null;

            var newExternalNode = Marshal.AllocHGlobal(sizeof(int) + sizeof(byte) * (ubytes.Length + 1));
            var newExternalNodePointer = (byte*) newExternalNode.ToPointer();
            newExternalNodePointer[0] = 0;
            *(int*) (newExternalNodePointer + sizeof(byte)) = ubytes.Length;
            u.CopyTo(new Span<byte>(newExternalNodePointer + sizeof(byte) + sizeof(int), u.Length));

            if (1 - newdirection == 0)
                newNode->Child1 = (CritBitTreeNode*) newExternalNodePointer;
            else
                newNode->Child2 = (CritBitTreeNode*) newExternalNodePointer;

            var wherep = _rootNode;
            
            var isRootNode = true;

            var direction1 = 0;
            CritBitTreeNode* lastNode = null;
            while (true)
            {
                p = wherep;
                if (p->Type == 0)
                    break;
                
                var q = p;

                if (q->Byte > newbyte) break;
                if (q->Byte == newbyte && p->Otherbits > newotherbits) break;

                c = 0;
                if (q->Byte < u.Length)
                    c = ubytes[q->Byte];
                direction1 = (1 + (q->Otherbits | c)) >> 8;
                wherep = direction1 == 0 ? q->Child1 : q->Child2;
                isRootNode = false;
                lastNode = q;
            }

            if (newdirection == 0)
                newNode->Child1 = wherep;
            else
                newNode->Child2 = wherep;

            if (isRootNode)
                _rootNode = newNode;
            else
            {
                if (direction1 == 0)
                    lastNode->Child1 = newNode;
                else
                    lastNode->Child2 = newNode;
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
