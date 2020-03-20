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
using System.Collections.Generic;

namespace Carbonfrost.Commons.DotNet {

    partial class SignatureParser {

        internal sealed class Scanner : IEnumerator<Token> {

            private Token _la;
            private Token _pushBack;
            private int _pos;
            private readonly string _text;

            public string Value {
                get {
                    return Current.Value;
                }
            }

            public bool IsError {
                get {
                    return _la != null && _la.Type == TokenType.Error;
                }
            }

            private char Char {
                get { return _text[_pos]; } }

            internal bool MoreChars {
                get { return _pos < _text.Length; } }

            public string Rest {
                get {
                    if (_pos >= _text.Length) {
                        return "<EOF>";
                    }
                    return _text.Substring(_pos);
                }
            }

            public Token Current {
                get { return _pushBack ??_la; } }

            object System.Collections.IEnumerator.Current {
                get { return Current; } }

            public bool EOF {
                get {
                    return Type == TokenType.EndOfInput;
                }
            }

            public TokenType Type {
                get {
                    return Current.Type;
                }
            }

            internal Scanner(string text) {
                _text = text;
                _pos = 0;
            }

            internal void PushBack() {
                if (_pushBack == null) {
                    _pushBack = _la;
                } else {
                    throw new NotImplementedException("Already have pushback " + _pushBack.Value);
                }
            }

            private static int ScanForPropertyExpressionEnd(
                string expression, int index) {
                int count = 0;

                while (index < expression.Length) {
                    char c = expression[index];

                    // Paren balancing
                    if (c == '(')
                        count++;

                    else if (c == ')')
                        count--;

                    if (count == 0)
                        return index;

                    index++;
                }

                return index;
            }

            private bool Skip2(Func<char, bool> predicate) {
                int start = _pos;
                while (MoreChars && predicate(Char)) {
                    _pos++;
                }
                return start != _pos;
            }

            private bool ParseIdentifier() {
                if (!MoreChars) {
                    return false;
                }
                int start = _pos;
                Skip2(IsIdentifierChar);
                if (start != _pos) {
                    string str = Subtext(start);
                    if (str == "modopt") {
                        Set(Token.Modopt);
                    } else if (str == "modreq") {
                        Set(Token.Modreq);
                    } else {
                        Set(new Token(TokenType.Identifier, str));
                    }
                    return true;
                }
                return false;
            }

            private bool IsIdentifierChar(char c) {
                return c == '_' || char.IsLetter(c) || char.IsDigit(c);
            }

            private Token SkipWhiteSpace() {
                if (Skip2(char.IsWhiteSpace)) {
                    return Token.Space;
                }
                return null;
            }

            private void SkipDigits() {
                Skip2(char.IsDigit);
            }

            private void Set(Token t) {
                _la = t;
            }

            private string Subtext(int start) {
                return _text.Substring(start, _pos - start);
            }

            void IDisposable.Dispose() {}

            public bool RequireMoveNext() {
                if (MoveNext())
                    return true;
                else
                    throw new NotImplementedException("Unterminated");
            }

            public bool MoveToContent() {
                while (MoveNext()) {
                    if (Current.Type == TokenType.Space) {
                        continue;
                    }
                    return true;
                }
                return false;
            }

            public bool MoveNext() {
                if (IsError) {
                    return false;
                }
                if (_pushBack != null) {
                    _pushBack = null;
                    return true;
                }

                var ws = SkipWhiteSpace();
                if (ws != null) {
                    Set(Token.Space);
                    return true;
                }

                if (_pos == _text.Length) {
                    Set(Token.EndOfInput);
                    _pos++;
                    return true;

                } else if (_pos > _text.Length) {
                    Set(null);
                    return false;
                }

                switch (Char) {
                    case '!':
                        return LASimple(Token.Bang, '!', Token.BangBang);

                    case '`':
                        int start = _pos;
                        if (LA('`')) {
                            _pos += 2;
                        } else {
                            _pos++;
                        }
                        SkipDigits();
                        var tokenStr = Subtext(start);
                        if (tokenStr.TrimStart('`').Length == 0) {
                            Set(Token.Error);
                            return true;
                        }

                        Set(new GenericPositionToken(tokenStr));
                        return true;

                    case '+':
                        return Simple(Token.Plus);

                    case '=':
                        return Simple(Token.EqualTo);

                    case '-':
                        return Simple(Token.Minus);

                    case '(':
                        return Simple(Token.LeftParen);

                    case ':':
                        return LASimple(Token.Colon, ':', Token.DoubleColon);

                    case ')':
                        return Simple(Token.RightParen);

                    case '[':
                        return Simple(Token.LeftBracket);

                    case ']':
                        return Simple(Token.RightBracket);

                    case '*':
                        return Simple(Token.Asterisk);

                    case '/':
                        return Simple(Token.Slash);

                    case '.':
                        if (LA("ctor")) {
                            Set(new Token(TokenType.Identifier, ".ctor"));
                            _pos += ".ctor".Length;
                            return true;
                        }
                        if (LA("cctor")) {
                            Set(new Token(TokenType.Identifier, ".cctor"));
                            _pos += ".cctor".Length;
                            return true;
                        }
                        return Simple(Token.Dot);

                    case ',':
                        return Simple(Token.Comma);

                    case '<':
                        Set(Token.LessThan);
                        _pos++;
                        return true;

                    case '>':
                        Set(Token.GreaterThan);
                        _pos++;
                        return true;

                    case '&':
                        Set(Token.And);
                        _pos++;
                        return true;
                }

                return ParseIdentifier();
            }

            private bool Simple(Token token) {
                Set(token);
                _pos++;
                return true;
            }

            private bool LASimple(Token token, char la, Token token2) {
                if (LA(la)) {
                    Set(token2);
                    _pos += 2;

                } else {
                    Set(token);
                    _pos++;
                }

                return true;
            }

            private bool LA(char c) {
                return (_pos + 1) < _text.Length
                    && _text[_pos + 1] == c;
            }

            private bool LA(string text) {
                return (_pos + text.Length) < _text.Length
                    && _text.Substring(_pos + 1, text.Length) == text;
            }

            void System.Collections.IEnumerator.Reset()  {
                throw new NotSupportedException();
            }
        }

        internal enum TokenType {
            Comma,
            EqualTo,
            Dot,
            LeftParen,
            RightParen,
            LeftBracket,
            RightBracket,
            LessThan,
            GreaterThan,
            Colon,
            DoubleColon,
            Space,

            And,

            Plus,
            Minus,
            Slash,
            Asterisk,

            Modopt,
            Modreq,

            GenericPosition,
            Bang,
            BangBang,
            Identifier,

            Error,

            Function,
            EndOfInput,
        }

        internal class GenericPositionToken : Token {

            public int Position {
                get {
                    return Int32.Parse(Value.TrimStart('`'));
                }
            }

            public bool IsMethod {
                get {
                    return Value.StartsWith("``");
                }
            }

            internal GenericPositionToken(string tokenString)
                : base(TokenType.GenericPosition, tokenString) {
            }
        }

        internal class Token {

            internal static readonly Token And = new Token(TokenType.And);
            internal static readonly Token Asterisk = new Token(TokenType.Asterisk);
            internal static readonly Token Bang = new Token(TokenType.Bang);
            internal static readonly Token BangBang = new Token(TokenType.BangBang);
            internal static readonly Token Colon = new Token(TokenType.Colon);
            internal static readonly Token Comma = new Token(TokenType.Comma);
            internal static readonly Token Dot = new Token(TokenType.Dot);
            internal static readonly Token DoubleColon = new Token(TokenType.DoubleColon);
            internal static readonly Token EndOfInput = new Token(TokenType.EndOfInput);
            internal static readonly Token EqualTo = new Token(TokenType.EqualTo);
            internal static readonly Token GreaterThan = new Token(TokenType.GreaterThan);
            internal static readonly Token LeftBracket = new Token(TokenType.LeftBracket);
            internal static readonly Token LeftParen = new Token(TokenType.LeftParen);
            internal static readonly Token LessThan = new Token(TokenType.LessThan);
            internal static readonly Token Minus = new Token(TokenType.Minus);
            internal static readonly Token Modopt = new Token(TokenType.Modopt);
            internal static readonly Token Modreq = new Token(TokenType.Modreq);
            internal static readonly Token Plus = new Token(TokenType.Plus);
            internal static readonly Token RightBracket = new Token(TokenType.RightBracket);
            internal static readonly Token RightParen = new Token(TokenType.RightParen);
            internal static readonly Token Slash = new Token(TokenType.Slash);
            internal static readonly Token Space = new Token(TokenType.Space);

            internal static readonly Token Error = new Token(TokenType.Error);

            private readonly string _text;
            private readonly TokenType _type;

            private Token(TokenType type) : this(type, null) {}

            internal Token(TokenType type, string tokenString) {
                _type = type;
                _text = tokenString;
            }

            internal TokenType Type {
                get {
                    return _type;
                }
            }

            internal string Value {
                get {
                    if (_text != null)
                        return _text;

                    switch (_type) {
                        case TokenType.Comma:
                            return ",";

                        case TokenType.LeftParen:
                            return "(";

                        case TokenType.RightParen:
                            return ")";

                        case TokenType.LeftBracket:
                            return "[";

                        case TokenType.RightBracket:
                            return "]";

                        case TokenType.Dot:
                            return ".";

                        case TokenType.LessThan:
                            return "<";

                        case TokenType.GreaterThan:
                            return ">";

                        case TokenType.Bang:
                            return "!";

                        case TokenType.BangBang:
                            return "!!";

                        case TokenType.Plus:
                            return "+";

                        case TokenType.Asterisk:
                            return "*";

                        case TokenType.Slash:
                            return "/";

                        case TokenType.Minus:
                            return "-";

                        case TokenType.And:
                            return "&";

                        case TokenType.Colon:
                            return ":";

                        case TokenType.DoubleColon:
                            return "::";

                        case TokenType.EqualTo:
                            return "=";

                        case TokenType.Space:
                            return " ";

                        case TokenType.Modreq:
                            return "modreq";

                        case TokenType.Modopt:
                            return "modopt";

                        case TokenType.EndOfInput:
                        case TokenType.Error:
                            return null;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }

        }
    }

}
