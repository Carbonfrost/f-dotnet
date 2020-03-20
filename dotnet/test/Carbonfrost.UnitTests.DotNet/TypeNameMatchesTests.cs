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
using System.Reflection;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class TypeNameMatchesTests {

        [Fact]
        public void matches_basic_name() {
            var subject = TypeName.Parse("System.String");

            Assert.True(TypeName.Parse("String").Matches(subject));
            Assert.False(TypeName.Parse("System").Matches(subject));
        }

        [Fact]
        public void matches_full_name() {
            var subject = TypeName.Parse("System.String");

            Assert.True(TypeName.Parse("System.String").Matches(subject));
        }

        [Fact]
        public void matches_full_name_array() {
            var subject = TypeName.Parse("System.String[]");

            Assert.True(TypeName.Parse("System.String[]").Matches(subject));
            Assert.True(TypeName.Parse("String[]").Matches(subject));
            Assert.False(TypeName.Parse("System.String").Matches(subject));
            Assert.False(TypeName.Parse("String").Matches(subject));
            Assert.False(TypeName.Parse("Int32[]").Matches(subject));
        }

        [Fact]
        public void matches_full_name_pointer() {
            var subject = TypeName.Parse("System.String*");

            Assert.True(TypeName.Parse("System.String*").Matches(subject));
            Assert.True(TypeName.Parse("String*").Matches(subject));
            Assert.False(TypeName.Parse("System.String").Matches(subject));
            Assert.False(TypeName.Parse("String").Matches(subject));
            Assert.False(TypeName.Parse("Int32*").Matches(subject));
        }

        [Fact]
        public void matches_generic_nested_full_name() {
            var subject = TypeName.Parse("System.Collections.Generic.Dictionary<,>.ValueCollection");

            Assert.True(TypeName.Parse("System.Collections.Generic.Dictionary<,>+ValueCollection").Matches(subject));
        }

        [Fact]
        public void matches_generic_nested_name() {
            var subject = TypeName.Parse("System.Collections.Generic.Dictionary<,>+ValueCollection");

            Assert.True(TypeName.Parse("ValueCollection").Matches(subject));
        }

        [Fact]
        public void does_not_match_partial_full_name() {
            var subject = TypeName.Parse("System.String");

            Assert.False(TypeName.Parse("Namespace.String").Matches(subject));
            Assert.False(TypeName.Parse("tem.String").Matches(subject));
            Assert.False(TypeName.Parse("Other.System.String").Matches(subject));
        }

        [Fact]
        public void matches_partial_namespace_name() {
            var subject = TypeName.Parse("System.Collections.Generic.List`1");

            Assert.True(TypeName.Parse("Generic.List`1").Matches(subject));
            Assert.True(TypeName.Parse("Collections.Generic.List`1").Matches(subject));
            Assert.False(TypeName.Parse("ections.Generic.List`1").Matches(subject));
        }

        [Fact]
        public void matches_generic_instance_type_name() {
            var subject = TypeName.Parse("System.Collections.Generic.List<System.Int32>");

            Assert.True(TypeName.Parse("List<System.Int32>").Matches(subject));
            Assert.True(TypeName.Parse("Collections.Generic.List<System.Int32>").Matches(subject));
            Assert.True(TypeName.Parse("Collections.Generic.List<Int32>").Matches(subject));
            Assert.False(TypeName.Parse("System.Collections.Generic.List`1").Matches(subject));
        }

        [Fact]
        public void get_operator_name_semantics() {
            var type = TypeName.Parse("System.Int32");
            var adder = type.GetOperator(OperatorType.Addition);

            Assert.Equal(MethodName.Parse("System.Int32.op_Addition(System.Int32, System.Int32) : System.Int32"), adder);
        }

        [Fact]
        public void get_property_name_indexer() {
            var type = TypeName.Parse("System.String");
            var prop = type.GetProperty("Chars",
                                        TypeName.FromType(typeof(char)),
                                        new [] { TypeName.FromType(typeof(int)) });

            Assert.Equal("System.String.Chars[System.Int32]", prop.FullName);
            Assert.Equal("System.Char", prop.PropertyType.FullName);
        }

        class C<T> {
            internal class D<U> {}
        }

        [Fact]
        public void nested_type_generic_parameters_position() {
            // proof, for reference:
            var gti = typeof(C<>.D<>).GetTypeInfo();
            Assert.Equal(0, gti.GetGenericArguments()[0].GenericParameterPosition);
            Assert.Equal(1, gti.GetGenericArguments()[1].GenericParameterPosition);
            Assert.Equal("D`1", gti.GetGenericArguments()[0].DeclaringType.Name);
            Assert.Equal("D`1", gti.GetGenericArguments()[1].DeclaringType.Name);

            var type = TypeName.Parse("C`1+D`1");
            Assert.Equal(1 + 1, type.GenericParameterCount);
            Assert.IsInstanceOf(typeof(RedirectedGenericParameterName), type.GenericParameters[0]);
            Assert.Equal(0, type.GenericParameters[0].Position);
            Assert.Equal("D`1", type.GenericParameters[0].DeclaringType.Name);

            Assert.IsInstanceOf(typeof(BoundGenericParameterName), type.GenericParameters[1]);
            Assert.Equal(1, type.GenericParameters[1].Position);
            Assert.Equal("D`1", type.GenericParameters[1].DeclaringType.Name);

            Assert.NotNull(type.GenericParameters[0].DeclaringGenericParameter);
            Assert.Null(type.GenericParameters[1].DeclaringGenericParameter);

            Assert.Equal("`0", type.GenericParameters[0].DeclaringGenericParameter.Name);
            Assert.Equal("C`1", type.GenericParameters[0].DeclaringGenericParameter.DeclaringType.Name);
        }

        [Theory]
        [InlineData("String*")]
        [InlineData("String&")]
        [InlineData("String[]")]
        public void Matches_should_apply_reflexively(string text) {
            var type1 = TypeName.Parse(text);
            var type2 = TypeName.Parse(text);
            Assert.True(type1.Matches(type2));
        }

        [Theory]
        [InlineData("System.Collection+Enumerator")]
        [InlineData("Collection+Enumerator")]
        [InlineData("Enumerator")]
        public void Matches_should_apply_to_nested_types(string text) {
            var type2 = TypeName.Parse(text);
            var name = TypeName.Parse("System.Collection+Enumerator");
            Assert.True(type2.Matches(name));
        }
    }

    class E {
        public class Nested {}
    }
}
