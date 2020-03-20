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
using System.Reflection;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    using AssemblyName = Carbonfrost.Commons.DotNet.AssemblyName;

    // Needed here for netstandard/1.6
    delegate TOutput Converter<TInput, TOutput>(TInput input);

    public class TypeNameParseTests {

        [Fact]
        public void Parse_should_handle_basic_name() {
            var t = TypeName.Parse("String");
            Assert.IsInstanceOf(typeof(DefaultTypeName), t);
            Assert.Equal("String", t.Name);
            Assert.Equal("String", t.FullName);
            Assert.Equal("", t.Namespace);
            Assert.Null(t.Assembly);
        }

        [Theory]
        [XInlineData("[[System.String, mscorlib]]")]
        [InlineData("[mscorlib] System.String")]
        [InlineData("System.String, mscorlib")]
        public void Parse_should_handle_assembly_name(string text) {
            var t = TypeName.Parse(text);
            Assert.NotNull(t.Assembly);
            Assert.Equal("mscorlib", t.Assembly.Name);
        }

        [Fact]
        public void Parse_should_apply_to_byref_types() {
            var t = TypeName.Parse("String&");
            Assert.IsInstanceOf(typeof(ByReferenceTypeName), t);
        }

        [Fact]
        public void Parse_should_apply_to_pointer_types() {
            var t = TypeName.Parse("String*");
            Assert.IsInstanceOf(typeof(PointerTypeName), t);
        }

        [Fact]
        public void Parse_generic_type_definition() {
            var tn = TypeName.Parse("System.Tuple`8");
            Assert.Equal("System.Tuple`8", tn.FullName);
            Assert.True(tn.IsGenericTypeDefinition);
            Assert.Equal(8, tn.GenericParameterCount);
            Assert.Equal(8, tn.GenericParameters.Count);
            Assert.Null(tn.Assembly);
        }

        [Fact]
        public void Parse_nested_type_name() {
            var tn = TypeName.Parse("System.Exception+ExceptionMessageKind");
            Assert.Equal("System.Exception+ExceptionMessageKind", tn.FullName);
            Assert.True(tn.IsNested);
            Assert.Equal("System.Exception", tn.DeclaringType.FullName);

            var mscorlib = AssemblyName.FromAssemblyName(typeof(object).GetTypeInfo().Assembly.GetName());

            Assert.Null(tn.DeclaringType.Assembly);
            Assert.Null(tn.Assembly);
        }

        [Fact]
        public void Parse_nested_type_name_alt_syntax() {
            var tn = TypeName.Parse("System.Exception/ExceptionMessageKind");
            Assert.Equal("System.Exception+ExceptionMessageKind", tn.FullName);
            Assert.True(tn.IsNested);
            Assert.Equal("System.Exception", tn.DeclaringType.FullName);
        }

        [Fact]
        public void Parse_pointer_type_name() {
            var tn = TypeName.Parse("System.String*");
            Assert.Equal("System.String*", tn.FullName);
            Assert.True(tn.IsPointer);
        }


        [Fact]
        public void Parse_byref_type_name() {
            var tn = TypeName.Parse("System.String&");
            Assert.Equal("System.String&", tn.FullName);
            Assert.True(tn.IsByReference);
        }

        [Fact]
        public void Parse_type_with_multiple_arrays() {
            var tn = TypeName.Parse("String[][]");
            Assert.Equal("String[][]", tn.FullName);
            Assert.True(tn.IsArray);
            Assert.Equal("String[]", ((ArrayTypeName) tn).ElementType.FullName);

            var inner = ((ArrayTypeName) ((ArrayTypeName) tn).ElementType).ElementType;
            Assert.Equal("String", inner.FullName);
        }

        [Fact]
        public void Parse_unqualified_nested_type_name() {
            var type = TypeName.Parse("C+D");

            Assert.Equal("C+D", type.FullName);
            Assert.Equal("D", type.Name);
            Assert.Equal("", type.Namespace);
        }

        [Fact]
        public void Parse_generic_nested_type_name() {
            var type = TypeName.Parse("System.Collections.Generic.Dictionary`2+ValueCollection");

            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", type.FullName);
            Assert.Equal("ValueCollection", type.Name);
            Assert.True(type.IsNested);
        }

        [Fact]
        public void Parse_generic_nested_type_name_with_parameters() {
            var type = TypeName.Parse("System.Collections.Generic.Dictionary`2+ValueCollection`2");

            Assert.True(type.IsNested);
            Assert.Equal(4, type.GenericParameterCount);
            Assert.Equal(2, type.DeclaringType.GenericParameterCount);
            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection`2", type.FullName);
            Assert.Equal("ValueCollection`2", type.Name);
        }

        [Fact]
        public void Parse_generic_nested_type_name_implied_by_mangle() {
            // The mangle implies that ValueCollection is a nested type
            var type = TypeName.Parse("System.Collections.Generic.Dictionary`2.ValueCollection");

            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", type.FullName);
            Assert.Equal("ValueCollection", type.Name);
            Assert.True(type.IsNested);
        }

        [Fact]
        public void Parse_generic_nested_type_name_implied_by_generic_arguments() {
            var type = TypeName.Parse("System.Collections.Generic.Dictionary<TKey,TValue>.ValueCollection", TypeNameParseOptions.AssumeGenericParameters);

            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", type.FullName);
            Assert.Equal("ValueCollection", type.Name);

            // Generic parameters are added to nested types
            Assert.Equal(2, type.GenericParameterCount);
            Assert.Equal("ValueCollection", type.GenericParameters[0].DeclaringType.Name);
            Assert.Equal("ValueCollection", type.GenericParameters[1].DeclaringType.Name);
            Assert.True(type.IsNested);
            Assert.True(type.IsGenericType);
        }

        [Fact]
        public void Parse_generic_nested_type_name_implied_by_generic_params() {
            var type = TypeName.Parse("Dictionary<TKey,TValue>.ValueCollection<TCool>",
                                      TypeNameParseOptions.AssumeGenericParameters);

            Assert.True(type.IsGenericType);
            Assert.True(type.IsNested);
            Assert.Equal(3, type.GenericParameterCount);
            Assert.Equal("ValueCollection`1", type.Name);
            Assert.Equal("Dictionary`2+ValueCollection`1", type.FullName);
        }

        class A<T> {
            public class B<U> {}
        }

        [Fact]
        public void Parse_generic_nested_type_unnamed_generics() {
            var type = TypeName.Parse("System.Collections.Generic.Dictionary<,>.ValueCollection<>");

            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection`1", type.FullName);
            Assert.True(type.IsGenericType);
            Assert.Equal(3, type.GenericParameterCount);
            Assert.Equal("ValueCollection`1", type.Name);
            Assert.True(type.IsNested);
        }

        [Fact]
        [ExpectedException(typeof(ArgumentException))]
        public void Parse_generic_nested_type_name_unterminated() {
            // Missing ctor arguments
            var type = TypeName.Parse("Dictionary<TKey,TValue>.ValueCollection<");
        }

        [Fact]
        public void Parse_unqualified_type_name_generic_extra_ws() {
            Assert.Equal("Converter`2", typeof(Converter<,>).Name);
            var type = TypeName.Parse("Converter<TInput , TOutput>",
                                      TypeNameParseOptions.AssumeGenericParameters);
            Assert.Equal("Converter`2", type.FullName);
            Assert.Equal(2, type.GenericParameterCount);
            Assert.Equal("Converter`2", type.Name);
        }

        [Fact]
        public void Parse_unqualified_type_name_generic_arguments() {
            var type = TypeName.Parse("Converter<TInput,TOutput>");
            Assert.Equal("Converter<TInput, TOutput>", type.FullName);
            Assert.Equal(0, type.GenericParameterCount);

            var gits = (GenericInstanceTypeName) type;
            Assert.Equal(2, gits.GenericArguments.Count);

            Assert.True(gits.ElementType.IsGenericType);
            Assert.True(gits.ElementType.IsGenericTypeDefinition);
            Assert.Equal("Converter`2", gits.ElementType.Name);
            Assert.Equal("Converter`2", gits.ElementType.FullName);

            Assert.Equal("Converter`2", type.Name);
        }
    }
}
