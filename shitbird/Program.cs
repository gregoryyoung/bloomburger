﻿// Copyright (c) 2014, Event Store LLP
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
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using BloomBurger;
using BloomBurger.Hashes;

namespace shitbird
{
    class Program
    {
        private const long KILOBYTE = 1024;
        private const long MEGABYTE = KILOBYTE*KILOBYTE;
        private const long GIGABYTE = MEGABYTE*MEGABYTE;

        static void Main(string[] args)
        {
            MemoryMappedFile();
            UnmanagedMemory();
            ManagedMemory();
        }

        private static void MemoryMappedFile()
        {
            Console.WriteLine("Memory Mapped File.");
            var filename = @"C:\curl\fofadasfho";
            var size = 600 * MEGABYTE;
            using (
                var memmap = System.IO.MemoryMappedFiles.MemoryMappedFile.CreateFromFile(filename, FileMode.OpenOrCreate, "MyMap2", size,
                                                             MemoryMappedFileAccess.ReadWrite))
            {
                using (var view = memmap.CreateViewAccessor(0, size, MemoryMappedFileAccess.ReadWrite))
                {
                    var watch = new Stopwatch();
                    watch.Start();
                    var filter = new BloomFilter(view.SafeMemoryMappedViewHandle.DangerousGetHandle(), size/4,
                                                 new IHasher[] {new Murmur3AUnsafe(), new XXHashUnsafe()});
                    for (int i = 0; i < 1000000; i++)
                    {
                        var bytes = Guid.NewGuid().ToByteArray();
                        filter.Add(bytes);
                        if (!filter.Contains(bytes))
                        {
                            throw new Exception("broken");
                        }
                        if (i%100000 == 0)
                        {
                            Console.Write(".");
                        }
                    }
                    Console.WriteLine();
                    Console.WriteLine(watch.Elapsed);
                }
            }
            File.Delete(filename);
        }

        private unsafe static void UnmanagedMemory()
        {
            Console.WriteLine("Unmanaged Memory");
            const int size = (int) (500*MEGABYTE);

            var watch = new Stopwatch();
            watch.Start();
            var ptr = Marshal.AllocHGlobal(size);
            var filter = new BloomFilter(ptr, size/4,
                                         new IHasher[] {new Murmur3AUnsafe(), new XXHashUnsafe()});
            for (int i = 0; i < 1000000; i++)
            {
                var bytes = Guid.NewGuid().ToByteArray();
                filter.Add(bytes);
                if (!filter.Contains(bytes))
                {
                    throw new Exception("broken");
                }
                if (i%100000 == 0)
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine();
            Console.WriteLine(watch.Elapsed);
            Marshal.FreeHGlobal(ptr);
        }

        private unsafe static void ManagedMemory()
        {
            Console.WriteLine("Managed Memory");
            const int size = (int)(500 * MEGABYTE);

            var watch = new Stopwatch();
            watch.Start();
            var filter = BloomFilter.FromManagedArray(size, new IHasher[] { new Murmur3AUnsafe(), new XXHashUnsafe() });
            for (int i = 0; i < 1000000; i++)
            {
                var bytes = Guid.NewGuid().ToByteArray();
                filter.Add(bytes);
                if (!filter.Contains(bytes))
                {
                    throw new Exception("broken");
                }
                if (i % 100000 == 0)
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine();
            Console.WriteLine(watch.Elapsed);
        }

    }
}
