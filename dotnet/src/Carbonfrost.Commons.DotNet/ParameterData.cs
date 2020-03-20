//
// Copyright 2013, 2015, 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    struct ParameterData {

        public readonly string Name;
        public readonly TypeName Type;

        public static readonly ParameterData Empty = new ParameterData(string.Empty, null);

        public ParameterData(string name, TypeName type) {
            Name = name;
            Type = type;
        }

        public static ParameterNameCollection ToArray(MemberName member, IEnumerable<ParameterData> items) {
            if (items == null) {
                return null;
            }

            Func<TypeName, TypeName> convertType = t => {
                if (t == null) {
                    return null;
                }
                return t.CloneBind(member.DeclaringType, member as MethodName);
            };
            var result = items.Select((t, i) => new DefaultParameterName(member, i, t.Name, convertType(t.Type), null))
                .ToArray();
            return new ParameterNameCollection(result);
        }

        public static ParameterData[] AsData(IEnumerable<ParameterName> items) {
            return items.Select(t => new ParameterData(t.Name, t.ParameterType)).ToArray();
        }

        internal static ParameterData[] AllFromTypes(IEnumerable<TypeName> types) {
            return types.Select(t => new ParameterData(null, t)).ToArray();
        }

        internal static ParameterData[] ConvertAll(
            IEnumerable<System.Reflection.ParameterInfo> all)
        {
            return all.Select(t => new ParameterData(t.Name, TypeName.FromType(t.ParameterType))).ToArray();
        }
    }
}
