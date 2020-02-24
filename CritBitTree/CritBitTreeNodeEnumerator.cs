using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CritBitTree
{
    internal unsafe class CritBitTreeNodeEnumerator : IEnumerator<byte[]>
    {
        private byte[][] _all;
        private int _current = -1;

        public CritBitTreeNodeEnumerator(UnmanagedCritBitTreeNode* rootNode)
        {
            _all = GetAll(rootNode);
        }

        private byte[][] GetAll(UnmanagedCritBitTreeNode* node)
        {
            if (node->Type == 0)
            {
                var bytes = new byte[node->KeyLength];
                Marshal.Copy(new IntPtr(&node->Key), bytes, 0, node->KeyLength);
                return new byte[1][] { bytes };
            }

            var children1 = GetAll(node->Child1);
            var children2 = GetAll(node->Child2);

            var result = new byte[children1.Length + children2.Length][];
            int currentPosition = 0;
            for (int i = 0; i < children1.Length; i++)
            {
                result[currentPosition++] = children1[i];
            }

            for (int i = 0; i < children2.Length; i++)
            {
                result[currentPosition++] = children2[i];
            }

            return result;
        }

        public byte[] Current => _all[_current];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++_current < _all.Length;
        }

        public void Reset()
        {
            _current = -1;
        }
    }
}
