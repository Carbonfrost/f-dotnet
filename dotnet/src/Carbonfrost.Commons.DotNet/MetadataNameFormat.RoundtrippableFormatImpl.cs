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

        class RoundtrippableFormatImpl : MetadataNameFormat {

            protected internal override string FormatMethod(string format, MethodName name, IFormatProvider formatProvider) {
                StringBuilder buffer = new StringBuilder();
                // System.IComparer.CompareTo(x:[mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Object, y:[mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]System.Object), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
                if (name.DeclaringType != null) {
                    BuildFullName(buffer, name.DeclaringType, formatProvider);
                    AppendNameSeparator(buffer, name.Name);
                }

                buffer.Append(name.Name);
                PrintGenericParamsOrArgs(buffer, name, false);
                if (name.HasParametersSpecified) {
                    DoAppendParameters(buffer, name);
                }

                if (name.Assembly != null) {
                    buffer.Append(", ");
                    buffer.Append(name.Assembly.FullName);
                }
                return buffer.ToString();
            }

            protected internal override string FormatGenericInstanceMethod(string format, GenericInstanceMethodName name, IFormatProvider formatProvider) {
                return this.FormatMethod(null, name, formatProvider);
            }

            private void DoAppendParameters(StringBuilder buffer, MethodName name) {
                buffer.Append('(');

                bool needComma = false;
                foreach (var p in name.Parameters) {
                    if (needComma) {
                        buffer.Append(", ");
                    }

                    buffer.Append(p.ToString("U", null));
                    needComma = true;
                }
                buffer.Append(')');
            }
        }
    }
}
