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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    class ModifierCollection {

        private readonly List<Modifier> _items = new List<Modifier>();

        public static readonly ModifierCollection Empty = new ModifierCollection();

        public int Count {
            get {
                return _items.Count;
            }
        }

        public IReadOnlyCollection<TypeName> RequiredModifiers {
            get { return new View(this, true); }
        }

        public IReadOnlyCollection<TypeName> OptionalModifiers {
            get { return new View(this, false); }
        }

        public ModifierCollection() {}

        public ModifierCollection(IEnumerable<TypeName> required,
                                  IEnumerable<TypeName> optional) {
            if (required != null) {
                foreach (var r in required) {
                    _items.Add(new Modifier(r, true));
                }
            }
            if (optional != null) {
                foreach (var r in optional) {
                    _items.Add(new Modifier(r, false));
                }
            }
        }

        public bool Match(ModifierCollection other) {
            if (_items.Count != other._items.Count) {
                return false;
            }
            foreach (var kvp in _items) {
                if (!other._items.Contains(kvp)) {
                    return false;
                }
            }
            return true;
        }

        public override string ToString() {
            var req = _items.Where(t => t.Required)
                            .Select(t => string.Format("modreq({0})", t.Type));
            var opt = _items.Where(t => !t.Required)
                            .Select(t => string.Format("modopt({0})", t.Type));
            return string.Join(" ", req.Concat(opt));
        }

        struct Modifier {

            public readonly TypeName Type;
            public readonly bool Required;

            public Modifier(TypeName t, bool required) {
                Type = t;
                Required = required;
            }
        }

        struct View : IReadOnlyCollection<TypeName> {

            private readonly ModifierCollection _parent;
            private readonly bool _predicate;

            public int Count {
                get {
                    bool predicate = _predicate;
                    return _parent._items.Count(t => t.Required == predicate);
                }
            }

            public View(ModifierCollection items, bool predicate) {
                _parent = items;
                _predicate = predicate;
            }

            public bool Contains(TypeName type) {
                return this.Any(t => t == type);
            }

            IEnumerator IEnumerable.GetEnumerator() {
                return GetEnumerator();
            }

            public IEnumerator<TypeName> GetEnumerator() {
                foreach (var m in _parent._items) {
                    if (_predicate == m.Required) {
                        yield return m.Type;
                    }
                }
            }
        }
    }
}
