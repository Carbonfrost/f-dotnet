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

namespace Carbonfrost.Commons.DotNet {

    partial class TypeName {

        public static readonly TypeName Attribute = TypeName.FromType(typeof(Attribute));
        public static readonly TypeName Object = TypeName.FromType(typeof(Object));

        public static readonly TypeName Void = TypeName.FromType(typeof(void));
        public static readonly TypeName Single = TypeName.FromType(typeof(float));
        public static readonly TypeName Double = TypeName.FromType(typeof(double));
        public static readonly TypeName Float32 = Single;
        public static readonly TypeName Float64 = Double;
        public static readonly TypeName Int32 = TypeName.FromType(typeof(int));
        public static readonly TypeName Int16 = TypeName.FromType(typeof(short));
        public static readonly TypeName Int64 = TypeName.FromType(typeof(long));
        public static readonly TypeName UInt32 = TypeName.FromType(typeof(uint));
        public static readonly TypeName UInt16 = TypeName.FromType(typeof(ushort));
        public static readonly TypeName UInt64 = TypeName.FromType(typeof(ulong));
        public static readonly TypeName Decimal = TypeName.FromType(typeof(decimal));

        public static readonly TypeName String = TypeName.FromType(typeof(string));

        public static GenericParameterName GenericParameter(int position) {
            return new UnboundGenericParameterName(position, false);
        }
    }
}
