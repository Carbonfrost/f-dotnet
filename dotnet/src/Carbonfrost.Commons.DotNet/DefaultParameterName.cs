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
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    class DefaultParameterName : ParameterName {

        private readonly string _name;
        private readonly TypeName _parameterType;
        private readonly int _position;
        private readonly MemberName _member;
        private readonly ModifierCollection _modifiers;

        public override int Position { get { return _position; } }

        public override TypeName ParameterType { get { return _parameterType; } }
        internal override MemberName Member { get { return _member; } }

        public override string Name { get { return _name; } }

        internal override ModifierCollection Modifiers {
            get {
                return _modifiers;
            }
        }

        internal DefaultParameterName(MemberName member,
                                      int position,
                                      string name,
                                      TypeName parameterType,
                                      ModifierCollection modifiers) {
            _parameterType = parameterType;
            _name = name;
            _member = member;
            _position = position;
            _modifiers = modifiers ?? ModifierCollection.Empty;
        }

    }
}
