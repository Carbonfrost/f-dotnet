//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class FieldNameTests {

        [InlineData("Empty", FieldNameComponents.Name)]
        [InlineData("Empty:String", FieldNameComponents.Name | FieldNameComponents.FieldType)]
        [InlineData("String.Empty:String", FieldNameComponents.Name | FieldNameComponents.FieldType
            | FieldNameComponents.DeclaringType)]
        [Theory]
        public void Components(string text, FieldNameComponents expected) {
            Assert.Equal(expected, FieldName.Parse(text).Components);
        }

        [Fact]
        public void Create_should_create_method_name_from_name() {
            var a = FieldName.Create("Hello");
            var b = FieldName.Parse("Hello");
            Assert.Equal(a, b);
        }

        [Fact]
        public void Create_should_create_method_name_from_name_and_type() {
            var a = FieldName.Create("Hello", TypeName.Create("System", "String"));
            var b = FieldName.Parse("Hello:System.String");
            Assert.Equal(a, b);
        }
    }
}
