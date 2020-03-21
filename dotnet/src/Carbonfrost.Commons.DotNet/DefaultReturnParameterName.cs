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

namespace Carbonfrost.Commons.DotNet {

    class DefaultReturnParameterName : ParameterName {

        private readonly MemberName _member;
        private readonly ModifierCollection _modifiers;
        private readonly TypeName _parameterType;

        internal DefaultReturnParameterName(MemberName member, TypeName parameterType, ModifierCollection modifiers) {
            _parameterType = parameterType;
            _member = member;
            if (modifiers != null) {
                _modifiers = ModifierCollection.Empty;
            }
        }

        internal override MemberName Member {
            get {
                return _member;
            }
        }

        internal override ModifierCollection Modifiers {
            get {
                return _modifiers;
            }
        }

        public override string Name {
            get {
                return null;
            }
        }

        public override TypeName ParameterType {
            get {
                return _parameterType;
            }
        }

        public override int Position {
            get {
                return -1;
            }
        }

    }
}
