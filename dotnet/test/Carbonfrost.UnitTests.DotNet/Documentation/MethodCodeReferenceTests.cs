//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class MethodCodeReferenceTests {

        [Fact]
        public void parse_constructor_no_parameters() {
            string text = "M:Microsoft.CSharp.CSharpCodeProvider.#ctor";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.True(name.IsConstructor);
            Assert.Equal("Microsoft.CSharp.CSharpCodeProvider", name.DeclaringType.FullName);
            Assert.Equal(0, name.Parameters.Count);

            Assert.Equal(text + "()", c.ToString());
        }

        [Fact]
        public void parse_constructor_special_name() {
            string text = "M:System.Json.JsonPrimitive.#ctor(System.Boolean)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.True(name.IsConstructor);
            Assert.Equal(".ctor", name.Name);
            Assert.Equal("System.Json.JsonPrimitive", name.DeclaringType.FullName);
            Assert.Equal("System.Boolean", name.Parameters[0].ParameterType.FullName);

            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_generic_constructed_types() {
            CodeReference c = CodeReference.Parse(
                "M:System.Json.FormUrlEncodedJson.Parse(System.Collections.Generic.IEnumerable{System.Collections.Generic.KeyValuePair{System.String,System.String}})");

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("Parse", name.Name);
            Assert.Equal("FormUrlEncodedJson", name.DeclaringType.Name);
            Assert.Equal("System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<System.String, System.String>>", name.Parameters[0].ParameterType.FullName);

            GenericInstanceTypeName ins = (GenericInstanceTypeName) name.Parameters[0].ParameterType;
            Assert.Equal("System.Collections.Generic.KeyValuePair<System.String, System.String>", ins.GenericArguments[0].FullName);
            Assert.Equal("System.String", ((GenericInstanceTypeName) ins.GenericArguments[0]).GenericArguments[0].FullName);
        }

        [Fact]
        public void parse_generic_constructed_array_types() {
            CodeReference c = CodeReference.Parse(
                "M:System.Array.Sort``2(``0[],``1[],System.Int32,System.Int32)");

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("Sort", name.Name);
            Assert.Equal("Array", name.DeclaringType.Name);
            Assert.True(name.GenericParameters[0].IsPositional);
            Assert.True(name.Parameters[0].ParameterType.IsArray);
            Assert.Equal("``0[]", name.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Array.Sort``2(``0[], ``1[], System.Int32, System.Int32)", name.FullName);

            var ins = (ArrayTypeName) name.Parameters[0].ParameterType;
            Assert.Equal("``0", ins.ElementType.FullName);
        }

        [Fact]
        public void parse_array_type_parameter() {
            string text = "M:System.ArgIterator.#ctor(System.RuntimeArgumentHandle,System.Void*)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal(".ctor", name.Name);
            Assert.Equal("System.ArgIterator", name.DeclaringType.FullName);
            Assert.True(name.Parameters[1].ParameterType.IsPointer);
            Assert.Equal("System.Void*", name.Parameters[1].ParameterType.FullName);
            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_pointer_type_parameter() {
            string text = "M:System.Security.Cryptography.AsnEncodedData.#ctor(System.Byte[])";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal(".ctor", name.Name);
            Assert.Equal("System.Security.Cryptography.AsnEncodedData", name.DeclaringType.FullName);
            Assert.True(name.Parameters[0].ParameterType.IsArray);
            Assert.Equal("System.Byte[]", name.Parameters[0].ParameterType.FullName);
            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_parameter_out_ref_parameter() {
            string text = "M:System.UriParser.Resolve(System.Uri,System.Uri,System.UriFormatException@)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("Resolve", name.Name);
            Assert.Equal("System.UriParser.Resolve(System.Uri, System.Uri, System.UriFormatException&)", name.ToString());
            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_method_return_type() {
            string text = "M:System.DateTimeOffset.op_Implicit(System.DateTime)~System.DateTimeOffset";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.True(name.IsOperator);
            Assert.Equal("op_Implicit", name.Name);
            Assert.Equal("System.DateTimeOffset", name.ReturnType.FullName);
            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_generic_argument_array_byref() {
            string text = "M:System.Array.Resize``1(``0[]@,System.Int32)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("Resize", name.Name);
            Assert.Equal("System.Array.Resize``1(``0[]&, System.Int32)", name.FullName);
            Assert.Equal(text, c.ToString());
        }

        [Fact]
        public void parse_generic_argument_array() {
            string text = "M:System.Collections.Concurrent.ConcurrentDictionary`2.System#Collections#Generic#ICollection{T}#CopyTo(System.Collections.Generic.KeyValuePair{`0,`1}[],System.Int32)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("System.Collections.Generic.ICollection`1.CopyTo", name.Name);
            Assert.Equal("System.Collections.Concurrent.ConcurrentDictionary`2::System.Collections.Generic.ICollection`1.CopyTo(System.Collections.Generic.KeyValuePair<`0, `1>[], System.Int32)", name.FullName);

            Assert.Equal("M:System.Collections.Concurrent.ConcurrentDictionary`2.System#Collections#Generic#ICollection{T}#CopyTo(System.Collections.Generic.KeyValuePair{`0,`1}[],System.Int32)", c.ToString());
        }

        [Fact]
        public void parse_generic_nested_constructor_leading_ws() {
            string text = " M:System.Collections.Generic.Dictionary`2.KeyCollection.#ctor(System.Collections.Generic.Dictionary{`0,`1})";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal(".ctor", name.Name);

            // Nested type is inferred from the mangle
            Assert.Equal("System.Collections.Generic.Dictionary`2+KeyCollection..ctor(System.Collections.Generic.Dictionary<`0, `1>)", name.FullName);

            Assert.Equal("M:System.Collections.Generic.Dictionary`2.KeyCollection.#ctor(System.Collections.Generic.Dictionary{`0,`1})", c.ToString());
        }

        [Fact]
        public void parse_generic_nested_method_array_trailing_ws() {
            string text = "M:System.Collections.Generic.Dictionary`2.KeyCollection.CopyTo(`0[],System.Int32) ";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("CopyTo", name.Name);

            // Nested type is inferred from the mangle
            Assert.Equal("System.Collections.Generic.Dictionary`2+KeyCollection.CopyTo(`0[], System.Int32)", name.FullName);

            Assert.Equal("M:System.Collections.Generic.Dictionary`2.KeyCollection.CopyTo(`0[],System.Int32)", c.ToString());
        }

        [Fact]
        public void parse_generic_nested_explicit_generic_interface_impl() {
            string text = "M:System.Collections.Generic.Dictionary`2.KeyCollection.System#Collections#Generic#ICollection{T}#Add(`0)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("System.Collections.Generic.ICollection`1.Add", name.Name);

            // Nested type is inferred from the mangle
            Assert.Equal("System.Collections.Generic.Dictionary`2+KeyCollection::System.Collections.Generic.ICollection`1.Add(`0)", name.FullName);

            Assert.Equal("M:System.Collections.Generic.Dictionary`2.KeyCollection.System#Collections#Generic#ICollection{T}#Add(`0)", c.ToString());
        }

        [Fact]
        public void Parse_should_handle_unqualified_names() {
            string text = "M:Add(string,string,int)";
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.Equal("Add", name.Name);

            // Nested type is inferred from the mangle
            Assert.Equal("Add(string, string, int)", name.FullName);
            Assert.Equal("M:Add(string,string,int)", c.ToString());
        }

        [Theory]
        [InlineData("M:System.Drawing.Size.op_Explicit(System.Drawing.Size to System.Drawing.Point)")]
        [InlineData("M:System.Drawing.Size.op_Explicit(System.Drawing.Size)~System.Drawing.Point")]
        [InlineData("M:System.Drawing.Size.op_Implicit(System.Drawing.Size to System.Drawing.Point)")]
        [InlineData("M:System.Drawing.Size.op_Implicit(System.Drawing.Size)~System.Drawing.Point")]
        public void Parse_should_consider_operator_syntax(string text) {
            CodeReference c = CodeReference.Parse(text);

            Assert.True(c.IsValid);
            Assert.Equal(CodeReferenceType.Valid, c.ReferenceType);

            MethodName name = (MethodName) c.MetadataName;
            Assert.True(name.IsOperator);
            Assert.Equal(1, name.Parameters.Count);
            Assert.Equal("System.Drawing.Size", name.Parameters[0].ParameterType.FullName);
            Assert.Equal("System.Drawing.Size", name.DeclaringType.FullName);
            Assert.Matches("op_..plicit", name.Name);
            Assert.Equal("System.Drawing.Point", name.ReturnType.FullName);
        }

        [Fact]
        public void Split_parameters_zero_length() {
            var p = MethodCodeReference.SplitParametersInternal(", ,,").ToArray();
            Assert.Equal(4, p.Length);
            Assert.Equal(string.Empty, p[0]);
            Assert.Equal(string.Empty, p[1]);
            Assert.Equal(string.Empty, p[2]);
            Assert.Equal(string.Empty, p[3]);
        }

        [Fact]
        public void Split_parameters_generic_names() {
            var p = MethodCodeReference.SplitParametersInternal("TInput[], Converter<TInput, TOutput>").ToArray();
            Assert.Equal(2, p.Length);
            Assert.Equal("TInput[]", p[0]);
            Assert.Equal("Converter<TInput, TOutput>", p[1]);
        }

        [Fact]
        public void Split_parameter_name_nominal_colon() {
            string param, type;
            MethodCodeReference.SplitParameterName("hello : String", out param, out type);
            Assert.Equal("String", type);
            Assert.Equal("hello", param);
        }

        [Fact]
        public void Split_parameter_name_generic_name() {
            string param, type;
            MethodCodeReference.SplitParameterName("Converter<TInput, TOutput>", out param, out type);
            Assert.Equal("Converter<TInput, TOutput>", type);
            Assert.Equal("", param);
        }
    }
}
