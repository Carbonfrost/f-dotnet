//
// Copyright 2013, 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public sealed partial class PropertyName : MemberName {

        private readonly string _name;
        private readonly ParameterNameCollection _parameters;

        internal PropertyName(TypeName declaringType,
                              string name,
                              TypeName propertyType,
                              ParameterData[] parameters) : base(declaringType) {
            _name = name;
            PropertyType = propertyType;
            _parameters = ParameterData.ToArray(this, parameters)
                ?? ParameterNameCollection.Empty;
        }

        internal PropertyName(TypeName declaringType,
                              string name,
                              TypeName propertyType,
                              ParameterNameCollection parameters) : base(declaringType) {
            _name = name;
            PropertyType = propertyType;
            _parameters = parameters ?? ParameterNameCollection.Empty;
        }

        public PropertyNameComponents Components {
            get {
                var result = PropertyNameComponents.Name;
                if (DeclaringType != null) {
                    result |= PropertyNameComponents.DeclaringType;
                }
                if (PropertyType != null) {
                    result |= PropertyNameComponents.PropertyType;
                }
                if (IsIndexer) {
                    result |= PropertyNameComponents.IndexParametersSpecified;
                    if (IndexParameterCount == 0 || IndexParameters.Any(y => y.ParameterType != null)) {
                        result |= PropertyNameComponents.IndexParameterTypes;
                    }
                    if (IndexParameterCount == 0 || IndexParameters.Any(y => !string.IsNullOrEmpty(y.Name))) {
                        result |= PropertyNameComponents.IndexParameterNames;
                    }
                }
                return result;
            }
        }

        public MethodName GetMethod {
            get {
                return GetGetMethod();
            }
        }

        public MethodName SetMethod {
            get {
                return GetSetMethod();
            }
        }

        public bool IsIndexer {
            get {
                return IndexParameterCount > 0;
            }
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Property;
            }
        }

        public int IndexParameterCount {
            get {
                return IndexParameters.Count;
            }
        }

        public TypeName PropertyType {
            get; private set;
        }

        public ParameterNameCollection IndexParameters {
            get {
                return _parameters;
            }
        }

        private MethodName GetGetMethod() {
            string name = "get_" + Name;

            var result = new DefaultMethodName(DeclaringType, name, PropertyType);
            var parameters = ParameterData.AllFromTypes(IndexParameters.Select(t => t.ParameterType));
            result.FinalizeParameters(parameters);
            return result;
        }

        private MethodName GetSetMethod() {
            string name = "set_" + Name;

            var result = new DefaultMethodName(DeclaringType, name, TypeName.Void);
            var parameters = new ParameterData[IndexParameters.Count + 1];
            parameters[parameters.Length - 1] = new ParameterData("value", PropertyType);
            var indexes = ParameterData.AllFromTypes(IndexParameters.Select(t => t.ParameterType));
            indexes.CopyTo(parameters, 0);

            result.FinalizeParameters(parameters);
            return result;
        }

        public static PropertyName Parse(string text) {
            return Utility.Parse<PropertyName>(text, _TryParse);
        }

        public static bool TryParse(string text, out PropertyName result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out PropertyName result) {
            result = default(PropertyName);
            if (text == null) {
                return new ArgumentNullException("text");
            }

            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace("text");
            }

            try {
                result = new SignatureParser(text).ParseProperty();
            } catch {
            }
            if (result == null) {
                return Failure.NotParsable("text", typeof(PropertyName));
            }
            return null;
        }

        public override string Name {
            get {
                return _name;
            }
        }

        public override string FullName {
            get {
                if (IsIndexer) {
                    return MetadataNameFormat.FullNameFormat.FormatProperty(null, this, null);
                }

                return base.FullName;
            }
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return name.SymbolType == SymbolType.Property && Matches((PropertyName) name);
        }

        public bool Matches(PropertyName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            bool matchesParameters = IndexParameters.Zip(name.IndexParameters,
                                        (t, u) => t.Matches(u)).AllTrue();
            return name.Name == this.Name
                && TypeName.SafeMatch(this.DeclaringType, name.DeclaringType)
                && matchesParameters;
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatProperty(format, this, provider);
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            return new PropertyName(declaringType, _name, PropertyType, IndexParameters);
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return WithDeclaringType(DeclaringType.WithAssembly(assembly));
        }

        public override bool Equals(object other) {
            var m = other as PropertyName;
            if (m != null) {
                return FullName == m.FullName;
            }
            return false;
        }

        public override int GetHashCode() {
            return FullName.GetHashCode();
        }

        public static PropertyName FromPropertyInfo(System.Reflection.PropertyInfo property) {
            if (property == null) {
                throw new ArgumentNullException("property");
            }

            var declaringType = TypeName.FromType(property.DeclaringType);
            var propertyType = TypeName.FromType(property.PropertyType);
            var parms = ParameterData.ConvertAll(property.GetIndexParameters());

            return new PropertyName(declaringType, property.Name, propertyType, parms);
        }

        public static PropertyName Create(string name) {
            return new PropertyName(null, name, null, ParameterNameCollection.Empty);
        }

        public static PropertyName Create(string name, TypeName propertyType) {
            return new PropertyName(null, name, propertyType, ParameterNameCollection.Empty);
        }
    }
}
