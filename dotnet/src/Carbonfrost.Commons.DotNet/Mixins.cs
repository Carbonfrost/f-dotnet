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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Carbonfrost.Commons.DotNet {

    static class Mixins {

        public static bool AllTrue(this IEnumerable<bool> items) {
            foreach (var e in items) {
                if (!e)
                    return false;
            }

            return true;
        }

        public static bool IsHomogeneous<T>(this IEnumerable<T> items, Func<T, bool> predicate) {
            bool first = false;
            bool isFirst = true;

            foreach (var t in items) {
                if (isFirst) {
                    first = predicate(t);
                    isFirst = false;

                } else if (predicate(t) != first) {
                    return false;
                }
            }

            return true;
        }

    }

}
