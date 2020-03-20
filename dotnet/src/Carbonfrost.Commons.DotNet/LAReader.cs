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

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    class LAReader : IEnumerator<char> {

        private StringBuilder buffer = new StringBuilder();
        private string text;
        private int index = -1;
        private char sentinel;

        public LAReader(string text) {
            this.text = text;
        }

        public LAReader(LAReader inside, char sentinel) {
            this.text = inside.text;
            this.index = inside.index;
            this.sentinel = sentinel;
        }

        public char LA {
            get {
                // Safe as long as current is safe
                if (index < 0) {
                    throw Failure.OutsideEnumeration();
                }
                else if (index + 1 == text.Length) {
                    return '\0';
                }
                else {
                    return text[index + 1];
                }
            }
        }

        public string Rest {
            get {
                if (index == text.Length) {
                    return string.Empty;
                }
                else {
                    return text.Substring(index);
                }
            }
        }

        public char Current {
            get {
                if (index < 0 || index >= text.Length) {
                    throw Failure.OutsideEnumeration();
                }
                return text[index]; }
        }

        public bool IsEof { get{ return index >= text.Length; } }

        public bool Whitespace { get { return _IsWhitespace(Current); } }
        public bool Digit { get { return Is('0', '9'); } }

        // Creates a new subreader with the given sentinel
        // The reader reads until the sentinel and will end
        public LAReader Sentinel(char c) {
            return new LAReader(this, c);
        }

        public bool Is(char from, char to) {
            char c = Current;
            return (from <= c) && (c <= to);
        }

        public string StopEating() {
            string result = buffer.ToString();
            buffer.Length = 0;
            return result;
        }

        // Compares the current character to c, if so advances
        // the reader; returns true if the data was eaten, otherwise
        // false if no data was left to eat or it did not match.
        public bool Eat(char c) {
            if (Current == c) {
                return MoveNext() || true;
            }
            else {
                return false;
            }
        }

        /*public string EatLine() {
		while (MoveNext()) {
		  if (Current == '\r')
		  if (Current == '\n')
		}
		}*/
        public bool Eat() {
            this.buffer.Append(Current);
            return MoveNext();
        }

        public bool TryEatDigits(out int result) {
            return Int32.TryParse(EatDigitsCommon(), out result);
        }

        // Considers the current character and all digit characters
        // to parse an integer.  The reader is positioned at the last
        // non-digit character.
        public int EatDigits() {
            return Int32.Parse(EatDigitsCommon());
        }

        string EatDigitsCommon() {
            while (!IsEof && Digit) {
                Eat();
            }
            return StopEating();
        }

        // TODO Support a stack to do more sophisticated bracket counting
        public string EatUntilWithBracketCounting(char stopOn) {
            int depth = 0;
            while (!IsEof) {
                if (this.Current == stopOn && depth == 0) {
                    break;
                }

                switch (Current) {
                    case '[':
                    case '<':
                    case '(':
                    case '{':
                        depth++;
                        break;
                    case ']':
                    case '>':
                    case ')':
                    case '}':
                        depth--;
                        break;
                }
                Eat();
            }
            return StopEating();
        }

        public string EatBracket(char closing) {
            char opening = Current;
            if (MoveNext()) {
                int depth = 1;
                while (!IsEof) {
                    if (Current == opening)
                        depth++;
                    else if (Current == closing)
                        depth--;

                    if (depth == 0)
                        return StopEating();

                    Eat();
                }
            }
            throw Failure.Eof();
        }

        // Advances the position of the reader, then skips
        // all whitespace; reader will be positioned on the first
        // non-whitespace character
        public bool SkipWhitespace() {
            while (MoveNext()) {
                if (!_IsWhitespace(Current)) {
                    return true;
                }
            }
            return false;
        }

        // Compares the current character to whitespace
        // Positioned on the last whitespace character
        public bool EatWhitespace() {
            bool result = false;
            if (_IsWhitespace(Current)) {
                while (_IsWhitespace(LA)) {
                    result = MoveNext();
                }
            }
            return result;
        }

        static bool _IsWhitespace(char c) {
            return c == ' '
                || c == '\t'
                || c == '\n'
                || c == '\r';
        }

        // IEnumerator implementation
        object System.Collections.IEnumerator.Current {
            get { return this.Current; } }

        public void Dispose() {}

        public bool MoveNext() {
            index++;
            return (index < this.text.Length);
        }

        public void Reset() {
            throw new NotSupportedException();
        }

        public string EatToEnd() {
            while (Eat())
                ;
            return StopEating();
        }
    }
}
