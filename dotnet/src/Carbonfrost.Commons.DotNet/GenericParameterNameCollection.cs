//
// Copyright 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class GenericParameterNameCollection : IReadOnlyList<GenericParameterName> {

        public static GenericParameterNameCollection Empty = new GenericParameterNameCollection(Array.Empty<GenericParameterName>());

        private readonly IReadOnlyList<GenericParameterName> _items;

        internal GenericParameterNameCollection(IReadOnlyList<GenericParameterName> items) {
            _items = items;
        }

        public GenericParameterName this[string name] {
            get {
                if (name == null) {
                    throw new ArgumentNullException("name");
                }
                if (string.IsNullOrEmpty("name")) {
                    throw Failure.EmptyString("name");
                }
                return this.FirstOrDefault(t => t.Name == name);
            }
        }

        public GenericParameterName this[int index] {
            get {
                return _items[index];
            }
        }

        public int Count {
            get {
                return _items.Count;
            }
        }

        public bool Contains(string name) {
            return this[name] != null;
        }

        public bool Contains(GenericParameterName item) {
            return _items.Contains(item);
        }

        public int IndexOf(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty("name")) {
                throw Failure.EmptyString("name");
            }
            for (int i = 0; i < _items.Count; i++) {
                if (_items[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(GenericParameterName item) {
            if (item == null) {
                throw new ArgumentNullException("item");
            }
            for (int i = 0; i < _items.Count; i++) {
                if (_items[i] == item) {
                    return i;
                }
            }
            return -1;
        }

        public IEnumerator<GenericParameterName> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        internal GenericParameterName[] ImmutableRemoveAt(int index, Func<GenericParameterName, int, GenericParameterName> cloneParameter) {
            return ImmutableUtility.RemoveAt(this, index, cloneParameter);
        }

        internal GenericParameterName[] ImmutableRemove(GenericParameterName parameter, Func<GenericParameterName, int, GenericParameterName> cloneParameter) {
            return ImmutableUtility.Remove(this, parameter, cloneParameter);
        }

        internal GenericParameterName[] ImmutableInsertAt(int index, GenericParameterName item, Func<GenericParameterName, int, GenericParameterName> cloneParameter) {
            return ImmutableUtility.Insert(this, index, cloneParameter, item);
        }

        internal GenericParameterName[] ImmutableSet(int index, GenericParameterName item, Func<GenericParameterName, int, GenericParameterName> cloneParameter) {
            return ImmutableUtility.Set(this, index, cloneParameter, item);
        }

        internal GenericParameterName[] ImmutableAdd(GenericParameterName item, Func<GenericParameterName, int, GenericParameterName> cloneParameter) {
            return ImmutableUtility.Add(this, cloneParameter, item);
        }
    }
}
