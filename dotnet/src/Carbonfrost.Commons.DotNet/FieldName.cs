//
// Copyright 2013, 2016, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.DotNet {

    public sealed partial class FieldName : MemberName {

        private readonly string _name;

        internal FieldName(TypeName declaringType,
                           string name,
                           TypeName fieldType) : base(declaringType) {
            _name = name;
            FieldType = fieldType;
        }

        public static FieldName FromFieldInfo(FieldInfo fieldInfo) {
            if (fieldInfo == null) {
                throw new ArgumentNullException("fieldInfo");
            }

            return new FieldName(TypeName.FromType(fieldInfo.DeclaringType),
                                 fieldInfo.Name,
                                 TypeName.FromType(fieldInfo.FieldType));
        }

        public FieldNameComponents Components {
            get {
                var result = FieldNameComponents.Name;
                if (DeclaringType != null) {
                    result |= FieldNameComponents.DeclaringType;
                }
                if (FieldType != null) {
                    result |= FieldNameComponents.FieldType;
                }
                return result;
            }
        }

        public TypeName FieldType {
            get;
            private set;
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Field;
            }
        }

        public static FieldName Create(string name) {
            return new FieldName(null, name, null);
        }

        public static FieldName Create(string name, TypeName fieldType) {
            return new FieldName(null, name, fieldType);
        }

        public static bool TryParse(string text, out FieldName result) {
            return _TryParse(text, out result) == null;
        }

        public static FieldName Parse(string text) {
            return Utility.Parse<FieldName>(text, _TryParse);
        }

        static Exception _TryParse(string text, out FieldName result) {
            return TypeName.ParseSimpleMember(
                text, out result, (a, b, c) => (new FieldName(a, b, c)));
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            return new FieldName(declaringType, _name, FieldType);
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return WithDeclaringType(DeclaringType.WithAssembly(assembly));
        }

        public override string FullName {
            get {
                if (this.DeclaringType == null) {
                    return Name;
                }

                return string.Concat(DeclaringType, ".", Name);
            }
        }

        public override string Name {
            get { return _name; } }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatField(format, this, provider);
        }

        public bool Matches(FieldName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.Name == Name
                && TypeName.SafeMatch(DeclaringType, name.DeclaringType)
                && TypeName.SafeMatch(FieldType, name.FieldType);
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return name.SymbolType == SymbolType.Field && Matches((FieldName) name);
        }
    }

}
