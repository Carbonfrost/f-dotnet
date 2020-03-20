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
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet.Documentation {

    abstract class ValidCodeReference : CodeReference {

        private string original;
        private MetadataName name;

        protected ValidCodeReference(string original, MetadataName name) {
            this.original = original;
            this.name = name;
        }

        public sealed override MetadataName MetadataName {
            get { return name; } }

        public sealed override string OriginalString {
            get { return original; } }

        public sealed override CodeReferenceType ReferenceType {
            get { return CodeReferenceType.Valid; } }

        protected abstract string CanonicalString { get; }

        public sealed override string ToString() {
            return CodeReferenceHelper.GetReferenceTypeSpecifierChar(SymbolType)
                + ":" + CanonicalString;
        }

        internal static void AppendBaseMemberName(MemberName name, StringBuilder sb) {
            if (name.DeclaringType != null) {
                sb.Append(FormatAny(name.DeclaringType)).Append(".");
            }

            string text = name.Name;
            MatchEvaluator me = m =>
                ("{" + GetTypeParamNamesCore(m) + "}");
            text = Regex.Replace(text, @"`(\d)", me);

            sb.Append(text.Replace('.', '#'));
        }

        private static string GetTypeParamNamesCore(Match m) {
            return string.Join(
                "@", GetTypeParamNames(Int32.Parse(m.Groups[1].Value)));
        }

        private static IEnumerable<string> GetTypeParamNames(int count) {
            if (count == 1) {
                yield return "T";
            } else {
                for (int i = 1; i <= count; i++) {
                    yield return ("T" + i);
                }
            }
        }

        internal static string FixupExplicitInterface(string text) {
            // System#Collections#Generic#IDictionary{TKey@TValue}
            //  ==> System.Collections.Generic.IDictionary`2
            MatchEvaluator me = m => ("`" + m.Value.Split('@').Length);
            text = Regex.Replace(text, "{.+}", me);

            return text.Replace("#", ".");
        }

        internal static string SplitMemberName(string text, out string declaring, out string parameters) {
            int lparen = text.LastIndexOf('(');
            string result;

            if (lparen < 0) {
                // Could have no declaring
                parameters = string.Empty;

                int dot = text.LastIndexOf('.');
                if (dot < 0) {
                    declaring = string.Empty;
                    result = text;

                } else {
                    declaring = text.Substring(0, dot);
                    result = text.Substring(dot +  1);
                }

            } else {

                int dot = text.LastIndexOf('.', lparen);
                parameters = text.Substring(lparen);

                if (dot < 0) {
                    declaring = string.Empty;
                    result = text.Substring(0, lparen);

                } else {
                    declaring = text.Substring(0, dot);
                    result = text.Substring(dot +  1, lparen - dot - 1);
                }
            }

            return FixupExplicitInterface(result);
        }

        internal static bool TryParseSimpleMember<T>(
            string text, out T result, Func<TypeName, string, TypeName, T> func) {

            Func<TypeName, string, TypeName, T> realThunk
                = (a, b, c) => (func(a, FixupExplicitInterface(b), c));

            return TypeName.ParseSimpleMember(
                text, out result, realThunk) == null;
        }

        internal static void AppendParameters(IEnumerable<ParameterName> parameters,
                                              StringBuilder sb) {
            bool comma = false;

            foreach (var t in parameters) {
                if (comma)
                    sb.Append(',');

                sb.Append(CodeReference.FormatAny(t.ParameterType));
                comma = true;
            }
        }

    }
}
