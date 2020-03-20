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
using System.Text;

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class EventCodeReference : ValidCodeReference {

        internal EventCodeReference(string original, EventName name)
            : base(original, name) {}

        public static CodeReference ParseEvent(string text) {
            EventName result;

            if (string.IsNullOrWhiteSpace(text) || !TryParseHelper(text, out result))
                return new InvalidCodeReference(SymbolType.Property, text);
            else
                return new EventCodeReference(text, result);
        }

        static bool TryParseHelper(string name, out EventName result) {
            return TryParseSimpleMember(
                name, out result, (a, b, c) => (new EventName(a, b, c)));
        }

        public override SymbolType SymbolType {
            get { return SymbolType.Event; } }

        protected override string CanonicalString {
            get {
                EventName name = (EventName) this.MetadataName;
                StringBuilder sb = new StringBuilder();

                AppendBaseMemberName(name, sb);
                return sb.ToString();
            }
        }

    }
}
