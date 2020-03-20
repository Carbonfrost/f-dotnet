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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.DotNet {

    public sealed class FunctionPointerTypeName : TypeName, INameWithParameters {

        private readonly ParameterNameCollection _parms;
        private readonly TypeName _returnType;

        public TypeName ReturnType { get { return _returnType; } }

        public override bool IsFunctionPointer { get { return true; } }

        internal FunctionPointerTypeName(IEnumerable<ParameterName> parms, TypeName returnType) {
            _parms = new ParameterNameCollection(parms.ToArray());
            _returnType = returnType;
        }

        public ParameterNameCollection Parameters {
            get { return _parms; }
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
            throw new NotImplementedException();
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            throw new NotImplementedException();
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
    }
}
