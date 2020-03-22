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

namespace Carbonfrost.Commons.DotNet {

    static class Utility {

        static readonly char[] WS = { ' ', '\t', '\r', '\n' };

        public static string[] SplitTokens(string text) {
            return text.Split(WS, StringSplitOptions.RemoveEmptyEntries);
        }

        internal delegate Exception TryParser<T>(string text, out T result);
        internal delegate Exception TryExactParser<T>(string text, string format, out T result);

        public static T Parse<T>(string text, TryParser<T> tryParse) {
            T result;
            Exception ex = tryParse(text, out result);
            if (ex == null) {
                return result;
            }
            throw ex;
        }

        public static T Parse<T>(string text, string format, TryExactParser<T> tryParse) {
            T result;
            Exception ex = tryParse(text, format, out result);
            if (ex == null) {
                return result;
            }
            throw ex;
        }
    }
}
