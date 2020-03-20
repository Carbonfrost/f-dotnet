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
using System.Text.RegularExpressions;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    abstract class BaseParser {

        internal class Scanner : IEnumerator<Token> {

            private int pos = -1;
            private Token current;
            private readonly string text;

            const string IDENT_START =
                @"(\p{Lu}|\p{Ll}|\p{Lt}|\p{Lm}|\p{Lo}|\p{Nl}|_)";

            const string IDENT_REPEAT =
                "(" + IDENT_START + @"|\p{Mn}|\p{Mc}|\p{Nd}|\p{Pc}|\p{Cf})";

            static readonly Regex Identifier = new Regex(
                IDENT_START + "(" + IDENT_REPEAT + ")*");

            static readonly Regex Mangle = new Regex(
                @"`(?<Value>\d+)");

            public Scanner(string text) {
                this.text = text;
            }

            public bool EOF {
                get {
                    return current == null;
                }
            }

            public TokenType Type {
                get {
                    return Current.Type;
                }
            }

            public Token Current {
                get {
                    if (current == null)
                        throw Failure.OutsideEnumeration();
                    else
                        return current;
                }
            }

            public string Rest {
                get {
                    return text.Substring(this.pos);
                }
            }

            object System.Collections.IEnumerator.Current {
                get { return Current; } }

            public void Dispose() {}

            public bool MoveNext() {
                this.current = null;

                pos++;

                while (pos < text.Length && char.IsWhiteSpace(text[pos])) {
                    pos++;
                }

                if (pos >= text.Length)
                    return false;

                char c = text[pos];
                switch (c) {
                    case '@':
                        return SetToken(Token.At);
                    case '&':
                        return SetToken(Token.Ampersand);
                    case ',':
                        return SetToken(Token.Comma);
                    case '(':
                        return SetToken(Token.LeftParen);
                    case ')':
                        return SetToken(Token.RightParen);
                    case '[':
                        return SetToken(Token.LeftBracket);
                    case ']':
                        return SetToken(Token.RightBracket);
                    case '<':
                        return SetToken(Token.LT);
                    case '>':
                        return SetToken(Token.GT);
                    case '{':
                        return SetToken(Token.LeftBrace);
                    case '}':
                        return SetToken(Token.RightBrace);
                    case '*':
                        return SetToken(Token.Star);
                    case '.':
                        return SetToken(Token.Dot);
                    case '`':
                        return ReadMangle();
                    case '+':
                    case '/': // Alternate syntax for nested types
                        return SetToken(Token.Plus);
                }

                Match m = Identifier.Match(text, pos);
                if (m.Success && m.Index == pos) {
                    pos += (m.Length - 1);
                    return SetToken(new Token(TokenType.Identifier, m.Value));
                }

                return SetToken(Token.Error);
            }

            bool ReadMangle() {
                bool methodMangle = false;

                if (text[pos + 1] == '`') {
                    pos++;
                    methodMangle = true;
                }
                Match m = Mangle.Match(text, pos);

                if (m.Success && m.Index == pos) {
                    pos += (m.Length - 1);
                    return SetToken(new Token(methodMangle ? TokenType.MethodMangle : TokenType.Mangle, m.Groups["Value"].Value));
                }

                return SetToken(Token.Error);
            }

            static bool Match(string text, string pattern, int pos) {
                int j = 0;

                for (int i = pos;
                     i < text.Length && j < pattern.Length;
                     j++, i++) {
                    if (text[i] != pattern[j])
                        return false;
                }

                return j == pattern.Length;
            }

            private bool SetToken(Token t) {
                this.current = t;
                return true;
            }

            public void Reset() {
                pos = 0;
            }

            public string ReadOne() {
                if (this.current == null)
                    return null;

                string text = Current.Value;
                MoveNext();
                return text;
            }
        }

        internal sealed class Token {

            public readonly TokenType Type;
            public readonly string Value;

            static readonly IDictionary<TokenType, char> MAP = new Dictionary<TokenType, char> {
                { TokenType.Comma, ',' },
                { TokenType.LeftParen, '(' },
                { TokenType.RightParen, ')' },
                { TokenType.LeftBracket, '[' },
                { TokenType.RightBracket, ']' },
                { TokenType.LT, '<' },
                { TokenType.GT, '>' },
                { TokenType.Dot, '.' },
                { TokenType.Star, '*' },
                { TokenType.At, '@' },
                { TokenType.Ampersand, '&' },
                { TokenType.Plus, '+' },
                { TokenType.LeftBrace, '{' },
                { TokenType.RightBrace, '}' },
                { TokenType.Error, ' ' },
            };

            private Token(TokenType type) {
                this.Type = type;
                this.Value = MAP[type].ToString();
            }

            internal Token(TokenType type, string text) {
                this.Type = type;
                this.Value = text;
            }

            public static readonly Token Comma = new Token(TokenType.Comma);
            public static readonly Token LeftParen = new Token(TokenType.LeftParen);
            public static readonly Token RightParen = new Token(TokenType.RightParen);
            public static readonly Token LeftBracket = new Token(TokenType.LeftBracket);
            public static readonly Token RightBracket = new Token(TokenType.RightBracket);
            public static readonly Token LT = new Token(TokenType.LT);
            public static readonly Token GT = new Token(TokenType.GT);
            public static readonly Token Dot = new Token(TokenType.Dot);
            public static readonly Token Plus = new Token(TokenType.Plus);
            public static readonly Token At = new Token(TokenType.At);
            public static readonly Token Ampersand = new Token(TokenType.Ampersand);
            public static readonly Token Star = new Token(TokenType.Star);
            public static readonly Token LeftBrace = new Token(TokenType.LeftBrace);
            public static readonly Token RightBrace = new Token(TokenType.RightBrace);
            public static readonly Token Error = new Token(TokenType.Error);
        }

        internal enum TokenType {
            Comma,
            LeftParen,
            RightParen,
            LeftBracket,
            RightBracket,
            LT, // <
            GT, // >
            Dot,
            Star,
            At,
            Ampersand,

            RightBrace,
            LeftBrace,

            ModOpt,
            ModReq,

            LessThan,
            GreaterThan,
            Identifier,
            Mangle,
            MethodMangle,
            Plus,

            Error,
        }
    }
}
