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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet.Documentation {

    class TypeCodeReferenceParser : BaseParser {

        private readonly Scanner s;
        private readonly GenericNameContext context;

        public TypeCodeReferenceParser(string typeName, GenericNameContext context) {
            this.s = new Scanner(typeName);
            this.context = context;
        }

        public TypeName Parse() {
            s.MoveNext();

            TypeName e = TypeRoot();
            if (s.EOF) {
                return e;
            }

            return null;
        }

        // TypeRoot = Mangle | QualifiedIdentifier Specifiers?

        // Mangle = ( '`' Number)
        // QualifiedIdentifier = MIdentifier ( '.' MIdentifier ) *
        // MIdentifier = Identifier Mangle ?

        // Specifiers = GenericConstruction | Array | Pointer
        // GenericConstruction = '{' TypeRoot + '}'
        // Array = '[' ']'
        // Pointer = '*'

        private TypeName TypeRoot() {
            if (s.Type == TokenType.Mangle) {
                int index = Int32.Parse(s.Current.Value);
                if (index < 0 || index >= context.Type.GenericParameters.Count) {
                    return null;
                }

                var result2 = this.context.Type.GenericParameters[index];
                s.MoveNext();
                return Specifiers(result2);
            }

            if (s.Type == TokenType.MethodMangle) {
                int index = Int32.Parse(s.Current.Value);
                if (index < 0 || index >= context.Method.GenericParameters.Count) {
                    return null;
                }

                var result2 = this.context.Method.GenericParameters[index];
                s.MoveNext();
                return Specifiers(result2);
            }

            if (s.Type != TokenType.Identifier) {
                return null;
            }

            string name = QualifiedIdentifier();
            if (name == null) {
                return null;
            }

            if (s.EOF) {
                return DefaultTypeName.FromFullName(name);
            }

            var result = DefaultTypeName.FromFullName(name);
            return Specifiers(result);
        }

        private TypeName Specifiers(TypeName result) {
            while (!s.EOF && (s.Type == TokenType.Mangle
                   || s.Type == TokenType.LeftBrace
                   || s.Type == TokenType.LeftBracket
                   || s.Type == TokenType.At
                   || s.Type == TokenType.Star)) {

                result = Specifier(result);
                if (result == null) {
                    return null;
                }
            }

            bool couldBeNested = false;
            while (!s.EOF && s.Type == TokenType.Dot) {

                if (s.MoveNext() && s.Type == TokenType.Identifier) {
                    result = result.GetNestedType(s.ReadOne());
                    couldBeNested = true;

                } else {
                    return null;
                }
            }

            if (couldBeNested) {
                result = Specifiers(result);
            }

            return result;
        }

        private TypeName Specifier(TypeName result) {
            if (s.EOF || result == null) {
                return result;
            }

            if (s.Type == TokenType.Mangle) {
                ((DefaultTypeName) result).FinalizeGenerics(Int32.Parse(s.Current.Value));
                s.MoveNext();
                return result;

            } else if (s.Type == TokenType.LeftBrace) {
                var generics = GenericConstruction();
                if (generics == null)
                    return null;

                ((DefaultTypeName) result).FinalizeGenerics(generics.Count());
                return result.MakeGenericType(generics);

            } else if (s.Type == TokenType.LeftBracket) {
                var item = Array();
                return item == null ? null : result.MakeArrayType(item);

            } else if (s.Type == TokenType.At) {
                s.MoveNext();
                return result.MakeByReferenceType();

            } else if (s.Type == TokenType.Star) {
                s.MoveNext();
                return result.MakePointerType();
            }

            return result;
        }

        IEnumerable<ArrayDimension> Array() {
            s.MoveNext();

            // TODO Could have ws between brackets
            if (s.Type == TokenType.RightBracket) {
                s.MoveNext();
                return new [] { ArrayDimension.Unsized };
            } else
                throw new NotImplementedException();
        }

        IEnumerable<TypeName> GenericConstruction() {
            var sb = new List<TypeName>();
            s.MoveNext();

            while (!s.EOF) {
                var name = TypeRoot();
                if (s.EOF || name == null) {
                    return null;
                }

                sb.Add(name);

                if (s.Type == TokenType.Comma) {
                    if (!s.MoveNext()) {
                        return null;
                    }

                } else if (s.Type == TokenType.RightBrace) {
                    s.MoveNext();
                    return sb;
                }
            }

            return null;
        }

        private string QualifiedIdentifier() {
            StringBuilder sb = new StringBuilder();
            sb.Append(s.ReadOne());

            while (!s.EOF && s.Type == TokenType.Dot) {
                sb.Append(".");

                if (s.MoveNext() && s.Type == TokenType.Identifier) {
                    sb.Append(s.ReadOne());

                } else {
                    return null;
                }
            }

            return sb.ToString();
        }
    }
}
