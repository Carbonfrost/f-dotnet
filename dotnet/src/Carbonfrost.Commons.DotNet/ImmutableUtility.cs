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

namespace Carbonfrost.Commons.DotNet {

    static class ImmutableUtility {

        public static T[] Add<T>(IReadOnlyList<T> source, Func<T, int, T> copyValues, T value) {
            return Insert(source, source == null ? 0 : source.Count, copyValues, value);
        }

        public static T[] Set<T>(IReadOnlyList<T> source, int index, Func<T, int, T> copyValues, T value) {
            T[] pms;
            if (source == null) {
                pms = new T[1];
            } else {
                pms = source.Select(copyValues).ToArray();
            }
            pms[index] = value;
            return pms;
        }

        public static T[] Insert<T>(IReadOnlyList<T> source, int index, Func<T, int, T> copyValues, T value) {
            T[] pms;
            int position = 0;
            if (source == null) {
                pms = new T[1];
            } else {
                pms = new T[source.Count + 1];
                foreach (var pm in source) {
                    pms[position] = copyValues(pm, position);
                    position++;
                }
            }
            pms[index] = value;
            return pms;
        }
    }
}
