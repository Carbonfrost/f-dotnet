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
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public sealed class ArrayDimensionCollection : ReadOnlyCollection<ArrayDimension> {

        internal ArrayDimensionCollection(IList<ArrayDimension> items) : base(items) {}

        public ArrayDimensionCollection(IEnumerable<ArrayDimension> items)
            : base(CheckArg(items)) {}

        static ArrayDimension[] CheckArg(IEnumerable<ArrayDimension> items) {
            if (items == null) {
                throw new ArgumentNullException("items");
            }
            return items.ToArray();
        }

        public override string ToString() {
            return string.Join(",", Items);
        }

        public static bool TryParse(string text, out ArrayDimensionCollection value) {
            value = _TryParse(text, false);
            return (value != null);
        }

        public static ArrayDimensionCollection Parse(string text) {
            return _TryParse(text, true);
        }

        private static ArrayDimensionCollection _TryParse(string text, bool throwOnError) {
            if (text == null) {
                if (throwOnError) {
                    throw new ArgumentNullException("text");
                }
                else {
                    return null;
                }
            }

            if (text.Length == 0) {
                if (throwOnError) {
                    throw Failure.EmptyString("text");
                }
                else {
                    return null;
                }
            }

            string[] items = text.Split(',');
            int index = 0;
            ArrayDimension[] result = new ArrayDimension[items.Length];
            foreach (var m in items) {
                string dim = m.Trim();

                if (dim.Length == 0) {
                    result[index++] = new ArrayDimension();
                }
                else {
                    ArrayDimension ad;
                    if (!ArrayDimension.TryParse(dim, out ad)) {
                        if (throwOnError) {
                            throw Failure.NotParsable("text", typeof(ArrayDimensionCollection));
                        }
                        else {
                            return null;
                        }
                    }

                    result[index++] = ad;
                }
            }

            return new ArrayDimensionCollection(result);
        }

    }
}
