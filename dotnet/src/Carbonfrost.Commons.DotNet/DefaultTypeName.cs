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
using System.Collections.Generic;
using System.Linq;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    sealed class DefaultTypeName : TypeName {

        private readonly string _fullName;
        private readonly string _name;
        private readonly string _namespace;
        private readonly AssemblyName _assembly;
        private GenericParameterNameCollection _genericParameters;

        internal DefaultTypeName(AssemblyName assembly,
                                 string name,
                                 string namespaceText) : base(null) {

            if (namespaceText == null) {
                throw new ArgumentNullException("namespaceText");
            }

            _fullName = TypeName.Combine(namespaceText, name);
            _name = name;
            _namespace = namespaceText;
            _assembly = assembly;
        }

        internal DefaultTypeName(AssemblyName assembly,
                                 string name,
                                 string namespaceText,
                                 int generics) : this(assembly, name, namespaceText) {
            FinalizeGenerics(generics);
        }

        internal DefaultTypeName(string name,
                                 DefaultTypeName declaringType) : base(declaringType) {

            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            _fullName = string.Concat(
                DeclaringType.FullName, '+', name);

            _name = name;
            _assembly = declaringType.Assembly;
        }

        public static DefaultTypeName FromFullName(string full, AssemblyName assembly = null) {
            string ns, name;
            TypeName.Split(full, out ns, out name);
            return FromFullName(ns, name, assembly);
        }

        public static DefaultTypeName FromFullName(string ns,
                                                   string name,
                                                   AssemblyName assembly = null) {
            int mangle;
            name = TypeName.StripMangle(name, out mangle);
            var result = new DefaultTypeName(assembly, name, ns);
            result.FinalizeGenerics(mangle);
            return result;
        }

        public override bool Matches(TypeName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.IsTypeSpecification) {
                return false;
            }
            DefaultTypeName other = (DefaultTypeName) name;

            return Name == other.Name
                && NamespaceName.SafeMatch(NamespaceName, name.NamespaceName)
                && TypeName.SafeMatch(DeclaringType, name.DeclaringType);
        }

        public override string FullName {
            get { return _fullName + GenericMangle(); }
        }

        public override bool IsGenericType {
            get { return IsGenericTypeDefinition; }
        }

        public override bool IsGenericTypeDefinition {
            get { return GenericParameters.Count > 0; }
        }

        public override AssemblyName Assembly {
            get {
                return _assembly;
            }
        }

        internal string NameWithoutMangle {
            get { return _name; }
        }

        public override string Name {
            get { return _name + GenericMangle(); }
        }

        public override string Namespace {
            get { return _namespace ?? DeclaringType.Namespace; }
        }

        public override GenericParameterNameCollection GenericParameters {
            get {
                if (_genericParameters == null) {
                    FinalizeGenerics(0);
                }

                return _genericParameters;
            }
        }

        string GenericMangle() {
            int ct = GenericParameterCount;

            if (DeclaringType != null) {
                ct -= DeclaringType.GenericParameterCount;
            }

            return GenericMangle(ct);
        }

        static string GenericMangle(int ct) {
            if (ct > 0)
                return "`" + ct;
            else
                return null;
        }

        internal void FinalizeGenerics(int count) {
            GenericParameterName[] names = new GenericParameterName[count];
            int m = DeclaringType == null ? 0 : DeclaringType.GenericParameterCount;

            for (int i = 0; i < count; i++, m++) {
                names[i] = GenericParameterName.New(this, m, "`" + m);
            }

            SetGenericParameters(names);
        }

        internal void FinalizeGenerics(GenericParameterName[] names, bool includesEnclosing = false) {
            if (names == null) {
                _genericParameters = new GenericParameterNameCollection(Empty<GenericParameterName>.Array);
            } else {
                _genericParameters = new GenericParameterNameCollection(names);
            }

            if (!includesEnclosing) {
                SetGenericParameters(names);
            }
        }

        private void SetGenericParameters(IEnumerable<GenericParameterName> names) {
            if (DeclaringType != null) {

                // Redirect as if they were declared here
                names = this.DeclaringType.GenericParameters.Select(
                    (t, i) => new RedirectedGenericParameterName(this, i, t))
                    .Concat(names);
            }

            _genericParameters = new GenericParameterNameCollection(names.ToArray());
        }

        internal string FormatName(bool useMangle, bool useGenericParameterPositions, bool useInheritedParams) {
            if (useMangle)
                return _name + GenericMangle();
            else if (GenericParameterCount == 0)
                return _name;
            else if (GenericParameters[0].IsPositional)
                return _name + GenericMangle();

            var gp = (useInheritedParams) ? GenericParameters : GenericParameters.SkipWhile(t => t is RedirectedGenericParameterName);

            if (gp.Any()) {
                string parms =
                    useGenericParameterPositions ?
                    string.Join(", ", gp.Select(t => "`" + t.Position)) :
                    string.Join(", ", gp.Select(t => t.Name));
                return string.Concat(_name, "<", parms, ">");

            } else {
                return _name;
            }
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatType(format, this, provider);
        }

        protected override TypeName WithNamespaceOverride(string ns) {
            return UpdateCore(ns, Assembly);
        }

        internal override TypeName SetGenericParameters(IEnumerable<TypeName> those) {
            var result = DeclaringType == null
                         ? new DefaultTypeName(Assembly, NameWithoutMangle, Namespace)
                         : new DefaultTypeName(NameWithoutMangle, (DefaultTypeName) DeclaringType);
            result.FinalizeGenerics(those.Select(t => (GenericParameterName) t).ToArray());
            return result;
        }

        private TypeName UpdateCore(string ns, AssemblyName assembly) {
            ns = ns ?? string.Empty;
            DefaultTypeName result;
            if (DeclaringType == null) {
                result = new DefaultTypeName(assembly, _name, ns);
            } else {
                var decl = DeclaringType
                    .WithAssembly(assembly)
                    .WithNamespace(ns);
                result = new DefaultTypeName(_name, (DefaultTypeName) decl);
            }
            CopyGenericsTo(result);
            return result;
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            if (declaringType.IsTypeSpecification) {
                var git = declaringType as GenericInstanceTypeName;
                if (git != null) {
                    declaringType = git.ElementType;
                } else {
                    throw DotNetFailure.NotSupportedBySpecifications();
                }
            }

            var result = new DefaultTypeName(_name, (DefaultTypeName) declaringType);
            CopyGenericsTo(result);
            return result;
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return UpdateCore(Namespace, assembly);
        }

        private void CopyGenericsTo(DefaultTypeName result) {
            if (_genericParameters != null) {
                int baseCount = DeclaringType == null ? 0 : DeclaringType.GenericParameterCount;
                var generics = _genericParameters
                    .Where(t => !(t is RedirectedGenericParameterName))
                    .Select((t, i) => GenericParameterName.New(result, baseCount + i, t.Name)).ToArray();

                result.FinalizeGenerics(generics);
            }
        }

    }
}
