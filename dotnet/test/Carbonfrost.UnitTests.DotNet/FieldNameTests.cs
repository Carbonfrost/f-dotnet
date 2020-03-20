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

    public class FieldNameTests {

        [InlineData("Empty", FieldNameComponents.Name)]
        [InlineData("Empty:String", FieldNameComponents.Name | FieldNameComponents.FieldType)]
        [InlineData("String.Empty:String", FieldNameComponents.Name | FieldNameComponents.FieldType
            | FieldNameComponents.DeclaringType)]
        [Theory]
        public void Components(string text, FieldNameComponents expected) {
            Assert.Equal(expected, FieldName.Parse(text).Components);
        }
    }
}
