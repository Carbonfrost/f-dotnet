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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Carbonfrost.Commons.DotNet {

    sealed class DefaultMethodName : MethodName {

        private ParameterNameCollection _parameters;
        private GenericParameterNameCollection _genericParameters;
        private readonly string _name;
        private ParameterName _returnParameter;

        public override string Name {
            get {
                return _name;
            }
        }

        public override IReadOnlyList<TypeName> GenericArguments {
            get {
                return Array.Empty<TypeName>();
            }
        }

        public override GenericParameterNameCollection GenericParameters {
            get {
                return _genericParameters ?? GenericParameterNameCollection.Empty;
            }
        }

        public override ParameterNameCollection Parameters {
            get {
                return _parameters ?? ParameterNameCollection.Empty;
            }
        }

        public override bool HasParametersSpecified {
            get {
                return _parameters != null;
            }
        }

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

        private DefaultMethodName(
            TypeName declaringType, string methodName) : base(declaringType) {

            _name = methodName;
        }

        internal DefaultMethodName(TypeName declaringType, string methodName, params Arg[] arguments) : base(declaringType) {
            _name = methodName;
            foreach (var a in arguments) {
                a(this);
            }
        }

        internal delegate void Arg(DefaultMethodName method);

        internal static Arg SetGenericMangle(int count) {
            return result => result.FinalizeGenerics(count);
        }

        internal static Arg SetParameters(ParameterInfo[] parameters) {
            return result => result.FinalizeParameters(ParameterData.ConvertAll(parameters));
        }

        internal static Arg SetParameters(IEnumerable<TypeName> parameterTypes) {
            return result => {
                result.FinalizeParameters(
                    ParameterData.AllFromTypes(
                        BindParameterTypes(result, parameterTypes)
                    )
                );
            };
        }

        private static IEnumerable<TypeName> BindParameterTypes(DefaultMethodName method, IEnumerable<TypeName> parameters) {
            foreach (var p in parameters) {
                yield return p.CloneBind(method.DeclaringType, method);
            }
        }

        internal static Arg SetParameters(ParameterData[] pms) {
            return result => result.FinalizeParameters(pms);
        }

        internal static Arg SetReturnType(TypeName returnType) {
            return result => result.FinalizeReturnType(returnType);
        }

        internal static Arg SetGenerics(GenericParameterName[] pms) {
            return result => result.FinalizeGenerics(pms);
        }

        internal static Arg SetGenericArguments(Type[] type) {
            return result => result.FinalizeGenerics(
                type.Select((t, i) => GenericParameterName.New(result, i, t.Name)).ToArray()
            );
        }

        internal static Arg SetGenericParameters(IEnumerable<string> names) {
            return result => result.FinalizeGenerics(
                names.Select((t, i) => GenericParameterName.New(result, i, t)).ToArray()
            );
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

        internal override MethodName WithParameters(ParameterName[] pms) {
            return new DefaultMethodName(DeclaringType, _name) {
                _parameters = pms == null ? null : new ParameterNameCollection(pms),
                _genericParameters = _genericParameters,
                _returnParameter = CopyReturnParameter(this),
            };
        }

        internal MethodName WithParameters(ParameterData[] pms) {
            return new DefaultMethodName(DeclaringType, _name) {
                _parameters = ParameterData.ToArray(this, pms),
                _genericParameters = _genericParameters,
                _returnParameter = CopyReturnParameter(this),
            };
        }

        internal override MethodName WithGenericParameters(GenericParameterName[] parameters) {
            var result = new DefaultMethodName(DeclaringType, _name);
            result.FinalizeGenerics(parameters);
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
            if (names == null) {
                _genericParameters = new GenericParameterNameCollection(Array.Empty<GenericParameterName>());
            } else {
                _genericParameters = new GenericParameterNameCollection(names);
            }
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatMethod(format, this, provider);
        }

    }
}
