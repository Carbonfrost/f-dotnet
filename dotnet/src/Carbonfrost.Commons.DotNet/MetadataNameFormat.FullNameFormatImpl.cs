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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    partial class MetadataNameFormat {

        class FullNameFormatImpl : MetadataNameFormat {

            private readonly bool skipDecl;

            public FullNameFormatImpl(bool skip) : base(false) {
                this.skipDecl = skip;
            }

            protected internal override string FormatGenericInstanceMethod(string format, GenericInstanceMethodName name, IFormatProvider formatProvider) {
                return this.FormatMethod(format, name, formatProvider);
            }

            protected internal override string FormatGenericInstanceType(string format, GenericInstanceTypeName name, IFormatProvider formatProvider) {
                string nameWithoutMangle = Regex.Replace(name.ElementType.FullName, @"`\d", string.Empty);
                MetadataNameFormat options = this.GetFormatHelper(formatProvider);

                StringBuilder sb = new StringBuilder(nameWithoutMangle);
                options.PrintGenericArgs(sb, name.GenericArguments);
                return sb.ToString();
            }

            protected internal override string FormatMethod(string format, MethodName name, IFormatProvider formatProvider) {
                StringBuilder buffer = new StringBuilder();
                // System.IComparer.CompareTo(System.Object x, System.Object y)
                if (!this.skipDecl && name.DeclaringType != null) {
                    BuildFullName(buffer, name.DeclaringType, formatProvider);
                    AppendNameSeparator(buffer, name.Name);
                }

                var options = GetFormatHelper(formatProvider);

                buffer.Append(name.Name);
                PrintGenericParamsOrArgs(buffer, name, options.UseGenericParameterPositions);

                if (name.HasParametersSpecified) {
                    AppendMethodParameters(buffer,
                                           name.Parameters,
                                           options.GetDefaultFormat(formatProvider, SymbolType.Parameter),
                                           formatProvider);
                }
                return buffer.ToString();
            }
        }
    }

}
