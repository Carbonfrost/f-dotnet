//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

using System.Collections.Generic;

namespace Carbonfrost.Commons.DotNet {

    interface INameWithParameters {
        int ParameterCount { get; }
        bool HasParametersSpecified { get; }
        ParameterNameCollection Parameters { get; }
    }

    interface INameWithParameters<TSelf> : INameWithParameters {
        TSelf AddParameter(string name);
        TSelf AddParameter(TypeName parameterType);
        TSelf AddParameter(string name, TypeName parameterType);
        TSelf AddParameter(string name, TypeName parameterType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers);

        TSelf RemoveParameters();
        TSelf RemoveParameterAt(int index);
        TSelf RemoveParameter(ParameterName parameter);

        TSelf InsertParameterAt(int index, string name);
        TSelf InsertParameterAt(int index, TypeName parameterType);
        TSelf InsertParameterAt(int index, string name, TypeName parameterType);
        TSelf InsertParameterAt(int index, string name, TypeName parameterType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers);

        TSelf SetParameter(int index, string name, TypeName parameterType);
        TSelf SetParameter(int index, string name, TypeName parameterType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers);
    }
}
