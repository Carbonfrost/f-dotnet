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

    public abstract partial class GenericParameterName : TypeName {

        public abstract MethodName DeclaringMethod { get; }
        public abstract int Position { get; }
        internal virtual bool IsMethodGenericParameter {
            get {
                return DeclaringMethod != null;
            }
        }

        public bool IsPositional {
            get { return string.IsNullOrEmpty(this.Name) || this.Name[0] == '`'; } }

        public sealed override bool IsGenericParameter {
            get { return true; }
        }

        public virtual GenericParameterName DeclaringGenericParameter {
            get {
                return null;
            }
        }

        public override string Namespace {
            get {
                if (DeclaringType == null) {
                    return null;
                }
                return DeclaringType.Namespace;
            }
        }

        public abstract override string Name { get; }

        public override string FullName {
            get { return this.Name; }
        }

        public sealed override GenericParameterNameCollection GenericParameters {
            get { return GenericParameterNameCollection.Empty; }
        }

        internal GenericParameterName(TypeName declaring)
            : base(declaring) {}

        internal GenericParameterName() {}

        public new GenericParameterName WithNamespace(string ns) {
            return (GenericParameterName) base.WithNamespace(ns);
        }

        public new GenericParameterName WithNamespace(NamespaceName ns) {
            return (GenericParameterName) base.WithNamespace(ns);
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            if (DeclaringMethod != null) {
                return UpdateOverride(DeclaringMethod.WithAssembly(assembly));
            }

            return UpdateOverride(DeclaringType.WithAssembly(assembly));
        }

        protected abstract GenericParameterName UpdateOverride(TypeName declaringType);
        protected abstract GenericParameterName UpdateOverride(MethodName declaringMethod);

        protected override TypeName WithNamespaceOverride(string ns) {
            return UpdateOverride(DeclaringType.WithNamespace(ns));
        }

        internal sealed override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatGenericParameter(format, this, provider);
        }

        internal static GenericParameterName New(TypeName declaring, int position, string name) {
            return new BoundGenericParameterName(declaring, position, name);
        }

        internal static GenericParameterName New(MethodName declaring, int position, string name) {
            return new BoundGenericParameterName(declaring, position, name);
        }

        internal abstract GenericParameterName Clone();

        internal sealed override TypeName CloneBind(TypeName declaring,
                                                    MethodName method) {
            var parms = IsMethodGenericParameter
                ? method.GenericParameters
                : declaring.GenericParameters;

            if (Position >= parms.Count) {
                throw DotNetFailure.CannotBindGenericParameterName();
            } else {
                return parms[Position];
            }
        }

    }

}
