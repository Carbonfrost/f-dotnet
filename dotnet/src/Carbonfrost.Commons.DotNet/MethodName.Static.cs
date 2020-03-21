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
using System.Linq;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public partial class MethodName {

        public static GenericParameterName GenericParameter(int position) {
            return new UnboundGenericParameterName(position, true);
        }

        public static bool TryParse(string text, out MethodName result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out MethodName result) {
            result = null;

            if (text == null) {
                return new ArgumentNullException("text"); // $NON-NLS-1
            }
            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace("text");
            }

            try {
                result = new SignatureParser(text, false, true).ParseMethod();
                return null;
            } catch (Exception ex) {
                if (Failure.IsCriticalException(ex)) {
                    throw;
                }
                return Failure.NotParsable("text", typeof(MethodName));
            }
        }

        public static MethodName Parse(string text) {
            MethodName result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        public static MethodName FromConstructorInfo(System.Reflection.ConstructorInfo constructor) {
            if (constructor == null) {
                throw new ArgumentNullException("constructor");
            }

            return new DefaultMethodName(
                TypeName.FromType(constructor.DeclaringType),
                constructor.Name,
                DefaultMethodName.SetGenericMangle(0),
                DefaultMethodName.SetParameters(
                    ParameterData.ConvertAll(constructor.GetParameters())
                )
            );
        }

        public static MethodName FromMethodInfo(System.Reflection.MethodInfo method) {
            if (method == null) {
                throw new ArgumentNullException(nameof(method));
            }

            if (method.IsGenericMethod && !method.IsGenericMethodDefinition) {
                return new GenericInstanceMethodName(
                    FromMethodInfo(method.GetGenericMethodDefinition()),
                    method.GetGenericArguments().Select(TypeName.FromType).ToArray()
                );
            }

            var generics = DefaultMethodName.SetGenericMangle(0);
            if (method.IsGenericMethodDefinition) {
                generics = DefaultMethodName.SetGenericArguments(method.GetGenericArguments());
            }

            var result = new DefaultMethodName(
                TypeName.FromType(method.DeclaringType),
                method.Name,
                generics,
                DefaultMethodName.SetParameters(method.GetParameters()),
                DefaultMethodName.SetReturnType(TypeName.FromType(method.ReturnType))
            );
            return result;
        }
    }
}
