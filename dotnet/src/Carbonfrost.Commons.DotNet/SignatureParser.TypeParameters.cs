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

namespace Carbonfrost.Commons.DotNet {

    partial class SignatureParser {

        internal class TypeParameters {
            // Could be type parameters (because none are qualified or type specs)
            public readonly bool CouldBeParameters;
            public readonly IList<TypeName> Raw;
            public readonly bool MustBeParameters;

            public TypeParameters(IEnumerable<TypeName> raw) {
                Raw = raw.ToList();
                bool lookLikeParamNames = raw.All(t => t == null || (t.IsTypeDefinition
                                               && t.Assembly == null
                                               && t.DeclaringType == null
                                               && string.IsNullOrEmpty(t.Namespace)));
                MustBeParameters = raw.All(t => t == null);
                CouldBeParameters = lookLikeParamNames;
            }


            public IList<GenericParameterName> ConvertToGenerics(bool method) {
                Func<TypeName, int, GenericParameterName> selector =
                     (t, i) => (t == null ? new UnboundGenericParameterName(i, method)
                                          : new UnboundGenericParameterName(i, method, t.Name));
                return Raw.Select(selector).ToList();
            }
        }
    }
}
