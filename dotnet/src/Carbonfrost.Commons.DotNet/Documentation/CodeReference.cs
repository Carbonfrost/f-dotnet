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
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet.Documentation {

    public abstract class CodeReference {

        internal static readonly MetadataNameFormat FORMAT
            = new CodeReferenceFormatter();

        public abstract string OriginalString { get; }
        public abstract CodeReferenceType ReferenceType { get; }
        public abstract MetadataName MetadataName { get; }
        public abstract SymbolType SymbolType { get; }

        public bool IsValid {
            get { return CodeReferenceType.Valid == ReferenceType; }
        }

        public bool IsUnspecified {
            get { return CodeReferenceType.Unspecified == ReferenceType; }
        }

        public bool IsInvalid {
            get { return CodeReferenceType.Invalid == ReferenceType; }
        }

        internal CodeReference() {}

        public abstract override string ToString();

        public static CodeReference Parse(string text) {
            return Utility.Parse<CodeReference>(text, _TryParse);
        }

        public static bool TryParse(string text, out CodeReference result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out CodeReference result) {
            result = null;
            if (text == null) {
                return new ArgumentNullException("text"); // $NON-NLS-1
            }

            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace("name"); // $NON-NLS-1
            }

            int index = text.IndexOf(':');
            if (index < 0) {
                result = new UnspecifiedCodeReference(text);
                return null;
            }

            string specifier = text.Substring(0, index).Trim();
            string name = text.Substring(index + 1).Trim();

            if (specifier.Length == 1) {
                SymbolType type = CodeReferenceHelper.GetReferenceType(specifier[0]);

                if (type != SymbolType.Unknown) {
                    result = Create(type, name);
                    return result.IsValid ? null : Failure.NotParsable("text", typeof(CodeReference));
                }
            }

            return Failure.NotParsable("text", typeof(CodeReference));
        }

        public static CodeReference Create(SymbolType type,
                                           string text) {
            switch (type) {
                case SymbolType.Event:
                    return Event(text);

                case SymbolType.Field:
                    return Field(text);

                case SymbolType.Method:
                    return Method(text);

                case SymbolType.Namespace:
                    return Namespace(text);

                case SymbolType.Property:
                    return Property(text);

                case SymbolType.Type:
                    return Type(text);

                case SymbolType.Assembly:
                    return Assembly(text);

                case SymbolType.Unknown:
                    return Unspecified(text);

                default:
                    throw Failure.NotDefinedEnum("type", type); // $NON-NLS-1
            }
        }

        public static CodeReference Event(string text) {
            return EventCodeReference.ParseEvent(text);
        }

        public static CodeReference Field(string text) {
            return FieldCodeReference.ParseField(text);
        }

        public static CodeReference Method(string text) {
            return MethodCodeReference.ParseMethod(text);
        }

        public static CodeReference Namespace(string text) {
            return NamespaceCodeReference.ParseNamespace(text);
        }

        public static CodeReference Property(string text) {
            return PropertyCodeReference.ParseProperty(text);
        }

        public static CodeReference Type(string text) {
            return TypeCodeReference.ParseType(text);
        }

        public static CodeReference Assembly(string text) {
            return AssemblyCodeReference.ParseAssembly(text);
        }

        public static CodeReference Unspecified(string name) {
            return new UnspecifiedCodeReference(name ?? string.Empty);
        }

        public static CodeReference Create(MetadataName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            switch (name.SymbolType) {
                case SymbolType.Field:
                    return new FieldCodeReference(null, (FieldName) name);

                case SymbolType.Property:
                    return new PropertyCodeReference(null, (PropertyName) name);

                case SymbolType.Event:
                    return new EventCodeReference(null, (EventName) name);

                case SymbolType.Method:
                    return new MethodCodeReference(null, (MethodName) name);

                case SymbolType.Type:
                    return new TypeCodeReference(null, (TypeName) name);

                case SymbolType.Namespace:
                    return new NamespaceCodeReference(null, (NamespaceName) name);

                case SymbolType.Assembly:
                    return new AssemblyCodeReference(null, (AssemblyName) name);

                case SymbolType.Unknown:
                case SymbolType.Module:
                case SymbolType.Resource:
                case SymbolType.Local:
                case SymbolType.Alias:
                case SymbolType.Attribute:
                case SymbolType.Parameter:
                case SymbolType.InternedLocation:
                default:
                    throw DotNetFailure.NotSupportedCodeReferenceConversion("name");
            }
        }

        public override bool Equals(object obj) {
            CodeReference other = obj as CodeReference;
            if (other == null) {
                return false;
            }

            return this.MetadataName == other.MetadataName;
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                if (MetadataName != null) {
                    hashCode += 1000000009 * MetadataName.GetHashCode();
                }
            }
            return hashCode;
        }

        internal static string FormatAny(MemberName name) {
            return FORMAT.Format(name);
        }

        class CodeReferenceFormatter : MetadataNameFormat {

            protected internal override string FormatMethod(string format, MethodName name, IFormatProvider formatProvider) {
                return MethodCodeReference.FormatMethod(name);
            }

            protected internal override string FormatType(string format, TypeName name, IFormatProvider formatProvider) {
                if (name.DeclaringType != null) {
                    return Format(name.DeclaringType) + "." + name.Name;
                }

                return name.FullName;
            }

            protected internal override string FormatArrayType(string format, ArrayTypeName name, IFormatProvider formatProvider) {
                return Format(name.ElementType) + "[]";
            }

            protected internal override string FormatByReferenceType(string format, ByReferenceTypeName name, IFormatProvider formatProvider) {
                return Format(name.ElementType) + "@";
            }

            protected internal override string FormatGenericInstanceMethod(string format, GenericInstanceMethodName name, IFormatProvider formatProvider) {
                return MethodCodeReference.FormatMethod(name);
            }

            protected internal override string FormatGenericInstanceType(string format, GenericInstanceTypeName name, IFormatProvider formatProvider) {
                string withoutMangle = Regex.Replace(name.ElementType.FullName, @"`\d", string.Empty);
                string parms = string.Join(",", name.GenericArguments.Select(t => t.Name));
                return string.Concat(withoutMangle, "{", parms, "}");
            }

            protected internal override string FormatGenericParameter(string format, GenericParameterName name, IFormatProvider formatProvider) {
                if (name.DeclaringMethod != null) {
                    return "``" + name.Position;
                }

                return "`" + name.Position;
            }

        }
    }
}
