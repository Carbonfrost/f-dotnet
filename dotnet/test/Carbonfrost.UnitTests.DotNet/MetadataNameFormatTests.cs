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
using System.Reflection;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class MetadataNameFormatTests {

        [Fact]
        public void Format_use_simple_parameter_type_names() {
            MethodName m = MethodName.FromMethodInfo(typeof(string).GetTypeInfo().GetMethod("CopyTo"));

            MetadataNameFormat format = new MetadataNameFormat();
            format.DefaultFormatString[SymbolType.Parameter] = "Cv";

            string expected = "System.String.CopyTo(Int32, Char[], Int32, Int32)";
            Assert.Equal(expected, format.Format(m));
            Assert.Equal(expected, m.ToString(null, format));
        }

        [Fact]
        public void Format_use_simple_parameter_type_names_generic() {
            MethodName m = MethodName.FromMethodInfo(
                typeof(string).GetTypeInfo().GetMethods().Single(t => t.Name == "Concat" && t.IsGenericMethod));

            MetadataNameFormat format = new MetadataNameFormat();
            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeTypeParameters = true;
            format.IncludeTypeConstraints = false;
            format.IncludeVariance = false;

            string expected = "System.String.Concat<T>(IEnumerable<T>)";
            Assert.Equal(expected, format.Format(m));
            Assert.Equal(expected, m.ToString(null, format));
        }

        [Fact]
        public void Format_use_generic_parameter_positions_and_arity() {
            MethodName m = MethodName.FromMethodInfo(
                typeof(string).GetTypeInfo().GetMethods().Single(t => t.Name == "Concat" && t.IsGenericMethod));

            MetadataNameFormat format = new MetadataNameFormat();

            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeTypeParameters = true;
            format.IncludeTypeConstraints = false;
            format.IncludeVariance = false;
            format.UseGenericParameterPositions = true;

            string expected = "System.String.Concat``1(IEnumerable<``0>)";
            Assert.Equal(expected, format.Format(m));
            Assert.Equal(expected, m.ToString(null, format));
        }

        [Fact]
        public void Format_use_unbound_generic_parameter_positions_should_work_with_method_generics() {
            var pmType = TypeName.Create(null, "Func`1").MakeGenericType(MethodName.GenericParameter(1));
            var format = new MetadataNameFormat {
                UseGenericParameterPositions = true,
                IncludeTypeParameters = true
            };
            Assert.Equal("Func<``1>", pmType.ToString(format));
        }

        [Fact]
        public void Format_use_generic_parameter_positions_array() {
            MethodName m = MethodName.FromMethodInfo(
                typeof(Array).GetTypeInfo().GetMethods().Single(t => t.Name == "TrueForAll" && t.IsGenericMethod));

            MetadataNameFormat format = new MetadataNameFormat();

            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeTypeParameters = true;
            format.IncludeTypeConstraints = false;
            format.IncludeVariance = false;
            format.UseGenericParameterPositions = true;

            string expected = "System.Array.TrueForAll``1(``0[], Predicate<``0>)";
            Assert.Equal(expected, format.Format(m));
            Assert.Equal(expected, m.ToString(null, format));
        }

        [Fact]
        public void Format_type_display_name_generic_nested() {
            var type = TypeName.FromType(typeof(Dictionary<,>.KeyCollection));

            MetadataNameFormat format = new MetadataNameFormat();
            format.IncludeTypeParameters = true;

            string expected = "System.Collections.Generic.Dictionary<TKey, TValue>+KeyCollection";
            Assert.Equal(expected, format.Format(type));
            Assert.Equal(expected, type.ToString(null, format));
        }

        [Fact]
        public void Format_type_generic_positions_open_generic_type() {
            var method = MethodName.FromMethodInfo(
                typeof(ICollection<>).GetTypeInfo().GetMethod("Add"));

            MetadataNameFormat format = new MetadataNameFormat();
            format.UseGenericParameterPositions = true;
            format.IncludeTypeParameters = true;

            string expected = "System.Collections.Generic.ICollection`1.Add(`0 item)";
            Assert.Equal(expected, format.Format(method));
            Assert.Equal(expected, method.ToString(null, format));
        }

        [Fact]
        public void Format_use_generic_type_generic_parameter_names() {
            TypeName m = TypeName.FromType(typeof(Tuple<,,,>));
            MetadataNameFormat format = new MetadataNameFormat();

            format.IncludeTypeParameters = true;
            format.IncludeTypeConstraints = false;
            format.IncludeVariance = false;

            string expected = "System.Tuple<T1, T2, T3, T4>";
            Assert.Equal(expected, format.Format(m));
            Assert.Equal(expected, m.ToString(null, format));
        }

        [Fact]
        public void Format_use_generic_parameter_compact_format() {
            TypeName t = TypeName.FromType(typeof(IDictionary<Delegate, int>));

            MetadataNameFormat format = new MetadataNameFormat();

            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeTypeParameters = true;

            string expected = "System.Collections.Generic.IDictionary<Delegate, Int32>";
            Assert.Equal(expected, format.Format(t));
            Assert.Equal(expected, t.ToString(null, format));
        }

        [Fact]
        public void Format_property_name_full_name_indexer_format_compact_parameters() {
            var pi = typeof(string).GetTypeInfo().GetProperty("Chars");
            var pn = PropertyName.FromPropertyInfo(pi);

            MetadataNameFormat format = new MetadataNameFormat();
            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeReturnTypes[SymbolType.Property] = true;

            // Uses the parameter format string for return type
            string expected = "System.String.Chars[Int32]:Char";
            Assert.Equal(expected, format.Format(pn));
            Assert.Equal(expected, pn.ToString(null, format));
        }

        [Fact]
        public void Format_method_name_include_return_types() {
            var mi = typeof(decimal).GetTypeInfo().GetMethods().First(t => t.Name == "op_Explicit");
            var name = MethodName.FromMethodInfo(mi);

            Assert.NotNull(mi.ReturnType);

            MetadataNameFormat format = new MetadataNameFormat();
            format.DefaultFormatString[SymbolType.Parameter] = "Cv";
            format.IncludeReturnTypes[SymbolType.Method] = true;

            // Uses the parameter format string for return type
            string expected = "System.Decimal.op_Explicit(Single):Decimal";
            Assert.Equal(expected, format.Format(name));
            Assert.Equal(expected, name.ToString(null, format));
        }

        [Fact]
        public void Format_method_unspecified_parameters() {
            var mi = typeof(decimal).GetTypeInfo().GetMethods().First(t => t.Name == "op_Explicit");
            var name = MethodName.FromMethodInfo(mi).WithParametersUnspecified();

            string expected = "System.Decimal.op_Explicit";
            var format = new MetadataNameFormat();

            Assert.Equal(expected, format.Format(name));
            Assert.Equal(expected, name.ToString(null, format));
        }
    }
}
