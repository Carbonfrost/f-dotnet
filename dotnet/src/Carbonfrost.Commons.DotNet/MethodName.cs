//
// Copyright 2013, 2015, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    public abstract partial class MethodName : MemberName, INameWithParameters  {

        public TypeName ReturnType {
            get {
                return ReturnParameter == null ? null : ReturnParameter.ParameterType;
            }
        }

        public abstract ParameterName ReturnParameter { get; }

        public MethodNameComponents Components {
            get {
                MethodNameComponents result = MethodNameComponents.Name;
                if (DeclaringType != null) {
                    result |= MethodNameComponents.DeclaringType;
                }
                if (ReturnType != null) {
                    result |= MethodNameComponents.ReturnType;
                }
                if (HasParametersSpecified) {
                    result |= MethodNameComponents.ParametersSpecified;
                    if (ParameterCount == 0 || Parameters.Any(y => y.ParameterType != null)) {
                        result |= MethodNameComponents.ParameterTypes;
                    }
                    if (ParameterCount == 0 || Parameters.Any(y => !string.IsNullOrEmpty(y.Name))) {
                        result |= MethodNameComponents.ParameterNames;
                    }
                }
                if (GenericParameterCount > 0) {
                    result |= MethodNameComponents.GenericParameters;
                    if (GenericParameters.Any(y => !y.IsPositional)) {
                        result |= MethodNameComponents.GenericParameterNames;
                    }
                }
                if (GenericArguments.Count > 0) {
                    result |= MethodNameComponents.GenericArguments;
                }
                return result;
            }
        }

        public bool IsConstructor {
            get { return Name == ".ctor" || Name == ".cctor"; }
        }

        public bool IsOperator {
            get { return OperatorType != OperatorType.Unknown; }
        }

        public OperatorType OperatorType {
            get {
                return MethodName.GetOperatorType(Name);
            }
        }

        public override string FullName {
            get {
                return MetadataNameFormat.FullNameFormat.FormatMethod(null, this, null);
            }
        }

        public PropertyName AssociatedProperty {
            get {
                return Associated(new [] { "get_", "set_" }, e => DeclaringType.GetProperty(e));
            }
        }

        public EventName AssociatedEvent {
            get {
                return Associated(new [] { "add_", "remove_", "raise_" }, e => DeclaringType.GetEvent(e));
            }
        }

        public MemberName AssociatedMember {
            get {
                return (MemberName) AssociatedProperty ?? AssociatedEvent;
            }
        }

        public virtual bool IsGenericMethod {
            get {
                return IsGenericMethodDefinition
                    || GenericArguments.Count > 0;
            }
        }

        public virtual bool IsGenericMethodDefinition {
            get {
                return GenericParameters.Count > 0;
            }
        }

        public virtual bool IsMethodSpecification { get { return false; } }
        public abstract int GenericParameterCount { get; }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Method;
            }
        }

        internal MethodName(TypeName declaringType)
            : base(declaringType) {}


        public static MethodName Create(string name) {
            return new DefaultMethodName(null, name);
        }

        internal static string StripMangle(string name, out int value) {
            value = 0;

            var m = Regex.Match(name, @"(.+)``(?<Value>\d+)$");
            if (m.Success) {
                value = System.Int32.Parse(m.Groups["Value"].Value);
                return m.Result("$1");
            } else {
                return name;
            }
        }

        public abstract MethodName WithName(string name);

        public MethodName AddParameter(string name) {
            return AddParameter(name, (TypeName) null);
        }

        public MethodName AddParameter(TypeName parameterType) {
            return AddParameter(null, parameterType);
        }

        public MethodName AddParameter(string name, TypeName parameterType) {
            return AddParameter(name, parameterType, null, null);
        }

        public MethodName AddParameter(string name,
                                       TypeName parameterType,
                                       IEnumerable<TypeName> requiredModifiers,
                                       IEnumerable<TypeName> optionalModifiers) {
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            return WithParameters(ImmutableUtility.Add(
                Parameters,
                (t, i) => new DefaultParameterName(this, i, t.Name, t.ParameterType, t.Modifiers),
                new DefaultParameterName(this, Parameters.Count, name, parameterType, modifiers)));
        }

        public MethodName SetParameter(int index, string name, TypeName parameterType) {
            return SetParameter(index, name, parameterType, null, null);
        }

        public MethodName SetParameter(int index,
            string name,
            TypeName parameterType,
            IEnumerable<TypeName> requiredModifiers,
            IEnumerable<TypeName> optionalModifiers) {
            var modifiers = new ModifierCollection(requiredModifiers, optionalModifiers);
            return WithParameters(ImmutableUtility.Set(
                Parameters,
                index,
                (t, i) => new DefaultParameterName(this, i, t.Name, t.ParameterType, t.Modifiers),
                new DefaultParameterName(this, index, name, parameterType, modifiers)));
        }

        public MethodName RemoveParameters() {
            return WithParameters(Array.Empty<ParameterName>());
        }

        public MethodName WithParametersUnspecified() {
            return WithParameters(null);
        }

        internal abstract MethodName WithParameters(ParameterName[] parameters);

        public abstract MethodName WithReturnParameter(TypeName returnType, IEnumerable<TypeName> requiredModifiers, IEnumerable<TypeName> optionalParameters);

        public MethodName WithReturnType(TypeName returnType) {
            return WithReturnParameter(returnType, null, null);
        }

        public MethodName WithReturnParameter(TypeName returnType) {
            return WithReturnParameter(returnType, null, null);
        }

        public abstract MethodName SetGenericParameter(int index, string name);

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return WithDeclaringType(DeclaringType.WithAssembly(assembly));
        }

        public int ParameterCount {
            get {
                if (!HasParametersSpecified) {
                    return -1;
                }
                return Parameters.Count;
            }
        }

        public abstract IReadOnlyList<TypeName> GenericArguments { get; }
        public abstract GenericParameterNameCollection GenericParameters { get; }
        public abstract ParameterNameCollection Parameters { get; }
        public abstract bool HasParametersSpecified { get; }

        public GenericInstanceMethodName MakeGenericMethod(params Type[] types) {
            return MakeGenericMethod(CheckArgs(types).Select(t => TypeName.FromType(t)));
        }

        public GenericInstanceMethodName MakeGenericMethod(IEnumerable<Type> types) {
            return MakeGenericMethod(CheckArgs(types).Select(t => TypeName.FromType(t)));
        }

        public GenericInstanceMethodName MakeGenericMethod(params TypeName[] types) {
            return new GenericInstanceMethodName(this, CheckArgs(types));
        }

        public GenericInstanceMethodName MakeGenericMethod(IEnumerable<TypeName> types) {
            return new GenericInstanceMethodName(this, CheckArgs(types));
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return name.SymbolType == SymbolType.Method && Matches((MethodName) name);
        }

        public bool Matches(MethodName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.Name == Name
                && TypeName.SafeMatch(DeclaringType, name.DeclaringType)
                && this.GenericParameterCount == name.GenericParameterCount
                && TypeName.MatchGenericArguments(this.GenericArguments, name.GenericArguments)
                && ParameterName.MatchHelper(this, name);
        }

        private T[] CheckArgs<T>(IEnumerable<T> types) {
            if (types == null) {
                throw new ArgumentNullException("types");
            }
            var typesArray = types.ToArray();
            if (typesArray.Length != this.GenericParameters.Count) {
                throw DotNetFailure.GenericParametersLengthMismatch("types");
            }

            return typesArray;
        }

        internal static OperatorType GetOperatorType(string name) {
            OperatorType result;
            if (Enum.TryParse(Regex.Replace(name, "^op_", string.Empty),
                              out result)) {
                return result;
            }

            return OperatorType.Unknown;
        }

        internal static string GetOperator(OperatorType type) {
            return "op_" + type;
        }

        internal static bool IsBinaryOperator(OperatorType type) {
            switch (type) {
                case OperatorType.Unknown:
                    return false;

                case OperatorType.Decrement:
                case OperatorType.Explicit:
                case OperatorType.False:
                case OperatorType.Implicit:
                case OperatorType.Increment:
                case OperatorType.Null:
                case OperatorType.OnesComplement:
                case OperatorType.True:
                case OperatorType.UnaryNegation:
                case OperatorType.UnaryPlus:
                    return false;

                case OperatorType.Addition:
                case OperatorType.BitwiseAnd:
                case OperatorType.BitwiseOr:
                case OperatorType.Division:
                case OperatorType.Equality:
                case OperatorType.ExclusiveOr:
                case OperatorType.GreaterThan:
                case OperatorType.GreaterThanOrEqual:
                case OperatorType.Inequality:
                case OperatorType.LeftShift:
                case OperatorType.LessThan:
                case OperatorType.LessThanOrEqual:
                case OperatorType.LogicalNot:
                case OperatorType.Modulus:
                case OperatorType.Multiply:
                case OperatorType.RightShift:
                case OperatorType.Subtraction:
                default:
                    return true;
            }
        }

        private T Associated<T>(IEnumerable<string> names, Func<string, T> factory) {
            return names.Where(n => Name.StartsWith(n, StringComparison.Ordinal))
                        .Select(n => factory(Name.Substring(n.Length)))
                        .FirstOrDefault();
        }
    }
}
