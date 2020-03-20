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

namespace Carbonfrost.Commons.DotNet {

    static class Empty<T> {

        public static readonly T[] Array  = new T[0];

        public static readonly Func<T, T> Func = t => t;

        public static readonly ReadOnlyCollection<T> ReadOnlyCollection
            = new ReadOnlyCollection<T>(Array);

        public static readonly IReadOnlyList<T> ReadOnlyList = Array;

    }
}
