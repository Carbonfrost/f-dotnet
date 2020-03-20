//
// Copyright 2013, 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    public abstract class MemberName : MetadataName {

        public TypeName DeclaringType {
            get; private set;
        }

        internal MemberName() {}

        internal MemberName(TypeName declaringType) {
            DeclaringType = declaringType;
        }

        public override string FullName {
            get {
                if (HasDeclaringTypeSpecified) {
                    return string.Concat(DeclaringType.FullName, (Name.Contains(".") ? "::" : "."), Name);
                }
                return Name;
            }
        }

        public bool HasAssemblySpecified {
            get {
                return Assembly != null;
            }
        }

        public bool HasDeclaringTypeSpecified {
            get {
                return DeclaringType != null;
            }
        }

        public virtual AssemblyName Assembly {
            get {
                TypeName tn = this.DeclaringType as TypeName;
                if (tn == null) {
                    return null;
                }
                return tn.Assembly;
            }
        }

        public static MemberName FromMemberInfo(MemberInfo memberInfo) {
            if (memberInfo == null) {
                throw new ArgumentNullException("memberInfo");
            }
            switch (memberInfo.MemberType) {
                case MemberTypes.Constructor:
                    return MethodName.FromConstructorInfo((ConstructorInfo) memberInfo);
                case MemberTypes.Event:
                    return EventName.FromEventInfo((EventInfo) memberInfo);
                case MemberTypes.Field:
                    return FieldName.FromFieldInfo((FieldInfo) memberInfo);
                case MemberTypes.Method:
                    return MethodName.FromMethodInfo((MethodInfo) memberInfo);
                case MemberTypes.Property:
                    return PropertyName.FromPropertyInfo((PropertyInfo) memberInfo);
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return TypeName.FromType(((TypeInfo) memberInfo).AsType());
                case MemberTypes.Custom:
                default:
                    throw new NotImplementedException();
            }
        }

        public MemberName WithAssembly(AssemblyName assembly) {
            return WithAssemblyOverride(assembly);
        }

        public MemberName WithAssemblyUnspecified() {
            return WithAssembly(null);
        }

        public MemberName WithDeclaringType(TypeName declaringType) {
            return WithDeclaringTypeOverride(declaringType);
        }

        public MemberName WithDeclaringTypeUnspecified() {
            return WithDeclaringType(null);
        }

        protected abstract MemberName WithAssemblyOverride(AssemblyName assembly);

        protected abstract MemberName WithDeclaringTypeOverride(TypeName declaringType);

        internal static string SplitMemberName(string text, out string declaring, out string parameters) {
            text = text.Trim();

            string[] results = Regex.Split(text, "::");
            declaring = string.Empty;
            parameters = string.Empty;

            if (results.Length >= 3) {
                return null;

            } else if (results.Length == 2) {
                declaring = results[0];
                return SplitParametersHelper(results[1], out parameters);
            }

            // Bracket counting from end
            // TODO Unmatched brackets?
            int depth = 0;
            int last = text.Length;
            int i = text.Length - 1;
            for (; i >= 0; i--) {
                switch (text[i]) {
                    case '[':
                    case '<':
                    case '(':
                    case '{':
                        depth++;
                        last = i;
                        break;
                    case ']':
                    case '>':
                    case ')':
                    case '}':
                        depth--;
                        break;
                    case '.':
                        if (depth == 0) {
                            goto exit;
                        }

                        break;
                }
            }

        exit:
            if (depth != 0) {
                return null;
            }

            if (i >= 0) {
                string result;
                declaring = text.Substring(0, i);
                result = text.Substring(i + 1, last - i - 1);
                parameters = text.Substring(last).Trim();
                return result;

            } else {
                declaring = string.Empty;
                string result = text.Substring(0, last);
                parameters = text.Substring(last).Trim();
                return result;
            }
        }

        static string SplitParametersHelper(string text,
                                            out string parameters)
        {
            for (int i = text.Length - 1, depth = 0; i >= 0; i--) {
                switch (text[i]) {
                    case '[':
                    case '<':
                    case '(':
                    case '{':
                        depth++;

                        if (depth == 0) {
                            parameters = text.Substring(i);
                            return text.Substring(0, i);
                        }

                        break;
                    case ']':
                    case '>':
                    case ')':
                    case '}':
                        depth--;
                        break;
                }
            }

            parameters = string.Empty;
            return text;
        }
    }
}
