//
// Copyright 2013, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class MethodCodeReference : ValidCodeReference {

        public MethodCodeReference(string s, MethodName name)
            : base(s, name) {}

        public static ParameterData[] SplitParameters(string parametersText, MethodName context) {
            return SplitParameters(parametersText, GenericNameContext.Create(context));
        }

        public static ParameterData[] SplitParameters(string parametersText, GenericNameContext context) {
            if (parametersText.Length == 0) {
                return Empty<ParameterData>.Array;
            }

            var list = new List<ParameterData>();

            foreach (string t in SplitParametersInternal(parametersText)) {
                if (string.IsNullOrWhiteSpace(t)) {
                    list.Add(new ParameterData(string.Empty, null));
                    continue;
                }

                string typeName = t.Trim();
                var targ = string.IsNullOrEmpty(typeName) ? null : TypeCodeReference.ParseTypeName(typeName, context);
                list.Add(new ParameterData(null, targ));
            }

            return list.ToArray();
        }

        public static CodeReference ParseMethod(string text) {
            MethodName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result)) {
                return new InvalidCodeReference(SymbolType.Field, text);
            }
            
            return new MethodCodeReference(text, result);
        }

        static bool TryParseHelper(string text, out MethodName result) {
            result = default(MethodName);
            text = text.Trim();

            string name, declaring;
            string myReturnType = null;
            string parameters;

            // LAMESPEC - though only op_Explicit and op_Implict are meant to be encoded
            // using the tilde, we have observed it is uncommon. It is more common to encode 
            // using the `to` syntax.  We also allow this return type tilde syntax to be used
            // on _any_ method though .NET doesn't consider return type in method signatures
            int tilde = text.LastIndexOf('~');
            if (tilde >= 0) {
                myReturnType = text.Substring(tilde + 1);
                text = text.Substring(0, tilde);
            }

            name = SplitMemberName(text, out declaring, out parameters);
            if (parameters.Length > 0 && parameters.Last() != ')' && parameters[0] != '(')
                return false;

            if (parameters.Length > 0) {
                parameters = parameters.Substring(1, parameters.Length - 2).Trim();
            }

            int mangle;
            string rawName = SplitRawNameFromMangle(name, out mangle);
            TypeName rt;
            ParameterData[] pms;            
            TypeName type = null;
            if (declaring.Length == 0 || TypeName.TryParse(declaring, out type)) {
                DefaultMethodName s = new DefaultMethodName(type, rawName);
                s.FinalizeGenerics(mangle);
                HelperParseParametersAndReturnType(s, myReturnType, parameters, out pms, out rt);
                s.FinalizeReturnType(rt);

                result = s;
                s.FinalizeParameters(pms);
                return true;
            }

            return false;
        }

        static void HelperParseParametersAndReturnType(DefaultMethodName s, string myReturnType, string parameters,
            out ParameterData[] pms, out TypeName returnType) {
            pms = null;
            returnType = null;

            if (myReturnType == null && s.Name == "op_Explicit" || s.Name == "op_Implicit") {
                var toSyntaxParams = Regex.Split(parameters, @"\s+to\s+");
                if (toSyntaxParams.Length == 2) {
                    returnType = TypeCodeReference.ParseTypeName(toSyntaxParams[1], s);
                    pms = MethodCodeReference.SplitParameters(toSyntaxParams[0], s);
                    return;
                }
            }
            if (myReturnType != null) {
                returnType = TypeCodeReference.ParseTypeName(myReturnType, s);
            }

            pms = MethodCodeReference.SplitParameters(parameters, s);
        }

        static string SplitRawNameFromMangle(string name, out int mangle) {
            var result = MethodName.StripMangle(name, out mangle);
            return result;
        }

        protected override string CanonicalString {
            get {
                return CodeReference.FORMAT.Format(this.MetadataName);
            }
        }

        public override SymbolType SymbolType {
            get { return SymbolType.Method; } 
        }

        internal static string FormatMethod(MethodName name) {
            StringBuilder sb = new StringBuilder();
            AppendBaseMemberName(name, sb);

            if (name.GenericParameterCount > 0) {
                sb.Append("``").Append(name.GenericParameterCount);
            }

            if (name.GenericArguments.Count > 0) {
                sb.Append('{');
                bool needComma = false;
                foreach (var s in name.GenericArguments) {
                    if (needComma) {
                        sb.Append(",");
                    }

                    sb.Append(s.Name);
                    needComma = true;
                }
                sb.Append('}');
            }

            sb.Append('(');
            AppendParameters(name.Parameters, sb);
            sb.Append(')');

            if (name.ReturnType != null)
                sb.Append("~").Append(FormatAny(name.ReturnType));

            return sb.ToString();
        }

        internal static void SplitParameterName(string t, out string paramName, out string typeName) {
            int index = t.IndexOf(':');
            if (index < 0) {
                LAReader lar = new LAReader(t);
                lar.MoveNext();
                typeName = lar.EatUntilWithBracketCounting(' ').Trim();
                paramName = lar.Rest.Trim();

            } else {

                // param:type
                paramName = t.Substring(0, index).Trim();
                typeName = t.Substring(index + 1).Trim();
            }
        }

        internal static IEnumerable<string> SplitParametersInternal(string parameters) {
            LAReader lar = new LAReader(parameters);
            int depth = 0;
            if (lar.MoveNext()) {
                do {
                    // TODO Throw on unbalanced brackets
                    switch (lar.Current) {
                        case '[':
                        case '(':
                        case '{':
                        case '<':
                            depth++;
                            break;

                        case ']':
                        case ')':
                        case '}':
                        case '>':
                            depth--;
                            break;
                    }

                    while (lar.Current == ',' && depth == 0) {
                        yield return lar.StopEating().Trim();
                        if (!lar.MoveNext()) {
                            goto exit;
                        }
                    }

                } while (lar.Eat());
            }

        exit:
            yield return lar.StopEating().Trim();
        }

    }

}
