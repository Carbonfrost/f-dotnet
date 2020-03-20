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

namespace Carbonfrost.Commons.DotNet {

    public abstract partial class TypeSpecificationName : TypeName {

        public TypeName ElementType {
            get;
            private set;
        }

        public sealed override bool IsTypeSpecification {
            get {
                return true;
            }
        }

        internal TypeSpecificationName(TypeName elementType) : base(null) {
            if (elementType == null) {
                throw new ArgumentNullException("elementType");
            }

            ElementType = elementType;
        }

        public override AssemblyName Assembly {
            get {
                return ElementType.Assembly;
            }
        }

        public override string Name {
            get {
                return ElementType.Name;
            }
        }

        public sealed override string Namespace {
            get {
                return ElementType.Namespace;
            }
        }

        public override string FullName {
            get {
                return ElementType.FullName;
            }
        }

        public override GenericParameterNameCollection GenericParameters {
            get {
                return GenericParameterNameCollection.Empty;
            }
        }

        public TypeSpecificationName WithElementType(TypeName elementType) {
            return (TypeSpecificationName) UpdateOverride(elementType);
        }

        public new TypeSpecificationName WithNamespace(string ns) {
            return (TypeSpecificationName) base.WithNamespace(ns);
        }

        protected sealed override TypeName WithNamespaceOverride(string ns) {
            return UpdateOverride(ElementType.WithNamespace(ns));
        }

        protected sealed override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return UpdateOverride(ElementType.WithAssembly(assembly));
        }

        protected sealed override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            return UpdateOverride(ElementType.WithDeclaringType(declaringType));
        }

        protected abstract TypeSpecificationName UpdateOverride(TypeName elementType);

        internal TypeName CloneBindElement(TypeName parent, MethodName method, out bool cloneNeeded) {
            var item = this.ElementType.CloneBind(parent, method);
            cloneNeeded = !object.ReferenceEquals(item, this.ElementType);
            return item;
        }
    }

}
