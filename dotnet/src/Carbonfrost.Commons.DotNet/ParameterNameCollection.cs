//
// Copyright 2017, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
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

    public class ParameterNameCollection : IList<ParameterName>, IReadOnlyList<ParameterName> {

        public static ParameterNameCollection Empty = new ParameterNameCollection(Array.Empty<ParameterName>());

        private readonly IReadOnlyList<ParameterName> _items;

        internal ParameterNameCollection(IReadOnlyList<ParameterName> items) {
            _items = items;
        }

        public ParameterName this[string name] {
            get {
                if (name == null) {
                    throw new ArgumentNullException(nameof(name));
                }
                if (string.IsNullOrEmpty(nameof(name))) {
                    throw Failure.EmptyString(nameof(name));
                }
                return this.FirstOrDefault(t => t.Name == name);
            }
        }

        public ParameterName this[int index] {
            get {
                return _items[index];
            }
        }

        public int Count {
            get {
                return _items.Count;
            }
        }

        bool ICollection<ParameterName>.IsReadOnly {
            get {
                return true;
            }
        }

        ParameterName IList<ParameterName>.this[int index] {
            get {
                return this[index];
            }
            set {
                throw Failure.ReadOnlyCollection();
            }
        }

        public bool Contains(string name) {
            return this[name] != null;
        }

        public bool Contains(ParameterName item) {
            return _items.Contains(item);
        }

        public int IndexOf(string name) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }
            if (string.IsNullOrEmpty(nameof(name))) {
                throw Failure.EmptyString(nameof(name));
            }
            for (int i = 0; i < _items.Count; i++) {
                if (_items[i].Name == name) {
                    return i;
                }
            }
            return -1;
        }

        public int IndexOf(ParameterName item) {
            if (item == null) {
                throw new ArgumentNullException(nameof(item));
            }
            for (int i = 0; i < _items.Count; i++) {
                if (_items[i] == item) {
                    return i;
                }
            }
            return -1;
        }

        public IEnumerator<ParameterName> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        void IList<ParameterName>.Insert(int index, ParameterName item) {
            throw Failure.ReadOnlyCollection();
        }

        void IList<ParameterName>.RemoveAt(int index) {
            throw Failure.ReadOnlyCollection();
        }

        void ICollection<ParameterName>.Add(ParameterName item) {
            throw Failure.ReadOnlyCollection();
        }

        void ICollection<ParameterName>.Clear() {
            throw Failure.ReadOnlyCollection();
        }

        void ICollection<ParameterName>.CopyTo(ParameterName[] array, int arrayIndex) {
            _items.ToList().CopyTo(array, arrayIndex);
        }

        bool ICollection<ParameterName>.Remove(ParameterName item) {
            throw Failure.ReadOnlyCollection();
        }
    }
}
