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

    public class PropertyCodeReferenceTests {

        [Fact]
        public void parse_property_nominal() {
            var cr = CodeReference.Parse("P:System.Json.JsonValueChangeEventArgs.Key");
            Assert.True(cr.IsValid);
            Assert.Equal(SymbolType.Property, cr.MetadataName.SymbolType);

            var name = (PropertyName) cr.MetadataName;
            Assert.Equal("System.Json.JsonValueChangeEventArgs", name.DeclaringType.FullName);
            Assert.Equal("Key", name.Name);
        }

        [Fact]
        public void parse_property_with_index_parameters() {
            var cr = CodeReference.Parse("P:System.Json.JsonValue.Item(System.String)");
            Assert.True(cr.IsValid);
            Assert.Equal(SymbolType.Property, cr.MetadataName.SymbolType);

            var name = (PropertyName) cr.MetadataName;
            Assert.Equal("System.Json.JsonValue", name.DeclaringType.FullName);
            Assert.Equal("Item", name.Name);
            Assert.Equal("System.String", name.Parameters[0].ParameterType.FullName);
        }

        [Fact]
        public void parse_property_explicit_implementation_generics() {
            string text = "P:System.Collections.ObjectModel.ReadOnlyDictionary`2.System#Collections#Generic#IDictionary{TKey@TValue}#Item(`0)";

            var cr = CodeReference.Parse(text);
            Assert.True(cr.IsValid);
            Assert.Equal(SymbolType.Property, cr.MetadataName.SymbolType);

            var name = (PropertyName) cr.MetadataName;
            Assert.Equal("System.Collections.ObjectModel.ReadOnlyDictionary`2", name.DeclaringType.FullName);
            Assert.Equal("System.Collections.Generic.IDictionary`2.Item", name.Name);
            Assert.Equal(1, name.Parameters.Count);
            Assert.Null(name.Parameters[0].Name);
            Assert.NotNull(name.Parameters[0].ParameterType);

            Assert.True(name.Parameters[0].ParameterType.IsGenericParameter);
            Assert.Equal(0, ((GenericParameterName) name.Parameters[0].ParameterType).Position);

            // TODO The name of the type parameters gets lost here (are these considered the same?)
            Assert.Equal("P:System.Collections.ObjectModel.ReadOnlyDictionary`2.System#Collections#Generic#IDictionary{T1@T2}#Item(`0)",
                    cr.ToString());
        }

        [Fact]
        public void parse_property_explicit_implementation() {
            var cr = CodeReference.Parse("P:System.ComponentModel.BindingList`1.System#ComponentModel#IBindingList#AllowEdit");
            Assert.True(cr.IsValid);
            Assert.Equal(SymbolType.Property, cr.MetadataName.SymbolType);

            var name = (PropertyName) cr.MetadataName;
            Assert.Equal("System.ComponentModel.BindingList`1", name.DeclaringType.FullName);
            Assert.Equal("System.ComponentModel.IBindingList.AllowEdit", name.Name);
            Assert.Equal("P:System.ComponentModel.BindingList`1.System#ComponentModel#IBindingList#AllowEdit", cr.ToString());
        }

    }
}
