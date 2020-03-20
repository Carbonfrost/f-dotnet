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

    static class CodeReferenceHelper {

        public static char GetReferenceTypeSpecifierChar(SymbolType type) {
            switch (type) {
                case SymbolType.Event:
                case SymbolType.Field:
                case SymbolType.Method:
                case SymbolType.Namespace:
                case SymbolType.Property:
                case SymbolType.Type:
                case SymbolType.Assembly:
                default:
                    return type.ToString()[0];
            }
        }

        public static SymbolType GetReferenceType(char specifier) {
            switch (specifier) {
                case 'E':
                    return SymbolType.Event;
                case 'F':
                    return SymbolType.Field;
                case 'M':
                    return SymbolType.Method;
                case 'N':
                    return SymbolType.Namespace;
                case 'P':
                    return SymbolType.Property;
                case 'T':
                    return SymbolType.Type;
                case 'A':
                    return SymbolType.Assembly;
                default:
                    return SymbolType.Unknown;
            }
        }

    }
}
