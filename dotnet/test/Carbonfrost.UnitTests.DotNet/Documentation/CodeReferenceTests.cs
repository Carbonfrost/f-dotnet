//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class CodeReferenceTests {

        [Fact, ExpectedException(typeof(ArgumentException))]
        public void invalid_parse_from_empty_string() {
            CodeReference.Parse("");
        }

        [Fact, ExpectedException(typeof(ArgumentNullException))]
        public void invalid_parse_from_null_string() {
            CodeReference.Parse(null);
        }

        [Fact]
        public void invalid_try_parse_from_empty_string() {
            CodeReference cr;
            Assert.False(CodeReference.TryParse("", out cr));
        }

        [Fact]
        public void invalid_try_parse_from_null_string() {
            CodeReference cr;
            Assert.False(CodeReference.TryParse(null, out cr));
        }

        [Fact]
        public void invalid_try_parse_from_invalid_type_string() {
            CodeReference cr;
            Assert.False(CodeReference.TryParse("T:{}", out cr));
            Assert.False(CodeReference.TryParse("T:Type.", out cr));
            Assert.False(CodeReference.TryParse("T:[]", out cr));
            Assert.False(CodeReference.TryParse("T:Team^", out cr));
        }
    }
}
