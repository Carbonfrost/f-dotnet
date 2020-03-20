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

    class TypeCodeReference : ValidCodeReference {

        public TypeCodeReference(string original, TypeName name)
            : base(original, name) {}

        public static CodeReference ParseType(string text) {
            TypeName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result)) {
                return new InvalidCodeReference(SymbolType.Type, text);
            }

            return new TypeCodeReference(text, result);
        }

        static bool TryParseHelper(string text, out TypeName result) {
            result = ParseTypeName(text, null);
            return result != null;
        }

        protected override string CanonicalString {
            get { return FORMAT.Format(this.MetadataName); } }

        public override SymbolType SymbolType {
            get { return SymbolType.Type; } }

        internal static TypeName ParseTypeName(string typeName, MethodName context) {
            return ParseTypeName(typeName, GenericNameContext.Create(context));
        }

        internal static TypeName ParseTypeName(string typeName, GenericNameContext context) {
            return new TypeCodeReferenceParser(typeName, context).Parse();
        }

    }

}
