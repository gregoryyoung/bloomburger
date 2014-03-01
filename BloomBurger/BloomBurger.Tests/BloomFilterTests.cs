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

using System.Text;
using BloomBurger.Hashes;
using NUnit.Framework;

namespace BloomBurger.Tests
{
    [TestFixture]
    public class BloomFilterTests
    {
        [Test]
        public void when_adding_a_value_it_contains_it()
        {
            var filter = BloomFilter.FromManagedArray(4096, new IHasher[] {new Murmur2Unsafe(), new XXHashUnsafe()});
            filter.Add(Encoding.ASCII.GetBytes("Hello There"));
            Assert.IsTrue(filter.Contains(Encoding.ASCII.GetBytes("Hello There")));
        }

        [Test]
        public void when_adding_a_value_it_does_not_contain_another()
        {
            var filter = BloomFilter.FromManagedArray(4096, new IHasher[] { new Murmur2Unsafe(), new XXHashUnsafe() });
            filter.Add(Encoding.ASCII.GetBytes("Hello There"));
            Assert.IsFalse(filter.Contains(Encoding.ASCII.GetBytes("Hi There")));
        }

        [Test]
        public void when_hashing_two_items_the_hashed_items_should_be_two()
        {
            var filter = BloomFilter.FromManagedArray(4096, new IHasher[] { new Murmur2Unsafe(), new XXHashUnsafe() });
            filter.Add(Encoding.ASCII.GetBytes("Hello There"));
            filter.Add(Encoding.ASCII.GetBytes("Hi There"));
            Assert.AreEqual(2, filter.HashedItems);
        }

        [Test]
        public void with_nothing_hashed_probability_of_false_positive_is_zero()
        {
            var filter = BloomFilter.FromManagedArray(4096, new IHasher[] { new Murmur2Unsafe(), new XXHashUnsafe() });
            Assert.AreEqual(0, filter.ProbabilityOfFalsePositive);
        }

        [Test]
        public void with_something_hashed_probability_is_calculated_reasonably()
        {
            var filter = BloomFilter.FromManagedArray(1024, new IHasher[] { new Murmur2Unsafe(), new XXHashUnsafe() });
            filter.Add(Encoding.ASCII.GetBytes("Hi There"));
            Assert.IsTrue(filter.ProbabilityOfFalsePositive < 0.000001);
        }
    }
}
