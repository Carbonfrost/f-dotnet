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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    class CompositeReadOnlyList<T> : IReadOnlyList<T> {

        private readonly IReadOnlyList<T> left;
        private readonly IReadOnlyList<T> right;

        public CompositeReadOnlyList(IReadOnlyList<T> left, IReadOnlyList<T> right) {
            this.left = left;
            this.right = right;
        }

        public T this[int index] {
            get {
                if (index < 0)
                    throw Failure.Negative("index", index);
                if (index < left.Count)
                    return left[index];

                index -= left.Count;
                if (index >= right.Count)
                    throw Failure.IndexOutOfRange("index", index);

                return right[index - left.Count];
            }
        }

        public int Count {
            get { return left.Count + right.Count; } }

        public IEnumerator<T> GetEnumerator() {
            return (left.Concat(right)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
