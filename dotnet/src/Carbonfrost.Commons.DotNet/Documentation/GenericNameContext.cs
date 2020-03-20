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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet.Documentation {

    struct GenericNameContext {

        private readonly TypeName _type;
        private readonly MethodName _method;

        public TypeName Type {
            get {
                return _type;
            }
        }

        public MethodName Method {
            get {
                return _method;
            }
        }

        public GenericNameContext(MethodName method, TypeName type) {
            _method = method;
            _type = type;
        }

        public static readonly GenericNameContext Empty = new GenericNameContext(null, null);

        public static GenericNameContext Create(MethodName m) {
            if (m == null) {
                return Empty;
            }
            return new GenericNameContext(m, m.DeclaringType);
        }

        public static GenericNameContext Create(TypeName m) {
            if (m == null) {
                return Empty;
            }
            return new GenericNameContext(null, m);
        }
    }
}
