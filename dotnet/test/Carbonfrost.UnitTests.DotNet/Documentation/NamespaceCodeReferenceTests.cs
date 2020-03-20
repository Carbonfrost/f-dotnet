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

    public class NamespaceCodeReferenceTests {

        [Fact]
        public void parse_namespace_name_nominal() {
            var cr = CodeReference.Parse("N:System.Json");
            Assert.True(cr.IsValid);
            Assert.Equal(SymbolType.Namespace, cr.MetadataName.SymbolType);

            var name = (NamespaceName) cr.MetadataName;
            Assert.Equal("System.Json", name.FullName);
        }
    }
}
