//s
// Copyright 2013, 2015, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public abstract partial class MethodSpecificationName : MethodName {

        private readonly MethodName _elementName;

        public sealed override bool IsMethodSpecification {
            get { return true; }
        }

        public MethodName ElementName {
            get { return _elementName; }
        }

        internal MethodSpecificationName(MethodName elementName) : base(elementName.DeclaringType) {
            if (elementName == null)
                throw new ArgumentNullException("elementName");

            _elementName = elementName;
        }

        public override ParameterName ReturnParameter {
            get {
                return ElementName.ReturnParameter;
            }
        }

        public override GenericParameterNameCollection GenericParameters {
            get {
                return GenericParameterNameCollection.Empty;
            }
        }

        public override int GenericParameterCount {
            get {
                return 0;
            }
        }

        public override ParameterNameCollection Parameters {
            get {
                return ElementName.Parameters;
            }
        }

        public override bool HasParametersSpecified {
            get {
                return ElementName.HasParametersSpecified;
            }
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            throw DotNetFailure.NotSupportedBySpecifications();
        }

        public override MethodName WithReturnParameter(TypeName returnType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalModifiers) {
            throw DotNetFailure.NotSupportedBySpecifications();
        }

        internal override MethodName WithParameters(ParameterName[] parameters) {
            throw DotNetFailure.NotSupportedBySpecifications();
        }

        public sealed override MethodName SetGenericParameter(int index, string name) {
            throw DotNetFailure.NotSupportedBySpecifications();
        }

        public sealed override MethodName WithName(string name) {
            throw DotNetFailure.NotSupportedBySpecifications();
        }
    }
}
