//
// Copyright 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using TokenType = Carbonfrost.Commons.DotNet.SignatureParser.TokenType;

namespace Carbonfrost.UnitTests.DotNet {

    public class SignatureParserTests {

        public IEnumerable<TypeName> ParameterTypes {
            get {
                return new [] {
                    TypeName.Parse("Type"),
                    TypeName.Parse("N.Type"),
                    TypeName.Parse("Type*"),
                    TypeName.Parse("N.Type"),
                    TypeName.Parse("Type&"),
                    TypeName.Parse("N.Type&"),
                    TypeName.Parse("Type&"),
                    TypeName.Parse("N.Type+U"),
                    TypeName.Parse("``1"),
                    TypeName.Parse("`1"),
                    TypeName.Parse("A`1"),
                    TypeName.Parse("A`1&"),
                    TypeName.Parse("A<E>"),
                    TypeName.Parse("A<E,B>"),
                    TypeName.Parse("A<E,B>&"),
                };
            }
        }

        public IEnumerable<string> ParameterFormats {
            get {
                return new [] {
                    "{name}:{type}",
                    "{name}: {type}",
                    "{name} :{type}",
                    "{name} : {type}",
                    "{type} {name}",
                    "{type} {name} ",
                };
            }
        }

        [Fact]
        public void ParseQualifiedType_should_handle_basic_name() {
            var t = SignatureParser.ParseQualifiedType("A");
            Assert.Equal("A", t.Name);
            Assert.Equal("", t.Namespace);
        }

        [Fact]
        public void ParseQualifiedType_should_handle_namespace() {
            var t = SignatureParser.ParseQualifiedType("A.B");
            Assert.Equal("B", t.Name);
            Assert.Equal("A", t.Namespace);
        }

        [Fact]
        public void ParseQualifiedType_should_handle_namespace_nested() {
            var t = SignatureParser.ParseQualifiedType("N.A.B");
            Assert.Equal("B", t.Name);
            Assert.Equal("N.A", t.Namespace);
        }

        [Theory]
        [InlineData("N.A/B")]
        [InlineData("N.A+B")]
        public void ParseQualifiedType_should_handle_nested_types(string text) {
            var t = SignatureParser.ParseQualifiedType(text);
            Assert.Equal("B", t.Name);
            Assert.Equal("A", t.DeclaringType.Name);
            Assert.Equal("N", t.Namespace);
        }

        [Fact]
        public void ParseTypeWithSpecifiers_type_args() {
            var s = new SignatureParser("Hello<W,X>");
            s.MoveNext();
            var t = s.ParseTypeWithSpecifiers();
            Assert.Equal("Hello`2", t.Name);
            var gargs = ((GenericInstanceTypeName) t).GenericArguments;
            Assert.HasCount(2, gargs);
            Assert.Equal("W", gargs[0].Name);
            Assert.Equal("X", gargs[1].Name);
        }

        [Fact]
        public void ParseTypeWithSpecifiers_type_parameters_given_name() {
            var s = new SignatureParser("Hello`1<W,X>");
            s.MoveNext();
            var t = s.ParseTypeWithSpecifiers();
            Assert.Equal("Hello`2", t.Name);
            Assert.HasCount(2, t.GenericParameters);
            Assert.Equal("W", t.GenericParameters[0].Name);
            Assert.Equal("X", t.GenericParameters[1].Name);
        }

        [Fact]
        public void ParseQualifiedType_should_extract_implied_generic_nested() {
            var t = SignatureParser.ParseQualifiedType("System.Collections.Generic.Dictionary`2.ValueCollection");
            Assert.Equal("ValueCollection", t.Name);
            Assert.Equal("Dictionary`2", t.DeclaringType.Name);
            Assert.Equal("System.Collections.Generic", t.Namespace);
        }

        [Theory]
        [InlineData("Hello")]
        [InlineData("Hello.Other")]
        [InlineData("Hello.Other.Another")]
        [InlineData("Hello+Other")]
        [XInlineData("Hello+Other<Arg, Arg>")]
        public void ParseDeclaringTypeOpt_should_extract_declaring(string declaring) {
            var s = new SignatureParser(declaring + ".World[");
            s.MoveNext();
            string name;
            var data = s.ParseDeclaringTypeOpt(out name);

            Assert.Equal(declaring, data.FullName);
            Assert.Equal("World", name);
            Assert.Equal(TokenType.LeftBracket, s.Type);
        }

        [Fact]
        public void ParseDeclaringTypeOpt_should_extract_implied_generic_nested() {
            var s = new SignatureParser("System.Collections.Generic.Dictionary`2.ValueCollection.Method(");
            s.MoveNext();
            string name;
            var data = s.ParseDeclaringTypeOpt(out name);

            Assert.Equal("Method", name);
            Assert.Equal("System.Collections.Generic.Dictionary`2+ValueCollection", data.FullName);
            Assert.Equal(TokenType.LeftParen, s.Type);
        }

        [Fact]
        public void ParseDeclaringTypeOpt_should_extract_declaring_alternate_nested_format() {
            var s = new SignatureParser("Hello/Other.World");
            s.MoveNext();
            string name;
            var data = s.ParseDeclaringTypeOpt(out name);

            Assert.Equal("Hello+Other", data.FullName);
            Assert.Equal("World", name);
            Assert.Equal(TokenType.EndOfInput, s.Type);
        }

        [Theory]
        [InlineData(".ctor")]
        [InlineData(".cctor")]
        [InlineData("UnqualifiedName")]
        public void ParseDeclaringTypeOpt_should_handle_unqualified_names(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            string name;
            var data = s.ParseDeclaringTypeOpt(out name);

            Assert.Null(data);
            Assert.Equal(text, name);
            Assert.Equal(TokenType.EndOfInput, s.Type);
        }

        [Theory]
        [InlineData("Hello..ctor")]
        [InlineData("Hello..cctor")]
        public void ParseDeclaringTypeOpt_should_handle_constructors_declaring_type(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            string name;
            var data = s.ParseDeclaringTypeOpt(out name);

            Assert.Equal("Hello", data.Name);
            Assert.Equal(text.Substring(1 + "Hello".Length), name);
            Assert.Equal(TokenType.EndOfInput, s.Type);
        }

        // Notice here that we must end all example parameters with a comma
        // that is realistic to the parser when it is called

        [Theory]
        [PropertyData("ParameterTypes")]
        public void ParseParameter_should_support_type_space_name(TypeName type) {
            string text = type + " " + "name" + ",";
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Equal("name", data.Name);
            Assert.Equal(type, data.Type);
        }

        [Theory]
        [PropertyData("ParameterTypes")]
        public void ParseParameter_should_support_name_colon_type(TypeName type) {
            string text = "name" + ":" + type + ",";
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Equal("name", data.Name);
            Assert.Equal(type, data.Type);
        }

        [Theory]
        [InlineData("name:,")]
        [InlineData("name : ,")]
        public void ParseParameter_should_support_name_without_type(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Equal("name", data.Name);
            Assert.Equal(null, data.Type);
        }

        [Theory]
        [PropertyData("ParameterTypes")]
        public void ParseParameter_should_support_type_without_name(TypeName type) {
            var s = new SignatureParser(type.FullName + ",");
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Empty(data.Name);
            Assert.Equal(type, data.Type);
        }

        [Theory]
        [PropertyData("ParameterTypes")]
        public void ParseParameter_should_support_colon_type(TypeName type) {
            string text = ":" + type.FullName + ",";
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Empty(data.Name);
            Assert.Equal(type, data.Type);
        }

        [Theory]
        [InlineData("x:[mscorlib, Version=2.0] System.Object, ")]
        [InlineData("[mscorlib, Version=2.0] System.Object x, ")]
        public void ParseParameter_should_handle_assembly_qualified_type(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();

            Assert.Equal("x", data.Name);
            Assert.Equal("System.Object", data.Type.FullName);
            Assert.Equal("mscorlib, Version=2.0", data.Type.Assembly.FullName);
        }

        [Theory]
        [PropertyData("ParameterTypes", "ParameterFormats")]
        public void ParseParameter_should_support_various_formats(TypeName parameterType, string format) {
            string text = format.Replace("{type}", parameterType.ToString())
                                .Replace("{name}", "p") + ",";
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameter();
            Assert.Equal("p", data.Name);
            Assert.Equal(parameterType, data.Type);
        }

        [Theory]
        [XInlineData("[[System.String, mscorlib]]")]
        [InlineData("[mscorlib] System.String")]
        public void ParseParameterTypeOpt_should_handle_assembly_name(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var t = s.ParseParameterTypeOpt();

            Assert.NotNull(t.Assembly);
            Assert.Equal("mscorlib", t.Assembly.Name);
        }

        [Theory]
        [InlineData("N.T+U")]
        [InlineData("N.T/U")]
        public void ParseParameterTypeOpt_should_handle_nested(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameterTypeOpt();

            Assert.NotNull(data);
            Assert.Equal("N.T+U", data.FullName);
            Assert.Equal("N", data.Namespace);
            Assert.Equal("U", data.Name);
            Assert.Equal("T", data.DeclaringType.Name);
        }

        [Theory]
        [InlineData("``1")]
        [InlineData("`1")]
        [InlineData("Type<,>")]
        [InlineData("Type<>")]
        [InlineData("N.Type<,>")]
        [InlineData("N.Type<>")]
        public void ParseParameterTypeOpt_should_handle_generics(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();

            var data = s.ParseParameterTypeOpt();

            Assert.NotNull(data);
            Assert.Equal(TypeName.Parse(text), data);
        }

        [Theory]
        [InlineData("A[]", 1)]
        [InlineData("A[,]", 2)]
        [InlineData("A[,,]", 3)]
        public void ParseParameterTypeOpt_should_handle_arrays(string text, int dimensions) {
            var s = new SignatureParser(text);
            s.MoveNext();

            var data = s.ParseParameterTypeOpt();

            Assert.NotNull(data);
            Assert.True(data.IsArray);
            Assert.Equal(dimensions, ((ArrayTypeName) data).Dimensions.Count);
        }

        [Fact]
        public void ParseParameters_should_handle_empty_list() {
            var s = new SignatureParser("[]");
            s.MoveNext();
            Assert.Empty(s.ParseParameters(TokenType.RightBracket));
        }

        [Theory]
        [InlineData("[Int64&]")]
        [InlineData("[Int64& name]")]
        [InlineData("[:Int64&]")]
        public void ParseParameters_should_handle_byreference_parameter(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameters(TokenType.RightBracket).ToList();

            Assert.Equal(1, data.Count);
            Assert.Equal("Int64&", data[0].Type.Name);
            Assert.True(data[0].Type.IsByReference);
            Assert.Equal(TokenType.RightBracket, s.Type);
        }

        [Theory]
        [InlineData("[Int64*]")]
        [InlineData("[Int64* name]")]
        [InlineData("[:Int64*]")]
        public void ParseParameters_should_handle_pointer_parameter(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameters(TokenType.RightBracket).ToList();

            Assert.Equal(1, data.Count);
            Assert.Equal("Int64*", data[0].Type.Name);
            Assert.True(data[0].Type.IsPointer);
            Assert.Equal(TokenType.RightBracket, s.Type);
        }

        [Theory]
        [XInlineData("[Int64* , Int32\t\t\n, Int64&, name : string &, string * name ]")]
        public void ParseParameters_should_handle_whitespace(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseParameters(TokenType.RightBracket).ToList();
            Assert.Equal(5, data.Count);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ParseTypeParametersOpt_should_handle_names(bool method) {
            var s = new SignatureParser("<T, U, V>");
            s.MoveNext();
            var data = s.ParseTypeParametersOpt();
            Assert.HasCount(3, data.Raw);
            Assert.Equal("T", data.Raw[0].Name);
            Assert.Equal("U", data.Raw[1].Name);
            Assert.Equal("V", data.Raw[2].Name);

            var gp = data.ConvertToGenerics(method);
            Assert.Equal(0, gp[0].Position);
            Assert.Equal(1, gp[1].Position);
            Assert.Equal(2, gp[2].Position);
            Assert.Equal(method, gp[0].IsMethodGenericParameter);
            Assert.Equal(method, gp[1].IsMethodGenericParameter);
            Assert.Equal(method, gp[2].IsMethodGenericParameter);
            Assert.True(data.CouldBeParameters);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ParseTypeParametersOpt_should_treat_names_as_positional(bool method) {
            var s = new SignatureParser("<,,>");
            s.MoveNext();
            var data = s.ParseTypeParametersOpt();
            Assert.HasCount(3, data.Raw);

            var gp = data.ConvertToGenerics(method);
            Assert.Equal(0, gp[0].Position);
            Assert.Equal(1, gp[1].Position);
            Assert.Equal(2, gp[2].Position);
            Assert.Equal(method, gp[0].IsMethodGenericParameter);
            Assert.Equal(method, gp[1].IsMethodGenericParameter);
            Assert.Equal(method, gp[2].IsMethodGenericParameter);
            Assert.True(data.CouldBeParameters);
        }

        [Fact]
        public void ParseTypeParametersOpt_should_handle_empty_list() {
            var s = new SignatureParser("<>");
            s.MoveNext();
            var result = s.ParseTypeParametersOpt();
            var data = result.Raw;
            Assert.HasCount(1, data);
            Assert.Null(data[0]);
            Assert.True(result.MustBeParameters);
        }

        [Theory]
        [InlineData("S.T")]
        [InlineData("S*")]
        [InlineData("S&")]
        [InlineData("[A, Version=1.0] S")]
        public void ParseTypeParametersOpt_should_imply_types_not_arguments_when_qualified(string text) {
            var s = new SignatureParser("<" + text + ", U, V>");
            s.MoveNext();
            var data = s.ParseTypeParametersOpt();
            Assert.IsNotInstanceOf(typeof(GenericParameterName), data.Raw[0]);
            Assert.Equal(TypeName.Parse(text), data.Raw[0]);
            Assert.False(data.CouldBeParameters);
        }

        [Theory]
        [InlineData("N.T+U")]
        [InlineData("N.T/U")]
        [InlineData("N.T.U")]
        [InlineData("N")]
        [InlineData(".ctor")]
        [InlineData(".cctor")]
        [InlineData("N..ctor")]
        [InlineData("N`2")]
        [InlineData("S.N`2")]
        [InlineData("S.T`2.N`4.Z")]
        public void ParseQualifiedName_should_handle_various(string text) {
            var s = new SignatureParser(text);
            s.MoveNext();
            var data = s.ParseQualifiedName();

            Assert.Equal(text, data);
        }

    }
}
