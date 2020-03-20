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
using System.Text.RegularExpressions;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public class ModuleName : MetadataName {

        private string name;

        internal ModuleName(string name) {
            this.name = name;
        }

        public virtual AssemblyName Assembly { get { return null; } }

        public override string Name {
            get { return this.name; } }

        public override string FullName {
            get { return this.name; } }

        public static ModuleName Parse(string text) {
            ModuleName result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        static Exception _TryParse(string text, out ModuleName result) {
            result = null;
            if (text == null)
                return new ArgumentNullException("text");

            text = text.Trim();
            if (text.Length == 0)
                return Failure.AllWhitespace("text");

            // TODO Verify which characters are considered valid
            if (Regex.IsMatch(text, @"\w+(\.\w+)*")) {
                result = new ModuleName(text);
                return null;
            }

            return Failure.NotParsable("text", typeof(ModuleName));
        }

        public static bool TryParse(string text, out ModuleName result) {
            return _TryParse(text, out result) == null;
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return name.SymbolType == SymbolType.Module && Matches((ModuleName) name);
        }

        public bool Matches(ModuleName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return this.Name == name.Name;
        }

        public override bool Equals(object obj) {
            ModuleName other = obj as ModuleName;
            if (other == null)
                return false;

            return this.name == other.name;
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * name.GetHashCode();
            }
            return hashCode;
        }

        public static bool operator ==(ModuleName lhs, ModuleName rhs) {
            if (ReferenceEquals(lhs, rhs))
                return true;
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null))
                return false;
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ModuleName lhs, ModuleName rhs) {
            return !(lhs == rhs);
        }

        public override SymbolType SymbolType {
            get { return SymbolType.Module; } }

        public ModuleName Update(string name = null) {
            if (name == null || name == this.Name)
                return this;
            else if (name.Trim().Length == 0)
                throw Failure.AllWhitespace("name");
            else
                return new ModuleName(name);
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatModule(format, this, provider);
        }

    }
}
