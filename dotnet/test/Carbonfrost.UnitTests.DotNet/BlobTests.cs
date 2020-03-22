//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using Carbonfrost.Commons.DotNet;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.DotNet {

    public class BlobTests : TestClass {

        [Fact]
        public void ParseExact_should_fail_when_other_format() {
            Assert.DoesNotThrow(() => Blob.Parse("ECBAgKA="));
            Assert.Throws<ArgumentException>(() => Blob.ParseExact("ECBAgKA=", "X"));
            Assert.Throws<ArgumentException>(() => Blob.ParseExact("ECBAgKA=", "x"));
        }

        [Theory]
        [InlineData("z", "ECBA gKA=", Name = "whitespace in z")]
        [InlineData("x", "ECBA", Name = "wrong case in x")]
        [InlineData("X", "ecba", Name = "wrong case in X")]
        public void ParseExact_should_fail_when_not_exact(string format, string text) {
            Assert.Throws<ArgumentException>(() => Blob.ParseExact(text, format));
        }
    }
}
