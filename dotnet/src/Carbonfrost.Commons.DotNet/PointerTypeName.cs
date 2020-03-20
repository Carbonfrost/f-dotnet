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

namespace Carbonfrost.Commons.DotNet {

    public sealed class PointerTypeName : TypeSpecificationName {

        internal PointerTypeName(TypeName elementType)
            : base(elementType) {
        }

        public override string Name {
            get { return ElementType.Name + "*"; }
        }

        public override string FullName {
            get { return ElementType.FullName + "*"; }
        }

        public override bool IsPointer {
            get { return true; }
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatPointerType(format, this, provider);
        }

        public override bool Matches(TypeName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.IsPointer) {
                var pointer = (PointerTypeName) name;

                return this.ElementType.Matches(pointer.ElementType);
            }

            return false;
        }

        protected override TypeSpecificationName UpdateOverride(TypeName elementType) {
            return new PointerTypeName(elementType);
        }
    }
}
