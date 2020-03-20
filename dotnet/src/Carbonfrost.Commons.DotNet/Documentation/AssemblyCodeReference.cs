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

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class AssemblyCodeReference : ValidCodeReference {

        internal AssemblyCodeReference(string original, AssemblyName name)
            : base(original, name) {}

        public static CodeReference ParseAssembly(string text) {
            AssemblyName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result)) {
                return new InvalidCodeReference(SymbolType.Property, text);
            }

            return new AssemblyCodeReference(text, result);
        }

        static bool TryParseHelper(string text, out AssemblyName result) {
            return AssemblyName.TryParse(text, out result);
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Assembly;
            }
        }

        protected override string CanonicalString {
            get {
                AssemblyName name = (AssemblyName) this.MetadataName;
                return name.FullName;
            }
        }

    }

}
