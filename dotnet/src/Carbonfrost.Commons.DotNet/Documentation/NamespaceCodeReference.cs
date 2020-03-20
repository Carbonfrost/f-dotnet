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

    sealed class NamespaceCodeReference : ValidCodeReference {

        internal NamespaceCodeReference(string original, NamespaceName name)
            : base(original, name) {}

        public static CodeReference ParseNamespace(string text) {
            NamespaceName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result))
                return new InvalidCodeReference(SymbolType.Field, text);
            else
                return new NamespaceCodeReference(text, result);
        }

        static bool TryParseHelper(string name, out NamespaceName result) {
            return NamespaceName.TryParse(name, out result);
        }

        public override SymbolType SymbolType {
            get { return SymbolType.Namespace; } }

        protected override string CanonicalString {
            get { return this.MetadataName.FullName; } }

    }

}
