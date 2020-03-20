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
using System.Linq;

namespace Carbonfrost.Commons.DotNet {

    public sealed class ArrayTypeName : TypeSpecificationName {

        private ArrayDimensionCollection dimensions;

        public ArrayDimensionCollection Dimensions {
            get { return dimensions; }
        }

        internal ArrayTypeName(TypeName elementType, ArrayDimension[] dimensions)
            : base(elementType) {
            this.dimensions = new ArrayDimensionCollection(dimensions);
        }

        public override string Name {
            get { return ElementType.Name + Suffix; }
        }

        public override string FullName {
            get { return ElementType.FullName + Suffix; }
        }

        string Suffix {
            get {
                return string.Concat("[", Dimensions, "]");
            }
        }

        public override bool IsArray {
            get { return true; }
        }

        public override bool Matches(TypeName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name.IsArray) {
                var array = (ArrayTypeName) name;

                // Must exactly match dimensions
                return this.ElementType.Matches(array.ElementType)
                    && array.Dimensions.Count == this.Dimensions.Count
                    && this.Dimensions.Zip(array.Dimensions,
                                           (t, u) => t == u).AllTrue();
            }

            return false;
        }

        internal override TypeName CloneBind(TypeName declaring, MethodName method) {
            bool result;
            TypeName et = CloneBindElement(declaring, method, out result);
            if (result)
                return new ArrayTypeName(et, this.dimensions.ToArray());
            else
                return this;
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatArrayType(format, this, provider);
        }

        protected override TypeSpecificationName UpdateOverride(TypeName elementType) {
            return new ArrayTypeName(elementType, this.dimensions.ToArray());
        }
    }
}
