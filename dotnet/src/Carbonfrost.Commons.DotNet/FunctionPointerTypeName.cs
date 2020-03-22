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

namespace Carbonfrost.Commons.DotNet {

    public sealed class FunctionPointerTypeName : TypeName, INameWithParameters<FunctionPointerTypeName> {

        private readonly ParameterNameCollection _parms;
        private readonly TypeName _returnType;

        public TypeName ReturnType { get { return _returnType; } }

        public override bool IsFunctionPointer { get { return true; } }

        internal FunctionPointerTypeName(IEnumerable<ParameterName> parms, TypeName returnType) {
            _parms = new ParameterNameCollection(parms.ToArray());
            _returnType = returnType;
        }

        public ParameterNameCollection Parameters {
            get {
                return _parms;
            }
        }

        public int ParameterCount {
            get {
                return Parameters.Count;
            }
        }

        bool INameWithParameters.HasParametersSpecified {
            get {
                return true;
            }
        }

        internal override TypeName CloneBind(TypeName declaring, MethodName method) {
            return this;
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatFunctionPointerType(format, this, provider);
        }

        public FunctionPointerTypeName Update() {
            throw new NotImplementedException();
        }

        protected override TypeName WithNamespaceOverride(string ns) {
            throw new NotSupportedException();
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            throw new NotSupportedException();
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            throw new NotSupportedException();
        }

        public override string Namespace {
            get {
                throw new NotImplementedException();
            }
        }

        public override string Name {
            get {
                throw new NotImplementedException();
            }
        }

        public override GenericParameterNameCollection GenericParameters {
            get {
                return GenericParameterNameCollection.Empty;
            }
        }

        public FunctionPointerTypeName AddParameter(string name) {
            return AddParameter(name, (TypeName) null);
        }

        public FunctionPointerTypeName AddParameter(TypeName parameterType) {
            return AddParameter(null, parameterType);
        }

        public FunctionPointerTypeName AddParameter(string name, TypeName parameterType) {
            return AddParameter(name, parameterType, null, null);
        }

        public FunctionPointerTypeName AddParameter(string name,
                                       TypeName parameterType,
                                       IEnumerable<TypeName> requiredModifiers,
                                       IEnumerable<TypeName> optionalModifiers) {
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            return WithParameters(Parameters.ImmutableAdd(
                new DefaultParameterName(this, Parameters.Count, name, parameterType, modifiers), CloneParameter
            ));
        }

        public FunctionPointerTypeName RemoveParameters() {
            return WithParameters(Array.Empty<ParameterName>());
        }

        public FunctionPointerTypeName RemoveParameterAt(int index) {
            return WithParameters(Parameters.ImmutableRemoveAt(index, CloneParameter));
        }

        public FunctionPointerTypeName RemoveParameter(ParameterName parameter) {
            return WithParameters(Parameters.ImmutableRemove(parameter, CloneParameter));
        }

        public FunctionPointerTypeName InsertParameterAt(int index, string name) {
            return WithParameters(Parameters.ImmutableInsertAt(
                index, new DefaultParameterName(this, index, null, null, null), CloneParameter
            ));
        }

        public FunctionPointerTypeName InsertParameterAt(int index, TypeName parameterType) {
            return WithParameters(Parameters.ImmutableInsertAt(
                index, new DefaultParameterName(this, index, null, parameterType, null), CloneParameter
            ));
        }

        public FunctionPointerTypeName InsertParameterAt(int index, string name, TypeName parameterType) {
            return WithParameters(Parameters.ImmutableInsertAt(
                index, new DefaultParameterName(this, index, name, parameterType, null), CloneParameter
            ));
        }

        public FunctionPointerTypeName InsertParameterAt(int index, string name, TypeName parameterType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers) {
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            return WithParameters(Parameters.ImmutableInsertAt(
                index, new DefaultParameterName(this, index, name, parameterType, modifiers), CloneParameter
            ));
        }

        public FunctionPointerTypeName SetParameter(int index, string name, TypeName parameterType) {
            return SetParameter(index, name, parameterType, null, null);
        }

        public FunctionPointerTypeName SetParameter(int index,
            string name,
            TypeName parameterType,
            IEnumerable<TypeName> requiredModifiers,
            IEnumerable<TypeName> optionalModifiers
        ) {
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            return WithParameters(
                Parameters.ImmutableSet(index, new DefaultParameterName(this, index, name, parameterType, modifiers), CloneParameter)
            );
        }

        private FunctionPointerTypeName WithParameters(ParameterName[] items) {
            return new FunctionPointerTypeName(items, ReturnType);
        }

        private ParameterName CloneParameter(ParameterName t, int index) {
            return new DefaultParameterName(this, index, t.Name, t.ParameterType, t.Modifiers);
        }
    }
}
