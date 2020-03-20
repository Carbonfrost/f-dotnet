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
using System.Text;

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class FieldCodeReference : ValidCodeReference {

        internal FieldCodeReference(string original, FieldName name)
            : base(original, name) {}

        public static CodeReference ParseField(string text) {
            FieldName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result))
                return new InvalidCodeReference(SymbolType.Field, text);
            else
                return new FieldCodeReference(text, result);
        }

        static bool TryParseHelper(string name, out FieldName result) {
            return FieldName.TryParse(name, out result);
        }

        public override SymbolType SymbolType {
            get { return SymbolType.Field; } }

        protected override string CanonicalString {
            get {
                var name = (FieldName) this.MetadataName;
                StringBuilder sb = new StringBuilder();

                AppendBaseMemberName(name, sb);
                return sb.ToString();
            }
        }

    }
}
