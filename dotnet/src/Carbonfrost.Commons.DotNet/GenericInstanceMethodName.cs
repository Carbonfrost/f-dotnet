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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Carbonfrost.Commons.DotNet {

    public sealed partial class GenericInstanceMethodName : MethodSpecificationName {

        private readonly TypeName[] _arguments;

        internal GenericInstanceMethodName(MethodName elementName,
                                           TypeName[] arguments)
            : base(elementName) {
            _arguments = arguments;
        }

        public override string Name {
            get {
                return ElementName.Name;
            }
        }

        public override IReadOnlyList<TypeName> GenericArguments {
            get { return new ReadOnlyList<TypeName>(_arguments); } }

        public override bool IsGenericMethod {
            get { return true; }
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatGenericInstanceMethod(format, this, provider);
        }
    }
}
