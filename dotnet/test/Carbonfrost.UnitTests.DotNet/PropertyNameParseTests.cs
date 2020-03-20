//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class PropertyNameParseTests {

        [Theory]
        [InlineData("Chars[Byte&&]")]
        public void Parse_should_throw_on_errors(string text) {
            var ex = Record.Exception(() => PropertyName.Parse(text));
            Assert.IsInstanceOf(typeof(ArgumentException), ex);
            Assert.Contains("instance of type `Carbonfrost.Commons.DotNet.PropertyName'", ex.Message);
        }

        [Fact]
        public void Parse_should_handle_assembly() {
            var pn = PropertyName.Parse("System.String.Length, mscorlib, Version=1.0.0.0");
            Assert.Equal(pn.Assembly.FullName, "mscorlib, Version=1.0.0.0");
        }

        [Fact]
        public void Parse_should_handle_declaring_type() {
            var pn = PropertyName.Parse("System.String.Length");
            Assert.Equal(TypeName.Parse("System.String"), pn.DeclaringType);
        }

        [Theory]
        [InlineData("System.String.Chars[int index]")]
        [InlineData("System.String.Chars[index:int]")]
        [InlineData("System.String.Chars[int index,string s]")]
        [InlineData("System.String.Chars[index:int,string s]")]
        [InlineData("System.String.Chars[int index,s:string]")]
        [InlineData("System.String.Chars[index:int,s:string]")]
        public void Parse_should_handle_parameter_list_type_and_name(string name) {
            var pn = PropertyName.Parse(name);
            Assert.Equal("Chars", pn.Name);
            Assert.Equal("int", pn.Parameters[0].ParameterType.Name);
            Assert.Equal("index", pn.Parameters[0].Name);
            Assert.Equal(0, pn.Parameters[0].Position);
        }

        [Theory]
        [InlineData("System.String.Chars[int]")]
        [InlineData("System.String.Chars[int,string,bool]")]
        [InlineData("System.String.Chars[int,string s]")]
        [InlineData("System.String.Chars[int,s:string]")]
        public void Parse_should_handle_parameter_list_type_only(string name) {
            var pn = PropertyName.Parse(name);
            Assert.Equal("Chars", pn.Name);
            Assert.Equal(string.Empty, pn.Parameters[0].Name);
            Assert.Equal("int", pn.Parameters[0].ParameterType.Name);
        }

        [Theory]
        [InlineData("System.String.Chars[,,,]", 4)]
        [InlineData("System.String.Chars[]", 1)]
        public void Parse_should_handle_anonymous_parameter_list(string name, int count) {
            var pn = PropertyName.Parse(name);
            Assert.Equal("Chars", pn.Name);
            Assert.Equal(count, pn.Parameters.Count);
            if (count > 0) {
                Assert.Equal(string.Empty, pn.Parameters[0].Name);
                Assert.Equal(null, pn.Parameters[0].ParameterType);
            }
        }

        [Theory]
        [InlineData("Chars[Int64&]")]
        [InlineData("Chars[Int64& name]")]
        [InlineData("Chars[:Int64&]")]
        public void Parse_should_handle_byreference_parameter(string text) {
            var pn = PropertyName.Parse(text);
            Assert.Equal("Chars", pn.Name);
            Assert.Equal(1, pn.Parameters.Count);
            Assert.Equal("Int64&", pn.Parameters[0].ParameterType.Name);
            Assert.True(pn.Parameters[0].ParameterType.IsByReference);
        }

        [Theory]
        [InlineData("Chars[Int64*]")]
        [InlineData("Chars[Int64* name]")]
        [InlineData("Chars[:Int64*]")]
        public void Parse_should_handle_pointer_parameter(string text) {
            var pn = PropertyName.Parse(text);
            Assert.Equal("Chars", pn.Name);
            Assert.Equal(1, pn.Parameters.Count);
            Assert.Equal("Int64*", pn.Parameters[0].ParameterType.Name);
            Assert.True(pn.Parameters[0].ParameterType.IsPointer);
        }

        [Theory]
        [InlineData("System.String.Chars[]:System.Char")]
        [InlineData("System.String.Chars:System.Char")]
        public void Parse_should_handle_return_type(string name) {
            var pn = PropertyName.Parse(name);
            Assert.Equal("System.Char", pn.PropertyType.FullName);
        }

        [Fact]
        public void Parse_assumes_that_bare_property_names_are_properties_not_indexers() {
            var pn = PropertyName.Parse("String.Length");
            Assert.False(pn.IsIndexer);
        }
    }
}
