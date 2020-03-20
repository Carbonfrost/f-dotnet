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

namespace Carbonfrost.Commons.DotNet {

    sealed class DefaultMethodName : MethodName {

        private ParameterNameCollection _parameters;
        private GenericParameterNameCollection _genericParameters;
        private string _name;
        private ParameterName _returnParameter;

        public override string Name { get { return _name; } }

        public override IReadOnlyList<TypeName> GenericArguments { get { return Empty<TypeName>.ReadOnlyList; } }
        public override GenericParameterNameCollection GenericParameters {
            get {
                return _genericParameters ?? GenericParameterNameCollection.Empty;
            }
        }
        public override ParameterNameCollection Parameters { get { return _parameters ?? ParameterNameCollection.Empty; } }

        public override bool HasParametersSpecified { get { return _parameters != null; } }

        public override ParameterName ReturnParameter {
            get {
                return _returnParameter;
            }
        }

        public override int GenericParameterCount {
            get {
                return GenericParameters.Count;
            }
        }

        internal DefaultMethodName(
            TypeName declaringType, string methodName) : base(declaringType) {

            _name = methodName;
        }

        internal DefaultMethodName(
            TypeName declaringType, string methodName, TypeName returnType) : base(declaringType) {

            _name = methodName;
            _returnParameter = new DefaultReturnParameterName(this, returnType, null);
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            var result = new DefaultMethodName(declaringType, _name);
            result._parameters = _parameters;
            result._genericParameters = _genericParameters;
            result._returnParameter = CopyReturnParameter(this);
            return result;
        }

        private ParameterName CopyReturnParameter(MethodName parent) {
            if (_returnParameter == null) {
                return null;
            }
            return new DefaultReturnParameterName(parent, _returnParameter.ParameterType, _returnParameter.Modifiers);
        }

        public override MethodName WithName(string name) {
            var result = new DefaultMethodName(DeclaringType, name);
            result._parameters = _parameters;
            result._genericParameters = _genericParameters;
            result._returnParameter = CopyReturnParameter(this);
            return result;
        }

        public override MethodName WithReturnParameter(TypeName returnType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers) {
            var result = new DefaultMethodName(DeclaringType, _name);
            result._parameters = _parameters;
            result._genericParameters = _genericParameters;
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            result._returnParameter = new DefaultReturnParameterName(this, returnType, modifiers);
            return result;
        }

        public override MethodName SetGenericParameter(int index, string name) {
            return WithGenericParameters(ImmutableUtility.Set(
                _genericParameters,
                index,
                (t, _) => t.Clone(),
                GenericParameterName.New(this, index, name)));
        }

        internal override MethodName WithParameters(ParameterName[] pms) {
            return new DefaultMethodName(DeclaringType, _name) {
                _parameters = pms == null ? null : new ParameterNameCollection(pms),
                _genericParameters = _genericParameters,
                _returnParameter = CopyReturnParameter(this),
            };
        }

        private MethodName WithGenericParameters(GenericParameterName[] pms) {
            var result = new DefaultMethodName(DeclaringType, _name);
            result.FinalizeGenerics(pms);
            result.FinalizeParameters(ParameterData.AsData(_parameters).ToArray());

            if (ReturnType != null) {
                result.FinalizeReturnType(ReturnType.CloneBind(DeclaringType, result));
            }
            return result;
        }

        internal void FinalizeReturnType(TypeName returnType) {
            _returnParameter = new DefaultReturnParameterName(this, returnType, null);
        }

        internal void FinalizeParameters(ParameterData[] pms) {
            _parameters = ParameterData.ToArray(this, pms);
        }

        internal void FinalizeGenerics(int count) {
            GenericParameterName[] names = new GenericParameterName[count];
            if (count == 1)
                names[0] = GenericParameterName.New(this, 0, "``0");
            else {
                for (int i = 0; i < count; i++)
                    names[i] = GenericParameterName.New(this, i, "``" + i);
            }

            _genericParameters = new GenericParameterNameCollection(names);
        }

        internal void FinalizeGenerics(GenericParameterName[] names) {
            if (names == null)
                _genericParameters = new GenericParameterNameCollection(Empty<GenericParameterName>.Array);
            else
                _genericParameters = new GenericParameterNameCollection(names);
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatMethod(format, this, provider);
        }

    }
}
