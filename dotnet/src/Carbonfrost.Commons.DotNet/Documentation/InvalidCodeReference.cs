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

namespace Carbonfrost.Commons.DotNet.Documentation {

    sealed class InvalidCodeReference : CodeReference {

        private SymbolType symbol;
        private string originalString;

        public InvalidCodeReference(SymbolType symbol, string originalString) {
            this.symbol = symbol;
            this.originalString = originalString;
        }

        public override string ToString() {
            return OriginalString;
        }

        public override SymbolType SymbolType {
            get { return symbol; } }

        public override CodeReferenceType ReferenceType {
            get { return CodeReferenceType.Invalid; } }

        public override string OriginalString {
            get { return originalString; } }

        public override MetadataName MetadataName {
            get { return null; } }
    }
}
