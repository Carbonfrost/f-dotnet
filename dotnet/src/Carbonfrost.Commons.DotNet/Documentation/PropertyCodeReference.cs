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
using System.Text;

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class PropertyCodeReference : ValidCodeReference {

        internal PropertyCodeReference(string s, PropertyName name)
            : base(s, name) {}

        public static CodeReference ParseProperty(string name) {
            PropertyName result;

            if (string.IsNullOrWhiteSpace(name) || !TryParseHelper(name, out result)) {
                return new InvalidCodeReference(SymbolType.Property, name);
            }

            return new PropertyCodeReference(name, result);
        }

        static bool TryParseHelper(string text, out PropertyName result) {
            result = default(PropertyName);
            text = text.Trim();

            string name, declaring;
            string parameters;

            name = SplitMemberName(text, out declaring, out parameters);
            if (parameters.Length > 0 && parameters.Last() != ')' && parameters[0] != '(') {
                return false;
            }

            if (parameters.Length > 0) {
                parameters = parameters.Substring(1, parameters.Length - 2).Trim();
            }

            if (declaring.Length == 0) {
                var pms = MethodCodeReference.SplitParameters(parameters, null);
                result = new PropertyName(null, name, null, pms);
                return true;

            } else {
                TypeName type;
                if (TypeName.TryParse(declaring, out type)) {
                    var pms = MethodCodeReference.SplitParameters(parameters, GenericNameContext.Create(type));

                    result = new PropertyName(type, name, null, pms);
                    return true;
                }
            }

            return false;
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Property;
            }
        }


        protected override string CanonicalString {
            get {
                PropertyName name = (PropertyName) this.MetadataName;
                StringBuilder sb = new StringBuilder();
                AppendBaseMemberName(name, sb);

                if (name.Parameters.Count > 0) {
                    sb.Append('(');
                    AppendParameters(name.Parameters, sb);
                    sb.Append(')');
                }

                return sb.ToString();
            }
        }

    }

}
