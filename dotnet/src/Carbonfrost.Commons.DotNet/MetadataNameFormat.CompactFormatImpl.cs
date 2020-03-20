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
using System.Text;

namespace Carbonfrost.Commons.DotNet {

    partial class MetadataNameFormat {

        class CompactFormatImpl : MetadataNameFormat {

            internal CompactFormatImpl()
                : base(false) {}

            protected internal override string FormatParameter(string format, ParameterName name, IFormatProvider formatProvider) {
                return name.ParameterType.Name;
            }

            protected internal override string FormatMethod(string format,
                                                            MethodName name,
                                                            IFormatProvider formatProvider) {

                StringBuilder buffer = new StringBuilder();

                // IComparer.CompareTo(Object, Object)
                if (name.DeclaringType != null) {
                    buffer.Append(name.DeclaringType.Name);
                    AppendNameSeparator(buffer, name.Name);
                }

                buffer.Append(name.Name);
                PrintGenericParamsOrArgs(buffer, name, this.UseGenericParameterPositions);
                if (name.HasParametersSpecified) {
                    AppendMethodParameters(buffer, name.Parameters, "Cv", formatProvider);
                }

                return buffer.ToString();
            }
        }
    }

}
