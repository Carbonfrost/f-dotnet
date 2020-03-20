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

    public class PropertyNameTests {

        [InlineData("Chars", PropertyNameComponents.Name)]
        [InlineData("Chars:String", PropertyNameComponents.Name | PropertyNameComponents.PropertyType)]
        [InlineData("String.Chars:String", PropertyNameComponents.Name | PropertyNameComponents.PropertyType
            | PropertyNameComponents.DeclaringType)]
        [InlineData("Chars[name:]", PropertyNameComponents.Name | PropertyNameComponents.IndexParametersSpecified
            | PropertyNameComponents.IndexParameterNames)]
        [InlineData("Chars[String]", PropertyNameComponents.Name | PropertyNameComponents.IndexParametersSpecified
            | PropertyNameComponents.IndexParameterTypes)]
        [InlineData("Chars[:Type]:String", PropertyNameComponents.Name
            | PropertyNameComponents.IndexParametersSpecified | PropertyNameComponents.IndexParameterTypes
            | PropertyNameComponents.PropertyType)]
        public void Components(string text, PropertyNameComponents expected) {
            Assert.Equal(expected, PropertyName.Parse(text).Components);
        }

        [Theory]
        [InlineData("Chars", "get_Chars()")]
        [InlineData("Chars:String", "get_Chars():String")]
        [InlineData("Chars[Int32]", "get_Chars(Int32)")]
        [InlineData("Chars[Int32, Int32]", "get_Chars(Int32, Int32)")]
        public void GetMethod_should_generate_from_property_or_indexer(string name, string expected) {
            var pn = PropertyName.Parse(name);
            var getter = MethodName.Parse(expected);
            Assert.Equal(getter, pn.GetMethod);
        }

        [Theory]
        [InlineData("Chars")]
        [InlineData("String.Chars")]
        public void IsIndexer_should_be_false_for_simple_names(string name) {
            var pn = PropertyName.Parse(name);
            Assert.False(pn.IsIndexer);
        }

        [Theory]
        [InlineData("Chars[]")]
        [InlineData("String.Chars[]")]
        public void IsIndexer_should_be_true_for_indexer(string name) {
            var pn = PropertyName.Parse(name);
            Assert.True(pn.IsIndexer);
        }

        [Theory]
        [InlineData("System.String.Chars[int]", true)]
        [InlineData("System.String.Chars[]", true)]
        [InlineData("System.String.Length", false)]
        public void IsIndexer_should_be_implied_by_parameters(string name, bool expected) {
            var pn = PropertyName.Parse(name);
            Assert.Equal(expected, pn.IsIndexer);
        }

        [Theory]
        [InlineData("Chars", "set_Chars(value:)")]
        [InlineData("Chars:String", "set_Chars(value:String)")]
        [InlineData("Chars[Int32]", "set_Chars(:Int32, value:)")]
        [InlineData("Chars[Int32, Int32]", "set_Chars(:Int32, :Int32, value:)")]
        public void SetMethod_should_generate_from_property_or_indexer(string name, string expected) {
            var pn = PropertyName.Parse(name);
            var setter = MethodName.Parse(expected);
            Assert.Equal(setter, pn.SetMethod);
        }
    }
}
