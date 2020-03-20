//
// Copyright 2016, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;
using TokenType = Carbonfrost.Commons.DotNet.SignatureParser.TokenType;

namespace Carbonfrost.UnitTests.DotNet {

    public class SignatureParserScannerTests {

        [Fact]
        public void PushBack_should_yield_extra_token() {
            var s = new SignatureParser.Scanner("[]");

            Assert.True(s.MoveNext());
            Assert.Equal("[", s.Value);

            s.PushBack();
            Assert.True(s.MoveNext());
            Assert.Equal("[", s.Value);

            Assert.True(s.MoveNext());
            Assert.Equal("]", s.Value);

            Assert.True(s.MoveNext());
            Assert.Null(s.Value);
            Assert.False(s.MoveNext());
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r")]
        [InlineData("\n")]
        internal void Value_should_yield_ws_correct_types(string text) {
            var s = new SignatureParser.Scanner(text);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.Space, s.Type);
            Assert.Equal(" ", s.Value); // always collapse
        }

        [Theory]
        [InlineData("``1", true, 1)]
        [InlineData("`20", false, 20)]
        public void Current_should_yield_generic_correct_types(string text, bool method, int position) {
            var s = new SignatureParser.Scanner(text);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.GenericPosition, s.Type);
            var token = (SignatureParser.GenericPositionToken) s.Current;
            Assert.Equal(method, token.IsMethod);
            Assert.Equal(position, token.Position);
        }

        [Theory]
        [InlineData("``")]
        [InlineData("`")]
        public void Type_should_return_Error_on_invalid_generics(string text) {
            var s = new SignatureParser.Scanner(text);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.Error, s.Type);
        }

        [Theory]
        [InlineData(".ctor")]
        [InlineData(".cctor")]
        public void MoveNext_should_allow_special_identifiers(string text) {
            var s = new SignatureParser.Scanner(text);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.Identifier, s.Type);
            Assert.Equal(text, s.Value);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.EndOfInput, s.Type);
        }

        [Theory]
        [InlineData(TokenType.Comma, ",")]
        [InlineData(TokenType.EqualTo, "=")]
        [InlineData(TokenType.Dot, ".")]
        [InlineData(TokenType.LeftParen, "(")]
        [InlineData(TokenType.RightParen, ")")]
        [InlineData(TokenType.LeftBracket, "[")]
        [InlineData(TokenType.RightBracket, "]")]
        [InlineData(TokenType.LessThan, "<")]
        [InlineData(TokenType.GreaterThan, ">")]
        [InlineData(TokenType.Colon, ":")]
        [InlineData(TokenType.DoubleColon, "::")]
        [InlineData(TokenType.Space, " ")]
        [InlineData(TokenType.And, "&")]
        [InlineData(TokenType.Plus, "+")]
        [InlineData(TokenType.Minus, "-")]
        [InlineData(TokenType.Slash, "/")]
        [InlineData(TokenType.Asterisk, "*")]
        [InlineData(TokenType.Modopt, "modopt")]
        [InlineData(TokenType.Modreq, "modreq")]
        [InlineData(TokenType.Bang, "!")]
        [InlineData(TokenType.BangBang, "!!")]
        [InlineData(TokenType.Identifier, "ident")]
        internal void Value_should_yield_correct_types(TokenType type, string text) {
            // NB: method is internal due to TokenType being internal
            var s = new SignatureParser.Scanner(text);

            Assert.True(s.MoveNext());
            Assert.Equal(type, s.Type);
            Assert.Equal(text, s.Value);

            Assert.True(s.MoveNext());
            Assert.Equal(TokenType.EndOfInput, s.Type);
        }
    }
}
