//
// Copyright 2020 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    partial class MethodName {

        public MethodName AddGenericParameter(string name) {
            return WithGenericParameters(GenericParameters.ImmutableAdd(
                GenericParameterName.New(this, GenericParameters.Count, name),
                CloneGenericParameter)
            );
        }

        public MethodName SetGenericParameter(int index, string name) {
            return WithGenericParameters(
                GenericParameters.ImmutableSet(index, GenericParameterName.New(this, index, name), CloneGenericParameter)
            );
        }

        public MethodName RemoveGenericParameters() {
            return WithGenericParameters(Array.Empty<GenericParameterName>());
        }

        internal abstract MethodName WithGenericParameters(GenericParameterName[] parameters);

        public MethodName RemoveGenericParameterAt(int index) {
            return WithGenericParameters(GenericParameters.ImmutableRemoveAt(index, CloneGenericParameter));
        }

        public MethodName RemoveGenericParameter(GenericParameterName parameter) {
            return WithGenericParameters(GenericParameters.ImmutableRemove(parameter, CloneGenericParameter));
        }

        public MethodName InsertGenericParameterAt(int index, string name) {
            return WithGenericParameters(GenericParameters.ImmutableInsertAt(
                index, GenericParameterName.New(this, index, name), CloneGenericParameter
            ));
        }

        public MethodName InsertGenericParameterAt(int index) {
            return InsertGenericParameterAt(index, null);
        }

        private GenericParameterName CloneGenericParameter(GenericParameterName t, int index) {
            return GenericParameterName.New(this, index, t.Name);
        }
    }
}
