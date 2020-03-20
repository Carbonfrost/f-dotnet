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
using System.Linq;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public abstract class MetadataName : IFormattable {

        public abstract string FullName { get; }
        public abstract string Name { get; }

        public abstract SymbolType SymbolType { get; }

        public override bool Equals(object obj) {
            MetadataName other = obj as MetadataName;
            if (other == null) {
                return false;
            }

            return this.FullName == other.FullName;
        }

        public override int GetHashCode() {
            return this.FullName.GetHashCode();
        }

        internal virtual MetadataName AddRight(MetadataName name) {
            throw new NotSupportedException();
        }

        public static MetadataName Combine(params MetadataName[] names) {
            if (names == null) {
                throw new ArgumentNullException("names");
            }
            if (names.Length == 0) {
                throw Failure.EmptyCollection("names");
            }
            if (names.Any(t => t == null)) {
                throw Failure.CollectionContainsNullElement("names");
            }
            return names.Reverse().Aggregate((x, y) => y.AddRight(x));
        }

        public static MetadataName Combine(MetadataName name1, MetadataName name2) {
            if (name1 == null) {
                throw new ArgumentNullException("name1");
            }
            if (name2 == null) {
                throw new ArgumentNullException("name2");
            }
            return name1.AddRight(name2);
        }

        public static bool operator ==(MetadataName lhs, MetadataName rhs) {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }
            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(MetadataName lhs, MetadataName rhs) {
            return !(lhs == rhs);
        }

        public override string ToString() {
            return this.ToString("G");
        }

        public string ToString(string format) {
            return MetadataNameFormat.Default.Format(format, this, null);
        }

        public string ToString(string format, IFormatProvider formatProvider) {
            return MetadataNameFormat.Default.Format(format, this, formatProvider);
        }

        public string ToString(MetadataNameFormat format) {
            if (format == null) {
                return ToString();
            }
            return format.Format(null, this, null);
        }

        public abstract bool Matches(MetadataName name);

        internal abstract string Accept(MetadataNameFormat formatter,
                                        string format,
                                        IFormatProvider provider);
    }
}
