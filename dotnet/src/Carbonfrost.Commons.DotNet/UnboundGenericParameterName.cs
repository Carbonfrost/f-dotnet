//
// Copyright 2013, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.DotNet {

    class UnboundGenericParameterName : GenericParameterName {

        private readonly int _position;
        private readonly bool _method;
        private readonly string _name;

        internal UnboundGenericParameterName(int position, bool method, string name = null) {
            _position = position;
            _method = method;
            if (name == null) {
                if (_method) {
                    _name = "``" + _position;
                } else {
                    _name = "`" + _position;
                }
            } else {
                _name = name;
            }
        }

        internal override bool IsMethodGenericParameter {
            get {
                return _method;
            }
        }

        public override int Position {
            get {
                return _position;
            }
        }

        public override MethodName DeclaringMethod {
            get {
                return null;
            }
        }

        public override string Name {
            get {
                return _name;
            }
        }

        protected override GenericParameterName UpdateOverride(MethodName declaringMethod) {
            throw new NotImplementedException();
        }

        protected override GenericParameterName UpdateOverride(TypeName declaringType) {
            throw new NotImplementedException();
        }

        internal override GenericParameterName Clone() {
            return new UnboundGenericParameterName(_position, _method);
        }
    }
}
