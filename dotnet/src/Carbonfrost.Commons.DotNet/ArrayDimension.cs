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

    public struct ArrayDimension : IEquatable<ArrayDimension> {

        static readonly Regex PATTERN = new Regex(@"^(?<LowerBound> \d+)?\.\.(?<UpperBound> \d+)?$",
                                                  RegexOptions.IgnorePatternWhitespace);

        public static readonly ArrayDimension Unsized = new ArrayDimension(null, null);

        private readonly int? _lowerBound;
        private readonly int? _upperBound;

        public bool IsSized {
            get {
                return (_lowerBound.HasValue || _upperBound.HasValue);
            }
        }

        public int? LowerBound {
            get {
                return _lowerBound;
            }
        }

        public int? UpperBound {
            get {
                return _upperBound;
            }
        }

        public ArrayDimension(int? lowerBound, int? upperBound) {
            if (lowerBound.HasValue
                && upperBound.HasValue
                && lowerBound.Value > upperBound.Value) {
                throw Failure.MinMustBeLessThanMax("lowerBound", lowerBound);
            }
            if (lowerBound.HasValue && lowerBound.Value < 0) {
                throw Failure.Negative("lowerBound", lowerBound);
            }
            if (upperBound.HasValue && upperBound.Value < 0) {
                throw Failure.Negative("lowerBound", upperBound);
            }

            _lowerBound = lowerBound;
            _upperBound = upperBound;
        }

        public static ArrayDimension Parse(string text) {
            return Utility.Parse<ArrayDimension>(text, _TryParse);
        }

        public static bool TryParse(string text, out ArrayDimension result) {
            return _TryParse(text, out result) == null;
        }

        // `object' overrides
        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * _lowerBound.GetHashCode();
                hashCode += 1000000009 * _upperBound.GetHashCode();
            }
            return hashCode;
        }

        public override bool Equals(object obj) {
            return (obj is ArrayDimension)
                && Equals((ArrayDimension) obj);
        }

        public override string ToString() {
            if (IsSized) {
                return (_lowerBound + ".." + _upperBound);
            }
            return string.Empty;
        }

        public bool Equals(ArrayDimension other) {
            return _lowerBound == other._lowerBound
                && _upperBound == other._upperBound;
        }

        public static bool operator ==(ArrayDimension lhs, ArrayDimension rhs) {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(ArrayDimension lhs, ArrayDimension rhs) {
            return !(lhs == rhs);
        }

        static Exception _TryParse(string text, out ArrayDimension result) {
            result = default(ArrayDimension);
            if (text == null) {
                return new ArgumentNullException("text");
            }

            text = text.Trim();
            if (text.Length == 0) {
                return Failure.EmptyString("text");
            }

            Match m = PATTERN.Match(text);
            if (!m.Success) {
                return Failure.NotParsable("text", typeof(ArrayDimension));
            }

            int? lowerBound = GetInt32(m.Groups["LowerBound"].Value);
            int? upperBound = GetInt32(m.Groups["UpperBound"].Value);
            if (lowerBound.HasValue || upperBound.HasValue) {
                result = new ArrayDimension(lowerBound, upperBound);
                return null;
            }

            return Failure.NotParsable("text", typeof(ArrayDimension));
        }

        static int? GetInt32(string text) {
            int result;
            if (Int32.TryParse(text, out result))
                return result;
            else
                return null;
        }
    }
}
