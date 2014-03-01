// Copyright (c) 2014, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BloomBurger.Hashes;

namespace BloomBurger
{
    public unsafe class BloomFilter
    {
        private readonly IHasher[] _hashes;
        private readonly Int32* _storage;
        private readonly long _storageSize;

        public int HashedItems
        {
            get { return _storage[_storageSize]; }
        }

        public double ProbabilityOfFalsePositive
        {
            get
            {
                return Math.Pow(
                    (1 - Math.Exp((double) (-_hashes.Length)*HashedItems/(_storageSize << 5)))
                    , _hashes.Length);
            }
        }


        public BloomFilter(IntPtr storage, long storageSize, IEnumerable<IHasher> hashes)
        {
            _storageSize = storageSize - 1;
            _hashes = hashes.ToArray();
            _storage = (Int32*) storage.ToPointer();
        }

        public static BloomFilter FromManagedArray(int size, IEnumerable<IHasher> hashes)
        {
            var memory = new Int32[size];
            var handle = GCHandle.Alloc(memory, GCHandleType.Pinned);
            return new BloomFilter(handle.AddrOfPinnedObject(), size, hashes);
        }

        public void Add(byte [] data)
        {
            for(int i=0;i<_hashes.Length;i++)
            {
                var hash = _hashes[i].Hash(data);
                var loc = hash % _storageSize;
                var mask = 1 << (Int32)loc % 32;
                _storage[loc] |= mask;
            }
            Interlocked.Increment(ref _storage[_storageSize]);
        }

        public bool Contains(byte [] data)
        {
            for(int i=0;i<_hashes.Length;i++)
            {
                var hash = _hashes[i].Hash(data);
                var loc = hash % _storageSize;
                var mask = 1 << (Int32) loc%32;
                if ((_storage[loc] & mask) == 0)
                    return false;
            }
            return true;
        }
    }
}
