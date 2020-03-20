//
// Copyright 2016, 2014 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

class NoNamespaceType {}

namespace Carbonfrost.UnitTests.DotNet {

    using AssemblyName = Carbonfrost.Commons.DotNet.AssemblyName;

    public class TypeNameTests {

        [Fact]
        public void FromType_should_apply_to_types_without_namespaces() {
            var type = TypeName.FromType(typeof(NoNamespaceType));
            Assert.Equal("", type.Namespace);
        }

        [Fact]
        public void Equal_should_apply_to_by_ref_types() {
            var type1 = TypeName.Parse("String").MakeByReferenceType();
            var type2 = TypeName.Parse("String").MakeByReferenceType();
            Assert.True(type1.Equals(type2));
        }

        [Fact]
        public void MakeGenericType_nominal() {
            TypeName tn = TypeName.FromType(typeof(Converter<,>));

            var gen = tn.MakeGenericType(TypeName.Int16, TypeName.Int32);

            Assert.Equal(tn.Assembly, gen.Assembly);
            Assert.Equal(tn, gen.ElementType);
            Assert.Equal("Converter`2", gen.Name);
        }

        [Fact,
         ExpectedException(typeof(ArgumentException))]
        public void MakeGenericType_mismatch_arg_count() {
            TypeName tn = TypeName.FromType(typeof(Converter<,>));
            tn.MakeGenericType(TypeName.Int16);
        }

        [Fact]
        public void MakeNullableType_from_byref_type() {
            TypeName tn = TypeName.FromType(typeof(int)).MakeByReferenceType();
            Assert.Throws<InvalidOperationException>(() => {
                            tn.MakeNullableType();
            });
        }

        [Fact]
        public void MakeByReferenceType_from_byref_type() {
            TypeName tn = TypeName.FromType(typeof(int)).MakeByReferenceType();
            Assert.Throws<InvalidOperationException>(() => {
                tn.MakeNullableType();
            });
        }

        [Fact]
        public void Assembly_exists_for_type_specification() {
            TypeName tn = TypeName.FromType(typeof(void).MakePointerType());

            var mscorlib = AssemblyName.FromAssemblyName(typeof(object).GetTypeInfo().Assembly.GetName());
            Assert.Equal(mscorlib, tn.Assembly);
        }

        [Fact]
        public void FromType_convert_nested_type() {
            TypeName name = TypeName.FromType(typeof(E.Nested));
            TypeName dname = TypeName.FromType(typeof(E));
            Assert.True(name.IsNested);
            Assert.Equal(dname, name.DeclaringType);
        }

        [Fact]
        public void FromType_convert_pointer_type() {
            TypeName name = TypeName.FromType(typeof(void).MakePointerType());
            Assert.True(name.IsPointer);
            Assert.Equal(TypeName.Void, ((TypeSpecificationName) name).ElementType);
            Assert.Equal("Void*", name.Name);
            Assert.Equal("System.Void*", name.FullName);
        }

        [Fact]
        public void FromType_convert_array_type() {
            TypeName name = TypeName.FromType(typeof(void).MakeArrayType(2));
            Assert.True(name.IsArray);

            ArrayTypeName array = (ArrayTypeName) name;
            Assert.Equal(TypeName.Void, array.ElementType);
            Assert.Equal(2, array.Dimensions.Count);
            Assert.Equal("Void[,]", array.Name);
        }

        [Fact]
        public void FromType_generic_instance_type() {
            TypeName name = TypeName.FromType(typeof(IList<Tuple<string, string[]>>));
            Assert.True(name.IsGenericType);
            Assert.False(name.IsGenericTypeDefinition);
            Assert.Equal("System.Collections.Generic.IList<System.Tuple<System.String, System.String[]>>", name.FullName);
        }

        [Fact]
        public void FromType_generic_type_definition() {
            TypeName name = TypeName.FromType(typeof(Tuple<,,,>));
            Assert.True(name.IsGenericType);
            Assert.True(name.IsGenericTypeDefinition);
            Assert.Equal("System.Tuple`4", name.FullName);
            Assert.Equal(4, name.GenericParameterCount);
            Assert.Equal(typeof(object).GetTypeInfo().Assembly.FullName, name.Assembly.FullName);
            Assert.Equal(new [] { "T1", "T2", "T3", "T4" }, name.GenericParameters.Select(t => t.Name));
        }

        [Fact]
        public void FromType_generic_type_definition_nested() {
            TypeName name = TypeName.FromType(typeof(Dictionary<,>.ValueCollection));
            Assert.True(name.IsGenericType);
            Assert.True(name.IsGenericTypeDefinition);
            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", name.FullName);
            Assert.Equal(2, name.GenericParameterCount);
            Assert.Equal(typeof(object).GetTypeInfo().Assembly.FullName, name.Assembly.FullName);

            Assert.Equal(new [] { "TKey", "TValue" }, name.GenericParameters.Select(t => t.Name));
            Assert.Equal(name, name.GenericParameters[0].DeclaringType);
        }

        class C<T,U> {
            internal class D<V, W> {}
        }

        class E<T> {
            internal class C<E> {}
        }

        [Fact]
        public void FromType_should_get_closed_inherited_generic_arguments() {
            var s = new C<int,long>.D<string, string>();

            // proof, for reference:
            var args = s.GetType().GetTypeInfo().GetGenericArguments();
            Assert.Equal(new [] { typeof(int), typeof(long), typeof(string), typeof(string) }, args);

            var t = (GenericInstanceTypeName) TypeName.FromType(s.GetType());
            Assert.HasCount(4, t.GenericArguments);
            Assert.Equal(new [] { "Int32", "Int64", "String", "String" },
                         t.GenericArguments.Select(u => u.Name).ToArray());
        }

        [Fact]
        public void FromType_generic_type_definition_nested_generic() {
            var ti = typeof(C<,>.D<,>).GetTypeInfo();
            TypeName name = TypeName.FromType(typeof(C<,>.D<,>));
            // proof, for reference:
            Assert.HasCount(4, ti.GetGenericArguments());
            Assert.Equal("D`2", ti.Name);
            Assert.Equal("Carbonfrost.UnitTests.DotNet.TypeNameTests+C`2+D`2", ti.FullName);

            Assert.True(name.IsGenericType);
            Assert.True(name.IsGenericTypeDefinition);
            Assert.Equal("Carbonfrost.UnitTests.DotNet.TypeNameTests+C`2+D`2", name.FullName);
            Assert.Equal(4, name.GenericParameterCount); // inherited generics
        }

        [Fact]
        public void GetNestedType_type_open_generic() {
            var type = TypeName.Parse("System.Array");
            var nested = type.GetNestedType("FunctorComparer`1");

            Assert.Equal("System.Array+FunctorComparer`1", nested.FullName);
            Assert.Equal(1, nested.GenericParameterCount);
        }

        [Fact]
        public void GetNestedType_type_open_generic_names() {
            var type = TypeName.Parse("System.Array");
            var nested = type.GetNestedType("FunctorComparer<T>");

            Assert.Equal("System.Array+FunctorComparer`1", nested.FullName);
            Assert.Equal(1, nested.GenericParameterCount);

            var gp = nested.GenericParameters[0];
            Assert.Equal("T", gp.Name);
            Assert.Null(gp.DeclaringGenericParameter);
            Assert.Equal("FunctorComparer`1", gp.DeclaringType.Name);
            Assert.Equal("System", gp.DeclaringType.Namespace);
        }

        [Fact]
        public void GetNestedType_should_append_nested_generic() {
            var type = TypeName.Parse("C`2");
            var nested = type.GetNestedType("D`2");

            Assert.Equal("C`2+D`2", nested.FullName);
            Assert.Equal(4, nested.GenericParameterCount); // due to redirected
        }

        [Fact]
        public void WithDeclaringType_should_update_it() {
            TypeName parentType = TypeName.FromType(typeof(int));
            TypeName name = TypeName.FromType(typeof(string));
            name = name.WithDeclaringType(parentType);
            Assert.Equal("String", name.Name);
            Assert.Equal("System.Int32+String", name.FullName);
            Assert.Equal("System", name.Namespace);
            Assert.Equal("System.Int32", name.DeclaringType.FullName);
            Assert.Same(parentType.Assembly, name.DeclaringType.Assembly);
            Assert.Same(parentType.Assembly, name.Assembly);
        }

        [Fact]
        public void WithNamespace_should_update_declaring_type() {
            TypeName t = TypeName.Parse("C+D");
            t = t.WithNamespace("S");
            Assert.Equal("S.C+D", t.FullName);
        }

        [Fact]
        public void WithAssembly_should_update_declaring_type() {
            TypeName t = TypeName.Parse("C+D");
            var asm = AssemblyName.Parse("A");
            t = t.WithAssembly(asm);
            Assert.Equal(asm, t.Assembly);
            Assert.Equal("C+D, A", t.AssemblyQualifiedName);
        }

        [Fact]
        public void WithAssembly_should_update_self() {
            var t = TypeName.Parse("D");
            var asm = AssemblyName.Parse("A");
            t = t.WithAssembly(asm);
            Assert.Equal("D, A", t.AssemblyQualifiedName);
        }

        [Fact]
        public void WithAssembly_should_update_specification() {
            var t = TypeName.Parse("D&");
            var asm = AssemblyName.Parse("A");
            t = t.WithAssembly(asm);
            Assert.Equal("D&, A", t.AssemblyQualifiedName);
        }

        [Fact]
        public void WithAssembly_should_update_declaring_type_with_null() {
            TypeName t = TypeName.Parse("C+D, A").WithAssembly(null);

            Assert.Equal(t.Assembly, null);
        }

        [Fact]
        public void WithAssembly_should_update_self_with_null() {
            var t = TypeName.Parse("D, A").WithAssembly(null);

            Assert.Equal(t.Assembly, null);
        }

        [Fact]
        public void MakeNullableType_nominal() {
            var subject = TypeName.Parse("System.Int32").MakeNullableType();
            Assert.Equal("System.Nullable<System.Int32>", subject.FullName);
            Assert.True(subject.IsNullable);
        }

    }
}
