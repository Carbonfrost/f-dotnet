//
// Copyright 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class MetadataNameTests {

        [Fact]
        public void Combine_should_qualify_type_with_assembly() {
            var actual = MetadataName.Combine(
                AssemblyName.Parse("mscorlib"),
                TypeName.Parse("String")
            );
            Assert.Equal(SymbolType.Type, actual.SymbolType);
            Assert.Equal("String, mscorlib", ((TypeName) actual).AssemblyQualifiedName);
        }

        [Fact]
        public void Combine_should_qualify_type_with_declaring_type() {
            var actual = MetadataName.Combine(
                TypeName.Parse("String"),
                TypeName.Parse("Enumerator")
            );
            Assert.Equal(SymbolType.Type, actual.SymbolType);
            Assert.Equal("String+Enumerator", actual.FullName);
        }

        [Fact]
        public void Combine_should_qualify_type_with_namespace_name() {
            var actual = MetadataName.Combine(
                TypeName.Parse("String"),
                TypeName.Parse("Enumerator")
            );
            Assert.Equal(SymbolType.Type, actual.SymbolType);
            Assert.Equal("String+Enumerator", actual.FullName);
        }

        [Fact]
        public void Combine_should_qualify_list_of_names() {
            var actual = MetadataName.Combine(
                AssemblyName.Parse("mscorlib"),
                NamespaceName.Parse("System"),
                TypeName.Parse("String"),
                TypeName.Parse("Enumerator")
            );
            Assert.Equal(SymbolType.Type, actual.SymbolType);
            Assert.Equal("System.String+Enumerator, mscorlib", ((TypeName) actual).AssemblyQualifiedName);
        }

        [Fact]
        public void Combine_should_combine_nested_type_ns() {
            var actual = MetadataName.Combine(
                NamespaceName.Parse("System"),
                TypeName.Parse("String+Enumerator")
            );
            Assert.Equal(SymbolType.Type, actual.SymbolType);
            Assert.Equal("System.String+Enumerator", actual.FullName);
        }

        [Fact]
        public void Combine_should_qualify_namespace_name_with_other() {
            var actual = MetadataName.Combine(
                NamespaceName.Parse("System"),
                NamespaceName.Parse("IO")
            );
            Assert.Equal(SymbolType.Namespace, actual.SymbolType);
            Assert.Equal("System.IO", actual.FullName);
        }

        [Theory]
        [InlineData(SymbolType.Field)]
        [InlineData(SymbolType.Property)]
        [InlineData(SymbolType.Event)]
        [InlineData(SymbolType.Method)]
        public void Combine_should_qualify_member_with_type(SymbolType symbolType) {
            var actual = MetadataName.Combine(
                TypeName.Parse("String"),
                SymbolName.Parse("Length").ConvertTo(symbolType)
            );
            Assert.Equal(symbolType, actual.SymbolType);
            Assert.Equal("String.Length", actual.FullName);
        }

    }

}
