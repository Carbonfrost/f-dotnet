//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    public sealed class GenericInstanceTypeName : TypeSpecificationName {

        // TODO Support Update method accepting type args

        private TypeName[] typeArgs;

        public IReadOnlyList<TypeName> GenericArguments {
            get { return typeArgs; }
        }

        public TypeName GenericTypeDefinition { get { return this.ElementType; } }

        internal GenericInstanceTypeName(TypeName elementType,
                                         TypeName[] typeArgs)
            : base(elementType) {
            this.typeArgs = typeArgs;
        }


        // ICollection`1 even for ICollection<string>, which conforms with BCL implementation
        public override string Name {
            get { return ElementType.Name; }
        }

        // ICollection<System.String>
        public override string FullName {
            get {
                return MetadataNameFormat.FullNameFormat.FormatGenericInstanceType(null, this, null);
            }
        }

        public override bool IsGenericType {
            get { return true; }
        }

        public override bool IsNullable {
            get {
                return this.ElementType.FullName == "System.Nullable`1";
            }
        }

        public override GenericParameterNameCollection GenericParameters {
            get { return GenericParameterNameCollection.Empty; }
        }

        public override bool Matches(TypeName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            if (name.IsGenericType && !name.IsGenericTypeDefinition) {
                GenericInstanceTypeName other = name as GenericInstanceTypeName;
                return this.Name == other.Name
                    && this.NamespaceName.Matches(name.NamespaceName)
                    && TypeName.MatchGenericArguments(this.GenericArguments, other.GenericArguments);
            }

            return false;
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatGenericInstanceType(format, this, provider);
        }

        internal override TypeName CloneBind(TypeName declaring, MethodName method) {
            bool cloneNeeded;
            var newElementType = CloneBindElement(declaring, method, out cloneNeeded);
            var newArguments = this.typeArgs.ToArray();
            int index = 0;

            foreach (var element in this.GenericArguments) {
                var item = element.CloneBind(declaring, method);
                cloneNeeded |= !object.ReferenceEquals(item, element);
                newArguments[index] = item;
                index++;
            }

            if (cloneNeeded) {
                return new GenericInstanceTypeName(newElementType, newArguments);
            }
            return this;
        }

        protected override TypeSpecificationName UpdateOverride(TypeName elementType) {
            return new GenericInstanceTypeName(elementType, this.typeArgs);
        }
    }
}
