//
// Copyright 2013, 2017, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

namespace Carbonfrost.Commons.DotNet {

    sealed class BoundGenericParameterName : GenericParameterName {

        private readonly string _name;
        private readonly int _position;
        private readonly MethodName _declaringMethod;

        internal BoundGenericParameterName(MethodName declaring, int position, string name)
            : base(RequireArg(declaring).DeclaringType)
        {
            _name = name;
            _declaringMethod = declaring;
            _position = position;
        }

        internal BoundGenericParameterName(TypeName declaring, int position, string name) : base(declaring) {
            _position = position;

            if (string.IsNullOrEmpty(name)) {
                var type = declaring.DeclaringType;

                while (type != null) {
                    position += type.GenericParameters.Count;
                    type = type.DeclaringType;
                }

                _name = "`" + position;
            } else {
                _name = name;
            }
        }

        static MethodName RequireArg(MethodName declaring) {
            if (declaring == null) {
                throw new ArgumentNullException("declaring");
            }

            return declaring;
        }

        internal override GenericParameterName Clone() {
            if (_declaringMethod == null) {
                return new BoundGenericParameterName(DeclaringType, _position, _name);
            }
            return new BoundGenericParameterName(_declaringMethod, _position, _name);
        }

        protected override GenericParameterName UpdateOverride(TypeName declaringType) {
            throw new NotImplementedException();
        }

        protected override GenericParameterName UpdateOverride(MethodName declaringMethod) {
            throw new NotImplementedException();
        }

        public override string Namespace {
            get { return DeclaringType.Namespace; }
        }

        public override string Name {
            get {
                return _name;
            }
        }

        public override string FullName {
            get { return Name; }
        }

        public override int Position {
            get {
                return _position;
            }
        }

        public override MethodName DeclaringMethod {
            get {
                return _declaringMethod;
            }
        }
    }
}
