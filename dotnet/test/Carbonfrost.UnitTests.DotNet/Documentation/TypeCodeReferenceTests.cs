//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using Carbonfrost.Commons.DotNet;
using Carbonfrost.Commons.DotNet.Documentation;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.DotNet.Documentation {

    public class TypeCodeReferenceTests {

        // TODO Nested types are usually ambiguous as metadata names
        // "T:System.ComponentModel.TypeConverter.StandardValuesCollection"

        [Fact]
        public void parse_type_nominal() {
            var cr = CodeReference.Parse("T:System.Runtime.Serialization.Json.JsonValueExtensions");
            Assert.Equal(SymbolType.Type, cr.MetadataName.SymbolType);

            var name = (TypeName) cr.MetadataName;
            Assert.True(cr.IsValid);
            Assert.Equal("System.Runtime.Serialization.Json.JsonValueExtensions", name.FullName);
            Assert.Equal("System.Runtime.Serialization.Json", name.Namespace);
        }

        [Fact]
        public void parse_type_generic_nested_type() {
            string text = "T:System.Collections.Generic.Dictionary`2.Enumerator";
            var cr = CodeReference.Parse(text);

            var name = (TypeName) cr.MetadataName;
            Assert.True(cr.IsValid);
            Assert.Equal("System.Collections.Generic.Dictionary`2+Enumerator", name.FullName);
            Assert.Equal("System.Collections.Generic", name.Namespace);

            Assert.Equal(text, cr.ToString());
        }

        [Fact]
        public void parse_type_simple() {
            var cr = CodeReference.Parse("T:Object");
            Assert.Equal(SymbolType.Type, cr.MetadataName.SymbolType);

            var name = (TypeName) cr.MetadataName;
            Assert.True(cr.IsValid);
            Assert.Equal("Object", name.FullName);
            Assert.Equal("", name.Namespace);
        }

        [Fact]
        public void parse_type_generic() {
            string text = "T:System.Web.Mvc.ViewUserControl`1";
            var cr = CodeReference.Parse(text);
            Assert.Equal(SymbolType.Type, cr.MetadataName.SymbolType);

            var name = (TypeName) cr.MetadataName;
            Assert.True(cr.IsValid);
            Assert.Equal("System.Web.Mvc.ViewUserControl`1", name.FullName);
            Assert.True(name.IsGenericTypeDefinition);

            Assert.Equal(text, cr.ToString());
        }

        [Fact]
        public void parse_type_implied_nested() {
            string text = "T:System.Collections.Generic.Dictionary`2.KeyCollection`2";
            var cr = CodeReference.Parse(text);
            Assert.Equal(SymbolType.Type, cr.MetadataName.SymbolType);

            var name = (TypeName) cr.MetadataName;
            Assert.True(cr.IsValid);
            Assert.Equal("System.Collections.Generic.Dictionary`2+KeyCollection`2", name.FullName);
            Assert.True(name.IsGenericTypeDefinition);

            Assert.Equal(text, cr.ToString());
        }

    }
}
