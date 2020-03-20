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

    public class AssemblyNameTests {

        [Theory]
        [InlineData("mscorlib", AssemblyNameComponents.Name)]
        [InlineData("mscorlib, PublicKeyToken=6499fd5d06ff8d8d",
            AssemblyNameComponents.Name | AssemblyNameComponents.PublicKeyOrToken)]
        [InlineData("mscorlib, Version=1.0.0.0",
            AssemblyNameComponents.Name | AssemblyNameComponents.Version)]
        [InlineData("mscorlib, PublicKey=, Version=1.0.0.0, Culture=en, Architecture=msil",
            AssemblyNameComponents.PublicKeyOrToken | AssemblyNameComponents.Name | AssemblyNameComponents.Version |
            AssemblyNameComponents.Culture | AssemblyNameComponents.Architecture)]
        public void Components(string text, AssemblyNameComponents expected) {
            Assert.Equal(expected, AssemblyName.Parse(text).Components);
        }

    }
}