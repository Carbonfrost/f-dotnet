//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using System;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class NamespaceNameTests {

        [Theory]
        [InlineData("IsolatedStorage")]
        [InlineData("IO.IsolatedStorage")]
        [InlineData("System.IO.IsolatedStorage")]
        public void Matches_should_apply_to_more_general_namespaces(string name) {
            var ns = NamespaceName.Parse("System.IO.IsolatedStorage");
            Assert.True(NamespaceName.Parse(name).Matches(ns));
        }

        [Theory]
        [InlineData("X.Code")]
        [InlineData("Carbonfrost.Commons")]
        [InlineData("ommons.Code")]
        public void Matches_mismatches(string name) {
            var ns = NamespaceName.Parse("Carbonfrost.Commons.Code");
            Assert.False(NamespaceName.Parse(name).Matches(ns));
        }

        [Fact]
        public void Parse_nominal() {
            var name = NamespaceName.Parse("Carbonfrost.Commons.Code");
            Assert.Equal("Code", name.Name);
            Assert.Equal("Carbonfrost.Commons.Code", name.FullName);
        }
    }
}