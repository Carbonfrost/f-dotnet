//
// Copyright 2013, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public sealed class NamespaceName : MetadataName, IComparable<NamespaceName>, IEquatable<NamespaceName> {

        private readonly string text;
        private static readonly Regex PATTERN
            = new Regex(@"\w+(\.\w+)*");

        public static readonly NamespaceName Default = new NamespaceName(string.Empty);

        public bool IsDefault {
            get { return this.FullName == string.Empty; }
        }

        public NamespaceName ParentNamespace {
            get {
                if (this.text.Length == 0) {
                    return null;
                }

                int index = text.LastIndexOf('.');
                if (index < 0) {
                    return Default;
                }

                return new NamespaceName(text.Substring(0, index));
            }
        }

        internal NamespaceName(string text) {
            this.text = text;
        }

        public TypeName GetType(string name, AssemblyName assembly = null) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            return DefaultTypeName.FromFullName(name, this.FullName, assembly);
        }

        public static NamespaceName Parse(String text) {
            return Utility.Parse<NamespaceName>(text, _TryParse);
        }

        public static bool TryParse(string text, out NamespaceName result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out NamespaceName result) {
            result = null;

            if (text == null) {
                return new ArgumentNullException("text");
            }
            text = text.Trim();

            if (text.Length == 0) {
                result = Default;
                return null;
            }
            if (PATTERN.IsMatch(text)) {
                result = new NamespaceName(text);
                return null;
            }

            return Failure.NotParsable("text", typeof(NamespaceName));
        }

        public override string FullName {
            get { return this.text; } }

        public override string Name {
            get {
                int index = text.LastIndexOf('.');
                if (index < 0) {
                    return text;
                }

                return text.Substring(1 + index);
            }
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Namespace;
            }
        }

        public override bool Equals(object obj) {
            return StaticEquals(this, obj as NamespaceName);
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * text.GetHashCode();
            }
            return hashCode;
        }

        public bool Matches(NamespaceName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            string nx = this.text;
            string ny = name.text;

            if (string.IsNullOrEmpty(nx)) {
                return true;
            }
            if (string.IsNullOrEmpty(ny)) {
                return false;
            }

            return (ny == nx) || ny.EndsWith("." + nx);
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.SymbolType == SymbolType.Namespace && Matches((NamespaceName) name);
        }

        internal static bool SafeMatch(NamespaceName a, NamespaceName b) {
            if (a == null || b == null) {
                return true;
            }
            return a.Matches(b);
        }

        internal override MetadataName AddRight(MetadataName name) {
            if (name.SymbolType == SymbolType.Namespace) {
                return NamespaceName.Parse(this + "." + name.FullName);
            }
            if (name.SymbolType == SymbolType.Type) {
                return ((TypeName) name).WithNamespace(this);
            }
            return base.AddRight(name);
        }

        public static bool operator ==(NamespaceName a, NamespaceName b) {
            return StaticEquals(a, b);
        }

        public static bool operator !=(NamespaceName a, NamespaceName b) {
            return !StaticEquals(a, b);
        }

        // `IComparable' implementation
        public int CompareTo(NamespaceName other) {
            if (other == null) {
                return 1;
            }

            return this.text.CompareTo(other.text);
        }

        // `IEquatable' implementation
        public bool Equals(NamespaceName other) {
            return StaticEquals(this, other);
        }

        static bool StaticEquals(NamespaceName self, NamespaceName other) {
            if (object.ReferenceEquals(self, other)) {
                return true;
            }
            if (object.ReferenceEquals(other, null)) {
                return false;
            }
            if (object.ReferenceEquals(self, null)) {
                return false;
            }

            return self.text == other.text;
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatNamespace(format, this, provider);
        }

    }

}
