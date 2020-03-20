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

namespace Carbonfrost.Commons.DotNet {

    struct ReadOnlyList<T> : IReadOnlyList<T> {

        private readonly IList<T> items;
        private readonly int offset;
        private readonly int count;

        public ReadOnlyList(IList<T> items, int offset = 0) {
            this.items = items;
            this.offset = offset;
            this.count = items.Count - offset;
        }

        public ReadOnlyList(IList<T> items, int offset, int count) {
            this.items = items;
            this.offset = offset;
            this.count = count;
        }

        public T this[int index] {
            get { return items[index + offset]; } }

        public int Count {
            get { return count; } }

        public IEnumerator<T> GetEnumerator() {
            return items.Skip(offset).Take(count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }

}
