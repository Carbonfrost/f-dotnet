//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public class SymbolTypes : SymbolTypeMap<bool> {

        public SymbolTypes() {
        }

        public SymbolTypes(bool value) : base(value) {

        }

        private SymbolTypes(IEnumerable<bool> values) : base(values) {
        }

        public override bool Equals(object obj) {
            return base.Equals(obj);
        }

        public override int GetHashCode() {
            return base.GetHashCode();
        }

        public static bool operator ==(SymbolTypes lhs, SymbolTypes rhs) {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }

            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
                return false;
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(SymbolTypes lhs, SymbolTypes rhs) {
            return !(lhs == rhs);
        }

        public static SymbolTypes Parse(string text) {
            return Utility.Parse<SymbolTypes>(text, _TryParse);
        }

        static Exception _TryParse(string text, out SymbolTypes result) {
            result = null;

            if (text == null) {
                return new ArgumentNullException(nameof(text));
            }

            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace(nameof(text));
            }
            if (text == "All") {
                result = new SymbolTypes(true);
                return null;
            }
            if (text == "None") {
                result = new SymbolTypes();
                return null;
            }

            result = new SymbolTypes();

            foreach (var t in Utility.SplitTokens(text)) {
                SymbolType symbol;

                if (Enum.TryParse(t, true, out symbol)) {
                    result[symbol] = true;

                } else {
                    return Failure.NotParsable(nameof(text), typeof(SymbolTypes));
                }
            }

            return null;
        }

        public static bool TryParse(string text, out SymbolTypes result) {
            return _TryParse(text, out result) == null;
        }

        public override string ToString() {
            if (this.IsHomogeneous(t => t.Value)) {
                return this[0] ? "All" : "None";
            }

            return string.Join(" ", this.Where(t => t.Value).Select(t => t.Key));
        }

        public new SymbolTypes Clone() {
            return new SymbolTypes(Values);
        }
    }

}
