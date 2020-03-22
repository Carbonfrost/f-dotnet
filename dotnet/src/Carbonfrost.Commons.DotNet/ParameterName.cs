//
// Copyright 2013, 2015, 2017, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
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
using System.Reflection;

namespace Carbonfrost.Commons.DotNet {

    public abstract class ParameterName : MetadataName {

        public abstract int Position { get; }

        public sealed override SymbolType SymbolType {
            get {
                return Position == -1 ? SymbolType.ReturnParameter : SymbolType.Parameter;
            }
        }

        public bool IsReturnParameter {
            get {
                return Position == -1;
            }
        }

        public bool IsIndexerParameter {
            get {
                return DeclaringProperty != null;
            }
        }

        public bool IsByReference {
            get {
                if (ParameterType == null) {
                    return false;
                }
                return ParameterType.IsByReference;
            }
        }

        public MethodName DeclaringMethod {
            get {
                return Member as MethodName;
            }
        }

        public PropertyName DeclaringProperty {
            get {
                return Member as PropertyName;
            }
        }

        public abstract TypeName ParameterType { get; }
        internal abstract MemberName Member { get; }

        public override string FullName {
            get {
                return Name;
            }
        }

        internal abstract ModifierCollection Modifiers { get; }

        public IReadOnlyCollection<TypeName> RequiredModifiers {
            get {
                return Modifiers.RequiredModifiers;
            }
        }

        public IReadOnlyCollection<TypeName> OptionalModifiers {
            get {
                return Modifiers.OptionalModifiers;
            }
        }

        public static ParameterName FromParameterInfo(ParameterInfo parameter) {
            if (parameter == null) {
                throw new ArgumentNullException(nameof(parameter));
            }
            var member = MemberName.FromMemberInfo(parameter.Member);
            switch (member) {
                case MethodName method:
                    return method.Parameters[parameter.Position];
                case PropertyName prop:
                    return prop.IndexParameters[parameter.Position];
            }
            return null;
        }

        internal static bool MatchHelper(INameWithParameters name, INameWithParameters other) {
            // -1 is for properties and means there are knowably parameters but
            // not known how many
            if (name.ParameterCount == -1) {
                return true;
            }
            if (other.ParameterCount == -1) {
                return name.ParameterCount != 0;
            }
            if (name.ParameterCount != other.ParameterCount) {
                return false;
            }

            return name.Parameters.Zip(other.Parameters,
                                           (t, u) => StaticMatches(t, u)).AllTrue();
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.SymbolType == SymbolType.Parameter && Matches((ParameterName) name);
        }

        public bool Matches(ParameterName name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            // TODO Sentinel matching could impact this (uncommon)
            return StaticMatches(this, name);
        }

        static bool StaticMatches(ParameterName self, ParameterName name) {
            return self.Position == name.Position
                && TypeName.SafeMatch((TypeName) self.ParameterType, (TypeName) name.ParameterType)
                && self.Modifiers.Match(name.Modifiers);
        }

        // object overrides
        public override string ToString() {
            return ToString("N", null);
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            if (IsReturnParameter) {
                throw new NotImplementedException();
            }
            return formatter.FormatParameter(format, this, provider);
        }

    }
}
