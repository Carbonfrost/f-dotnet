//
// Copyright 2016, 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class MethodNameParseTests {

        [Fact]
        public void Parse_basic_method_name() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(System.Object, System.Int32)");

            Assert.Equal("CompareTo", id.Name);
            Assert.NotNull(id.DeclaringType);
            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal(2, id.Parameters.Count);

            Assert.Equal(2, id.ParameterCount);
            Assert.Equal("System.Object", id.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Int32", id.Parameters[1].ParameterType.FullName);
            Assert.Equal("Int32", id.Parameters[1].ParameterType.Name);
            Assert.Empty(id.Parameters[1].Name);
        }

        [Fact]
        public void Parse_method_name_with_argument_names() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(System.Object x, System.Int32 y)");

            Assert.Equal("CompareTo", id.Name);
            Assert.NotNull(id.DeclaringType);
            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal(2, id.Parameters.Count);

            Assert.Equal(2, id.ParameterCount);
            Assert.Equal("System.Object", id.Parameters[0].ParameterType.FullName);
            Assert.Equal("x", id.Parameters[0].Name);

            Assert.Equal("y", id.Parameters[1].Name);
            Assert.Equal("System.Int32", id.Parameters[1].ParameterType.FullName);
            Assert.Equal("Int32", id.Parameters[1].ParameterType.Name);
        }

        [Fact]
        public void Parse_method_name_no_parameter_names() {
            MethodName id = MethodName.Parse("CompareTo(:,:,:)");

            Assert.Equal("CompareTo", id.Name);
            Assert.Null(id.DeclaringType);
            Assert.Equal(3, id.Parameters.Count);

            Assert.Equal(3, id.ParameterCount);
            Assert.Equal(string.Empty, id.Parameters[0].Name);
            Assert.Equal(string.Empty, id.Parameters[1].Name);
            Assert.Equal(string.Empty, id.Parameters[2].Name);

            Assert.Null(id.Parameters[2].ParameterType);
            Assert.Equal(2, id.Parameters[2].Position);

            Assert.Equal("CompareTo(,,)", id.FullName);
        }

        [Fact]
        public void Parse_should_apply_to_methods_without_parameters_or_declaring_types() {
            var method = MethodName.Parse("ToString");
            Assert.Null(method.DeclaringType);
            Assert.NotNull(method.Parameters);
            Assert.False(method.HasParametersSpecified);
            Assert.Equal("ToString", method.Name);
        }

        [Fact]
        public void Parse_should_apply_to_ctor_with_declaring_type() {
            var method = MethodName.Parse("System.Exception..ctor(String, Exception)");
            Assert.True(method.HasParametersSpecified);
            Assert.Equal("System.Exception..ctor(String, Exception)", method.ToString());
            Assert.Equal(".ctor", method.Name);
        }

        [Fact]
        public void Parse_should_apply_to_ctor_without_declaring_type() {
            var method = MethodName.Parse(".ctor(Char*,Int32,Int32)");
            Assert.Null(method.DeclaringType);
            Assert.NotNull(method.Parameters);
            Assert.True(method.HasParametersSpecified);
            Assert.Equal(".ctor", method.Name);
            Assert.Equal(".ctor(Char*, Int32, Int32)", method.ToString());
        }

        [Fact]
        public void Parse_should_parse_byref_parameter_type() {
            var method = MethodName.Parse("TryGetTrailByte(Byte&)");
            Assert.Null(method.DeclaringType);
            Assert.NotNull(method.Parameters);
            Assert.True(method.HasParametersSpecified);
            Assert.Equal("TryGetTrailByte", method.Name);
            Assert.Equal("Byte&", method.Parameters[0].ParameterType.ToString());
            Assert.Equal("TryGetTrailByte(Byte&)", method.ToString());
        }

        [Fact]
        public void Parse_should_parse_byref_parameter_type_list() {
            MethodName method;
            Assert.True(MethodName.TryParse("TryParse(String,NumberStyles,IFormatProvider,UInt64&)", out method));
            Assert.Equal("UInt64&", method.Parameters.Last().ParameterType.Name);
        }

        [Fact]
        public void Parse_should_parse_return_type() {
            var method = MethodName.Parse("ToString():Byte");
            var format = new MetadataNameFormat();
            format.IncludeReturnTypes.All = true;
            Assert.Null(method.DeclaringType);
            Assert.Equal("ToString():Byte", method.ToString(format));
            Assert.NotNull(method.ReturnType);
            Assert.Equal(TypeName.Parse("Byte"), method.ReturnType);
        }

        [Fact]
        public void Parse_should_parse_byref_type() {
            var method = MethodName.Parse("TryParse(String,Int32&)");
            Assert.Null(method.DeclaringType);
            Assert.Equal("TryParse(String, Int32&)", method.ToString());
        }

        [Fact]
        public void Parse_should_parse_nested_array_type() {
            var method = MethodName.Parse("C(S+T[][])");
            Assert.Equal("C(S+T[][])", method.ToString());
        }

        [Fact]
        public void Parse_should_parse_pointer_type_arg() {
            var method = MethodName.Parse(".ctor(Char*)");
            Assert.Null(method.DeclaringType);
            Assert.Equal("Char*", method.Parameters[0].ToString());
        }

        [Theory]
        [InlineData("")]
        [InlineData((string) null)]
        public void Parse_should_throw_on_ArgumentException_errors(string text) {
            var ex = Record.Exception(() => MethodName.Parse(text));
            Assert.IsInstanceOf(typeof(ArgumentException), ex);
        }

        [Theory]
        [InlineData("TryParse(")]
        [InlineData("TryParse(Byte&&)")]
        [InlineData("TryParse(::)")]
        public void Parse_should_throw_on_errors(string text) {
            var ex = Record.Exception(() => MethodName.Parse(text));
            Assert.IsInstanceOf(typeof(ArgumentException), ex);
            Assert.Contains("instance of type `Carbonfrost.Commons.DotNet.MethodName'", ex.Message);
        }

        [Fact]
        public void Parse_method_name_no_parameters() {
            MethodName id = MethodName.Parse("CompareTo(,,)");

            Assert.Equal("CompareTo", id.Name);
            Assert.Null(id.DeclaringType);
            Assert.Equal(3, id.Parameters.Count);

            Assert.Equal(string.Empty, id.Parameters[0].Name);
            Assert.Equal(string.Empty, id.Parameters[1].Name);
            Assert.Equal(string.Empty, id.Parameters[2].Name);

            Assert.Null(id.Parameters[2].ParameterType);
            Assert.Equal(2, id.Parameters[2].Position);
        }

        [Fact]
        public void Parse_method_name_with_argument_names_alternate_form() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(x:System.Object, y:System.Int32)");

            Assert.Equal("CompareTo", id.Name);
            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal(2, id.Parameters.Count);

            Assert.Equal("System.Object", id.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Int32", id.Parameters[1].ParameterType.FullName);
            Assert.Equal("Int32", id.Parameters[1].ParameterType.Name);

            Assert.Equal("x", id.Parameters[0].Name);
            Assert.Equal("y", id.Parameters[1].Name);
        }

        [Fact]
        public void Parse_implied_nested_generic_type() {
            MethodName id = MethodName.Parse("System.Collections.Generic.Dictionary`2.ValueCollection.CopyTo()");

            Assert.Equal("CopyTo", id.Name);
            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", id.DeclaringType.FullName);
        }

        [Fact]
        public void Parse_assembly_qualified_name() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(x:System.Object, y:System.Int32), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("CompareTo", id.Name);

            Assert.NotNull(id.DeclaringType);
            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", id.DeclaringType.Assembly.FullName);

            Assert.Equal(2, id.Parameters.Count);

            Assert.Equal("System.Object", id.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Int32", id.Parameters[1].ParameterType.FullName);
            Assert.Equal("Int32", id.Parameters[1].ParameterType.Name);

            Assert.Equal("x", id.Parameters[0].Name);
            Assert.Equal("y", id.Parameters[1].Name);
        }

        [Fact]
        public void Parse_assembly_qualified_name2() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(x:[mscorlib, Version=2.0] System.Object, y:System.Int32), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("CompareTo", id.Name);

            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", id.DeclaringType.Assembly.FullName);

            Assert.Equal(2, id.Parameters.Count);

            Assert.Equal("System.Object", id.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Int32", id.Parameters[1].ParameterType.FullName);
            Assert.Equal("Int32", id.Parameters[1].ParameterType.Name);

            Assert.Equal("x", id.Parameters[0].Name);
            Assert.Equal("y", id.Parameters[1].Name);
        }

        [Fact]
        public void Parse_method_name_unqualified_parameters() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo");
            Assert.Equal("CompareTo", id.Name);
            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.NotNull(id.Parameters);
            Assert.Equal(-1, id.ParameterCount);
        }

        [Fact]
        public void Parse_method_name_unqualified_parameters_assembly() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");
            Assert.Equal("CompareTo", id.Name);

            Assert.Equal("System.IComparer", id.DeclaringType.FullName);
            Assert.Equal("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", id.DeclaringType.Assembly.FullName);

            Assert.False(id.HasParametersSpecified);
            Assert.NotNull(id.Parameters);
            Assert.Equal(-1, id.ParameterCount);
        }

        [Theory]
        [InlineData("System.Array.ConvertAll<TInput,TOutput>()")]
        [InlineData("System.Array.ConvertAll``2<TInput,TOutput>()")]
        [InlineData("System.Array.ConvertAll``2<,>()")]
        [InlineData("System.Array.ConvertAll``2()")]
        public void Parse_method_generic_parameter_count(string text) {
            var m = MethodName.Parse(text);

            Assert.IsInstanceOf(typeof(DefaultMethodName), m);
            Assert.Equal(2, m.GenericParameterCount);
            Assert.Equal("ConvertAll", m.Name);
        }

        [Theory]
        [InlineData("System.Array.ConvertAll<TInput,TOutput>()")]
        [InlineData("System.Array.ConvertAll``2<TInput,TOutput>()")]
        public void Parse_method_named_generic_parameters(string text) {
            var m = MethodName.Parse(text);

            Assert.IsInstanceOf(typeof(DefaultMethodName), m);
            Assert.Equal("System.Array.ConvertAll<TInput, TOutput>()", m.FullName);
            Assert.Equal(2, m.GenericParameterCount);
        }

        [Theory]
        [InlineData("System.Array.ConvertAll``2<,>(``0[])")]
        [InlineData("System.Array.ConvertAll``2(``0[])")]
        public void Parse_method_generic_parameter_array_argument(string text) {
            var m = MethodName.Parse(text);

            var first = (ArrayTypeName) m.Parameters[0].ParameterType;
            Assert.Equal(0, ((GenericParameterName) first.ElementType).Position);
        }

        [Fact]
        public void Parse_should_create_class_generic_parameters_dereference() {
            var method = MethodName.Parse("System.Class`2..ctor(`0,`1)");
            Assert.Same(method.DeclaringType.GenericParameters[0], method.Parameters[0].ParameterType);
            Assert.Same(method.DeclaringType.GenericParameters[1], method.Parameters[1].ParameterType);
        }

        [Fact]
        public void Parse_should_create_method_generic_parameters_dereference() {
            var method = MethodName.Parse("System.Class.Invoke``2(``0,``1)");
            Assert.Same(method.GenericParameters[0], method.Parameters[0].ParameterType);
            Assert.Same(method.GenericParameters[1], method.Parameters[1].ParameterType);
        }
    }
}
