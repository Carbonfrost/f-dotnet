//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class MethodNameMatchesTests {

        [Fact]
        public void Matches_should_apply_to_byref_parameters() {
            var method1 = MethodName.Parse("System.UInt32.TryParse(String, UInt32&)");
            var method2 = MethodName.Parse("System.UInt32.TryParse(String, UInt32&)");
            Assert.True(method1.Equals(method2));
            Assert.True(method1.Matches(method2));
        }

        [Theory]
        [InlineData("TryParse")]
        [InlineData("TryParse(String, UInt32&)")]
        [InlineData("UInt32.TryParse(String, UInt32&)")]
        public void Matches_should_apply_to_all_less_specific_names(string text) {
            var method1 = MethodName.Parse(text);
            var name = MethodName.Parse("System.UInt32.TryParse(String, UInt32&)");
            Assert.True(method1.Matches(name));
        }


        [Fact, Skip("Can't yet parse complex method names")]
        public void Matches_explicit_interface_name() {
            var subject = MethodName.Parse("System.Tuple`2::System.ITuple.ToString()");
            Assert.True(MethodName.Parse("Tuple`2::System.ITuple.ToString()").Matches(subject));

            // Doesn't match because it is considered "ToString()"
            Assert.False(MethodName.Parse("System.ITuple.ToString()").Matches(subject));
        }

        [Fact]
        public void Matches_method_without_parameters() {
            var subject = MethodName.Parse("System.Object.ToString()");
            Assert.True(MethodName.Parse("Object.ToString()").Matches(subject));
        }

        [Fact]
        public void Matches_method_generic_parameter_count() {
            var subject = MethodName.Parse("System.Array.ConvertAll<TInput,TOutput>(TInput[], Converter<TInput, TOutput>)");
            Assert.True(MethodName.Parse("System.Array.ConvertAll<TInput,TOutput>(TInput[], Converter<TInput, TOutput>)").Matches(subject));
            Assert.True(MethodName.Parse("ConvertAll<TInput,TOutput>(TInput[], Converter<TInput, TOutput>)").Matches(subject));
            Assert.False(MethodName.Parse("ConvertAll(TInput[], Converter<TInput, TOutput>)").Matches(subject));
        }

    }

}