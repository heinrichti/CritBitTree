using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace CritBitTree
{
    internal unsafe class UnmanagedMemoryPool : IDisposable
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct FreeInfo
        {
            public int Bytes;
        }

        private readonly int _pageSize;
        private FreeInfo* _nextFree;
        private readonly List<IntPtr> _pages = new List<IntPtr>();

        public UnmanagedMemoryPool(int pageSize)
        {
            _pageSize = pageSize;
            var page = Marshal.AllocHGlobal(pageSize);
            _pages.Add(page);
            _nextFree = (FreeInfo*) page.ToPointer();
            _nextFree->Bytes = pageSize;
        }

        public void* Rent(in int bytes)
        {
            var requiredBytes = bytes + sizeof(FreeInfo);
            if (requiredBytes > _pageSize)
                throw new ArgumentException("PageSize of Pool too small");

            var freeBlock = _nextFree;
            if (requiredBytes >= freeBlock->Bytes)
            {
                var page = Marshal.AllocHGlobal(_pageSize);
                _pages.Add(page);
                freeBlock = (FreeInfo*) page.ToPointer();
                freeBlock->Bytes = _pageSize;
            }

            var freeBytes = freeBlock->Bytes - bytes;
            _nextFree = (FreeInfo*) ((byte*) freeBlock + bytes);
            _nextFree->Bytes = freeBytes;

            return freeBlock;
        }

        public void Dispose()
        {
            foreach (var page in _pages)
            {
                Marshal.FreeHGlobal(page);
            }
        }
    }

}
