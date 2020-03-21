//
// Copyright 2013, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

    public class SymbolTypeMap<T>: IDictionary<SymbolType, T> {

        const int MAX = 1 + (int) SymbolType.Assembly;
        private readonly T[] _values = new T[MAX];

        private int _version;

        public T All {
            get {
                return _values[0];
            }
            set {
                ThrowIfReadOnly();
                _version++;
                for (int i = 0; i < _values.Length; i++) {
                    _values[i] = value;
                }
            }
        }

        public T Field {
            get {
                return this[SymbolType.Field];
            }
            set {
                this[SymbolType.Field] = value;
            }
        }

        public T Property {
            get {
                return this[SymbolType.Property];
            }
            set {
                this[SymbolType.Property] = value;
            }
        }

        public T Event {
            get {
                return this[SymbolType.Event];
            }
            set {
                this[SymbolType.Event] = value;
            }
        }

        public T Method {
            get {
                return this[SymbolType.Method];
            }
            set {
                this[SymbolType.Method] = value;
            }
        }

        public T Type {
            get {
                return this[SymbolType.Type];
            }
            set {
                this[SymbolType.Type] = value;
            }
        }

        public T Attribute {
            get {
                return this[SymbolType.Attribute];
            }
            set {
                this[SymbolType.Attribute] = value;
            }
        }

        public T Parameter {
            get {
                return this[SymbolType.Parameter];
            }
            set {
                this[SymbolType.Parameter] = value;
            }
        }

        public T InternedLocation {
            get {
                return this[SymbolType.InternedLocation];
            }
            set {
                this[SymbolType.InternedLocation] = value;
            }
        }

        public T Namespace {
            get {
                return this[SymbolType.Namespace];
            }
            set {
                this[SymbolType.Namespace] = value;
            }
        }

        public T Module {
            get {
                return this[SymbolType.Module];
            }
            set {
                this[SymbolType.Module] = value;
            }
        }

        public T Resource {
            get {
                return this[SymbolType.Resource];
            }
            set {
                this[SymbolType.Resource] = value;
            }
        }

        public T Local {
            get {
                return this[SymbolType.Local];
            }
            set {
                this[SymbolType.Local] = value;
            }
        }

        public T Alias {
            get {
                return this[SymbolType.Alias];
            }
            set {
                this[SymbolType.Alias] = value;
            }
        }

        public T Assembly {
            get {
                return this[SymbolType.Assembly];
            }
            set {
                this[SymbolType.Assembly] = value;
            }
        }

        public int Count {
            get {
                return _values.Length;
            }
        }

        public bool IsReadOnly { get; private set; }

        protected void ThrowIfReadOnly() {
            if (IsReadOnly) {
                throw Failure.ReadOnlyCollection();
            }
        }

        public T this[SymbolType key] {
            get {
                return _values[(int) key];
            }
            set {
                ThrowIfReadOnly();
                _values[(int) key] = value;
                _version++;
            }
        }

        public ICollection<SymbolType> Keys {
            get {
                return (SymbolType[]) Enum.GetValues(typeof(SymbolType));
            }
        }

        public ICollection<T> Values {
            get {
                return _values;
            }
        }

        public SymbolTypeMap() {}

        public SymbolTypeMap(T defaultValue) {
            for (int i = 0; i < _values.Length; i++) {
                _values[i] = defaultValue;
            }
        }

        internal SymbolTypeMap(IEnumerable<T> values) {
            _values = values.ToArray();
        }

        bool ICollection<KeyValuePair<SymbolType, T>>.Contains(KeyValuePair<SymbolType, T> item) {
            return object.Equals(_values[(int) item.Key],
                                 item.Value);
        }

        bool IDictionary<SymbolType, T>.ContainsKey(SymbolType key) {
            return Keys.Contains(key);
        }

        public bool ContainsValue(T value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }

            return _values.Contains(value);
        }

        internal void CopyBuffer(SymbolTypeMap<T> copyFrom) {
            Array.Copy(copyFrom._values, _values, _values.Length);
        }

        public void CopyTo(KeyValuePair<SymbolType, T>[] array, int arrayIndex) {
            if ((arrayIndex < 0) || (arrayIndex >= array.Length)) {
                throw Failure.IndexOutOfRange("arrayIndex", arrayIndex, 0, array.Length - 1);
            }

            if (array.Rank != 1) {
                throw Failure.RankNotOne("array");
            }

            if ((array.Length - arrayIndex) < Count) {
                throw Failure.NotEnoughSpaceInArray("arrayIndex", arrayIndex);
            }

            foreach (KeyValuePair<SymbolType, T> pair in this)
                array[arrayIndex++] = pair;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            _values.CopyTo(array, arrayIndex);
        }

        public override bool Equals(object obj) {
            SymbolTypeMap<T> map = obj as SymbolTypeMap<T>;
            if (map == null) {
                return false;
            }

            return _values.SequenceEqual<T>(map._values);
        }

        public IEnumerator<KeyValuePair<SymbolType, T>> GetEnumerator() {
            int start = _version;
            for (int i = 0; i < _values.Length; i++) {
                if (_version != start) {
                    throw Failure.CollectionModified();
                }

                yield return new KeyValuePair<SymbolType, T>((SymbolType) i, _values[i]);
            }
        }

        public override int GetHashCode() {
            int num = 0;
            unchecked {
                foreach (T local in _values) {
                    num += 337 * ((local == null) ? 37 : local.GetHashCode());
                }
            }

            return num;
        }

        void ICollection<KeyValuePair<SymbolType, T>>.Add(KeyValuePair<SymbolType, T> item) {
            throw new NotSupportedException();
        }

        void ICollection<KeyValuePair<SymbolType, T>>.Clear() {
            Array.Clear(_values, 0, MAX);
        }

        bool ICollection<KeyValuePair<SymbolType, T>>.Remove(KeyValuePair<SymbolType, T> item) {
            throw new NotSupportedException();
        }

        void IDictionary<SymbolType, T>.Add(SymbolType key, T value) {
            throw new NotSupportedException();
        }

        bool IDictionary<SymbolType, T>.Remove(SymbolType key) {
            throw new NotSupportedException();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        bool IDictionary<SymbolType, T>.TryGetValue(SymbolType key, out T value) {
            value = _values[(int) key];
            return true;
        }

        internal void MakeReadOnly() {
            IsReadOnly = true;
        }

        public SymbolTypeMap<T> Clone() {
            return CloneCore();
        }

        protected virtual SymbolTypeMap<T> CloneCore() {
            return new SymbolTypeMap<T>(_values);
        }
    }
}
