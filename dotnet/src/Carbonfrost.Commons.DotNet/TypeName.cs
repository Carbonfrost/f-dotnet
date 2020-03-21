//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public abstract partial class TypeName : MemberName {

        public abstract GenericParameterNameCollection GenericParameters { get; }
        public abstract string Namespace { get; }

        public NamespaceName NamespaceName {
            get {
                return new NamespaceName(Namespace);
            }
        }

        public string AssemblyQualifiedName {
            get {
                if (Assembly == null)
                    return FullName;
                else
                    return string.Concat(FullName, ", ", Assembly);
            }
        }

        public bool IsTypeDefinition { get { return !IsTypeSpecification; } }
        public virtual bool IsArray { get { return false; } }
        public virtual bool IsPointer { get { return false; } }
        public virtual bool IsByReference { get { return false; } }
        public virtual bool IsGenericParameter { get { return false; } }
        public virtual bool IsGenericType { get { return false; } }
        public virtual bool IsGenericTypeDefinition { get { return false; } }
        public virtual bool IsFunctionPointer { get { return false; } }
        public virtual bool IsTypeSpecification { get { return false; } }
        public bool IsNested { get { return this.DeclaringType != null; } }

        public virtual bool IsNullable { get { return false; } }

        public int GenericParameterCount {
            get {
                return GenericParameters.Count;
            }
        }

        internal TypeName(TypeName declaringType)
            : base(declaringType) {}

        internal TypeName() {}

        public override SymbolType SymbolType {
            get { return SymbolType.Type; } }

        internal virtual TypeName SetGenericParameters(IEnumerable<TypeName> those) {
            throw new NotSupportedException();
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (object.ReferenceEquals(this, name))
                return true;

            return name.SymbolType == SymbolType.Type && Matches((TypeName) name);
        }

        public virtual bool Matches(TypeName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return false;
        }

        public static bool TryParse(string text, out TypeName typeName) {
            return TryParse(text, TypeNameParseOptions.None, out typeName);
        }

        public static bool TryParse(string text, TypeNameParseOptions options, out TypeName result) {
            return _TryParse(text, options, out result) == null;
        }

        static Exception _TryParse(string text, TypeNameParseOptions options, out TypeName result) {
            result = null;

            if (text == null) {
                return new ArgumentNullException("text"); // $NON-NLS-1
            }
            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace("text");
            }

            bool preferGenericParams = options.HasFlag(TypeNameParseOptions.AssumeGenericParameters);
            try {
                result = new SignatureParser(text, preferGenericParams).ParseType();
                return null;
            } catch (Exception ex) {
                if (Failure.IsCriticalException(ex)) {
                    throw;
                }
                return Failure.NotParsable("text", typeof(TypeName));
            }
        }

        public static TypeName Parse(string text) {
            return Parse(text, TypeNameParseOptions.None);
        }

        public static TypeName Parse(string text, TypeNameParseOptions options) {
            TypeName result;
            Exception ex = _TryParse(text, options, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        internal static void Split(string full, out string ns, out string name) {
            int index = full.LastIndexOf('.');
            if (index < 0) {
                ns = string.Empty;
                name = full;
            } else {
                ns = full.Substring(0, index);
                name = full.Substring(index + 1);
            }
        }

        internal static string Combine(string ns, string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            if (string.IsNullOrEmpty(ns)) {
                return name;
            } else {
                return string.Concat(ns, ".", name);
            }
        }

        public static TypeName Create(string ns, string name) {
            return TypeName.Parse(Combine(ns, name));
        }

        internal static Exception ParseSimpleMember<T>(string text, out T result, Func<TypeName, string, TypeName, T> func) {
            result = default(T);
            if (text == null)
                return new ArgumentNullException("text");

            text = text.Trim();
            if (text.Length == 0)
                return Failure.AllWhitespace("text");

            string name = text;
            TypeName returnType = null;
            int colonIndex = text.LastIndexOf(':');

            if (colonIndex >= 0) {
                name = text.Substring(0, colonIndex);
                if (!TypeName.TryParse(text.Substring(colonIndex + 1), out returnType))
                    return Failure.NotParsable("text", typeof(T));
            }

            TypeName type;
            int index = name.LastIndexOf('.');
            if (index < name.Length - 1) {
                if (index > 0) {
                    if (TypeName.TryParse(name.Substring(0, index), out type)) {
                        string simpleName = name.Substring(index + 1);
                        result = func(type, simpleName, returnType);
                        return null;
                    }
                } else {

                    result = func(null, name, returnType);
                    return null;
                }
            }

            return Failure.NotParsable("text", typeof(T));
        }

        internal virtual TypeName CloneBind(TypeName declaring, MethodName method) {
            return this;
        }

        public ByReferenceTypeName MakeByReferenceType() {
            if (this.IsByReference)
                throw DotNetFailure.CannotMakeByReferenceType();

            return new ByReferenceTypeName(this);
        }

        public GenericInstanceTypeName MakeNullableType() {
            if (this.IsByReference)
                throw DotNetFailure.CannotMakeNullableType();

            return TypeName.Parse("System.Nullable`1").MakeGenericType(this);
        }

        public ArrayTypeName MakeArrayType(params ArrayDimension[] dimensions) {
            return new ArrayTypeName(this, dimensions);
        }

        public ArrayTypeName MakeArrayType(IEnumerable<ArrayDimension> dimensions) {
            if (dimensions == null)
                throw new ArgumentNullException("dimensions");

            return new ArrayTypeName(this, dimensions.ToArray());
        }

        public PointerTypeName MakePointerType() {
            return new PointerTypeName(this);
        }

        public ArrayTypeName MakeArrayType(int rank) {
            if (rank <= 0)
                throw Failure.NegativeOrZero("rank", rank);

            return MakeArrayType(Enumerable.Repeat(ArrayDimension.Unsized, rank));
        }

        public GenericInstanceTypeName MakeGenericType(IEnumerable<TypeName> arguments) {
            if (arguments == null)
                throw new ArgumentNullException("arguments");

            return MakeGenericType(arguments.ToArray());
        }

        public GenericInstanceTypeName MakeGenericType(params TypeName[] arguments) {
            if (arguments == null)
                throw new ArgumentNullException("arguments");
            if (arguments.Length == 0)
                throw Failure.EmptyCollection("arguments");
            if (arguments.Any(t => t == null))
                throw Failure.CollectionContainsNullElement("arguments");
            if (arguments.Length != this.GenericParameterCount)
                throw DotNetFailure.GenericParametersLengthMismatch("arguments");

            return new GenericInstanceTypeName(this, arguments);
        }

        public TypeName GetNestedType(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");
            if (this.IsTypeSpecification && !this.IsGenericType)
                throw DotNetFailure.NotSupportedBySpecifications();

            TypeName tn;
            if (!TypeName.TryParse(name, TypeNameParseOptions.AssumeGenericParameters, out tn))
                throw Failure.NotParsable("name", typeof(TypeName));

            return GetNestedType(tn);
        }

        public TypeName GetNestedType(TypeName nested) {
            if (nested == null) {
                throw new ArgumentNullException("nested");
            }
            return nested.WithDeclaringType(this);
        }

        public PropertyName GetProperty(string name,
                                        TypeName propertyType,
                                        IEnumerable<TypeName> parameters) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            parameters = parameters ?? Array.Empty<TypeName>();
            if (parameters.Any(t => t == null))
                throw Failure.CollectionContainsNullElement("parameters");

            // TODO Can't bind unboound method parameters here
            var allParams = ParameterData.AllFromTypes(BindParameterTypes(null, parameters.ToArray()));
            return new PropertyName(this, name, SafeCloneBind(propertyType), allParams);
        }

        public PropertyName GetProperty(string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            return new PropertyName(this, name, null, Array.Empty<ParameterData>());
        }

        public PropertyName GetProperty(string name, params TypeName[] parameters) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            parameters = parameters ?? Array.Empty<TypeName>();
            if (parameters.Any(t => t == null)) {
                throw Failure.CollectionContainsNullElement("parameters");
            }

            parameters = BindParameterTypes(null, parameters);
            return new PropertyName(this, name, null, ParameterData.AllFromTypes(parameters));
        }

        public MethodName GetMethod(string name, params TypeName[] parameters) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            parameters = parameters ?? Array.Empty<TypeName>();
            if (parameters.Any(t => t == null)) {
                throw Failure.CollectionContainsNullElement("parameters");
            }

            int mangle;
            var method = TypeName.StripMangle(name, out mangle);
            var result = new DefaultMethodName(this, method, null);

            result.FinalizeGenerics(mangle);
            parameters = BindParameterTypes(result, parameters);
            result.FinalizeParameters(ParameterData.AllFromTypes(parameters));
            return result;
        }

        private TypeName[] BindParameterTypes(MethodName method,
                                              TypeName[] parameters) {

            for (int i = 0; i < parameters.Length; i++) {
                parameters[i] = parameters[i].CloneBind(this, method);
            }

            return parameters;
        }

        public MethodName GetMethod(string name, IEnumerable<TypeName> parameters) {
            return GetMethod(name, null, parameters);
        }

        public MethodName GetMethod(string name, IEnumerable<string> typeParameters, IEnumerable<TypeName> parameters) {
            return GetMethod(name, TypeName.Void, typeParameters, parameters);
        }

        public MethodName GetMethod(string name, TypeName returnType, IEnumerable<string> typeParameters, IEnumerable<TypeName> parameters) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }
            if (returnType == null) {
                throw new ArgumentNullException("returnType");
            }

            typeParameters = typeParameters ?? Array.Empty<string>();
            parameters = parameters ?? Array.Empty<TypeName>();

            if (typeParameters.Any(t => t == null)) {
                throw Failure.CollectionContainsNullElement("typeParameters");
            }
            if (parameters.Any(t => t == null)) {
                throw Failure.CollectionContainsNullElement("parameters");
            }

            var result = new DefaultMethodName(this, name, returnType);
            result.FinalizeGenerics(typeParameters.Select((t, i) => GenericParameterName.New(result, i, t)).ToArray());
            result.FinalizeParameters(ParameterData.AllFromTypes(BindParameterTypes(result, parameters.ToArray())));
            return result;
        }

        // TODO Add GetMethod where there are generic parameters

        public MethodName GetOperator(OperatorType operatorType) {
            if (operatorType == OperatorType.Unknown) {
                throw DotNetFailure.UnknownConversionOperatorCannotBeUsed("operatorType", operatorType);
            }
            if (operatorType == OperatorType.Explicit
                || operatorType == OperatorType.Implicit) {
                throw DotNetFailure.UnknownConversionOperatorCannotBeUsed("operatorType", operatorType);
            }

            if (MethodName.IsBinaryOperator(operatorType)) {
                return GetBinaryOperator(operatorType, this, this, this);
            }
            return GetUnaryOperator(operatorType, this, this);
        }

        public MethodName GetConversionOperator(OperatorType operatorType,
                                                TypeName returnType,
                                                TypeName conversionType) {
            switch (operatorType) {
                case OperatorType.Explicit:
                case OperatorType.Implicit:
                    break;

                default:
                    throw DotNetFailure.ConversionOperatorExpected("operatorType", operatorType);
            }

            TypeName[] parms = null;
            if (conversionType != null) {
                parms = new [] { conversionType };
            }

            return GetMethod("op_" + operatorType,
                             returnType,
                             null,
                             parms);
        }

        public MethodName GetBinaryOperator(OperatorType operatorType,
                                            TypeName leftOperandType,
                                            TypeName rightOperandType,
                                            TypeName resultType) {
            if (!MethodName.IsBinaryOperator(operatorType)) {
                throw DotNetFailure.BinaryOperatorRequired("operatorType", operatorType);
            }

            string name = MethodName.GetOperator(operatorType);
            var result = new DefaultMethodName(this, name, resultType ?? this);
            result.FinalizeParameters(ParameterData.AllFromTypes(new[] { leftOperandType ?? this, rightOperandType ?? this }));
            return result;
        }

        public MethodName GetUnaryOperator(OperatorType operatorType,
                                           TypeName resultType,
                                           TypeName operandType
                                          ) {
            string name;

            switch (operatorType) {
                case OperatorType.Decrement:
                case OperatorType.Increment:
                case OperatorType.False:
                case OperatorType.True:
                case OperatorType.OnesComplement:
                    name = "op_" + operatorType;
                    break;

                case OperatorType.UnaryNegation:
                    name = "op_Negate";
                    break;

                default:
                    throw DotNetFailure.UnaryOperatorExpected("operandType", operandType);
            }

            if (resultType == null) {
                throw new ArgumentNullException("resultType");
            }

            if (operandType == null) {
                throw new ArgumentNullException("operandType");
            }

            return GetMethod(name, resultType, null, new[] { operandType });
        }

        public EventName GetEvent(string name) {
            return GetEvent(name, null);
        }

        public EventName GetEvent(string name, TypeName eventType) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            return new EventName(this, name, SafeCloneBind(eventType));
        }

        public FieldName GetField(string name) {
            return GetField(name, null);
        }

        public FieldName GetField(string name, TypeName fieldType) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0) {
                throw Failure.EmptyString("name");
            }

            return new FieldName(this, name, SafeCloneBind(fieldType));
        }

        public static TypeName FromType(Type type) {
            if (type == null) {
                throw new ArgumentNullException("type");
            }

            if (type.IsByRef) {
                return FromType(type.GetElementType()).MakeByReferenceType();
            }

            else if (type.IsArray) {
                return FromType(type.GetElementType()).MakeArrayType(Enumerable.Repeat(ArrayDimension.Unsized, type.GetArrayRank()));
            }

            else if (type.IsPointer) {
                return FromType(type.GetElementType()).MakePointerType();
            }

            else if (type.IsGenericParameter) {
                if (type.GetTypeInfo().DeclaringMethod == null)
                    return GenericParameterName.New(FromType(type.DeclaringType), type.GenericParameterPosition, type.Name);
                else {
                    // TODO This is unbound, but getting the method is recursive
                    return MethodName.GenericParameter(type.GenericParameterPosition);
                }

            } else if (type.GetTypeInfo().IsGenericType)
                return ConvertGenericType(type);

            else {
                TypeName declaring = null;
                if (type.DeclaringType == null) {
                    return new DefaultTypeName(
                        AssemblyName.FromAssemblyName(type.GetTypeInfo().Assembly.GetName()),
                        type.Name,
                        type.Namespace ?? string.Empty);

                } else {
                    declaring = FromType(type.DeclaringType);
                    return new DefaultTypeName(type.Name, (DefaultTypeName) declaring);
                }
            }
        }

        internal static string StripMangle(string name, out int value) {
            value = 0;

            var m = Regex.Match(name, @"(.+)`(?<Value>\d+)$");
            if (m.Success) {
                value = System.Int32.Parse(m.Groups["Value"].Value);
                return m.Result("$1");
            } else {
                return name;
            }
        }

        internal static string StripMangle(string name) {
            int dummy;
            return StripMangle(name, out dummy);
        }

        static TypeName ConvertGenericType(Type type) {
            if (type.GetTypeInfo().IsGenericTypeDefinition) {

                DefaultTypeName result;
                int skipCount;
                string plainName = StripMangle(type.Name);

                if (type.DeclaringType == null) {
                    result = new DefaultTypeName(
                        AssemblyName.FromAssemblyName(type.GetTypeInfo().Assembly.GetName()),
                        plainName,
                        type.Namespace);
                    skipCount = 0;

                } else {
                    var declaring = FromType(type.DeclaringType);
                    result = new DefaultTypeName(plainName, (DefaultTypeName) declaring);
                    skipCount = declaring.GenericParameterCount;
                }

                var gen = type.GetGenericArguments().Skip(skipCount).Select(
                    (t, i) => GenericParameterName.New(result, i, t.Name)).ToArray();
                result.FinalizeGenerics(gen);

                return result;
            }

            var args = type.GetGenericArguments().Select(FromType);
            return FromType(type.GetGenericTypeDefinition()).MakeGenericType(args);
        }

        public TypeName WithGenericParameters(int count) {
            if (count < 0) {
                throw Failure.Negative("count", count);
            }
            GenericParameterName[] names = new GenericParameterName[count];
            int m = 1;
            for (int i = 0; i < count; i++, m++) {
                names[i] = GenericParameterName.New(this, m, "`" + m);
            }
            return SetGenericParameters(names);
        }

        public TypeName WithNamespace(NamespaceName ns) {
            if (ns == null) {
                throw new ArgumentNullException("ns");
            }
            return WithNamespace(ns.FullName);
        }

        public TypeName WithNamespace(string ns) {
            return WithNamespaceOverride(ns);
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            throw new NotImplementedException();
        }

        protected abstract TypeName WithNamespaceOverride(string ns);

        internal override MetadataName AddRight(MetadataName name) {
            var member = name as MemberName;
            if (member != null) {
                return member.WithDeclaringType(this);
            }
            return base.AddRight(name);
        }

        internal static bool SafeMatch(TypeName a, TypeName b) {
            if (a == null || b == null) {
                return true;
            }
            return a.Matches(b);
        }

        private TypeName SafeCloneBind(TypeName type) {
            if (type == null)
                return null;
            else
                return type.CloneBind(this, null);
        }

        internal static bool MatchGenericArguments(IReadOnlyList<TypeName> left,
                                                   IReadOnlyList<TypeName> other) {

            return left.Count == other.Count
                && left.Zip(other, (t, u) => t.Matches(u)).AllTrue();
        }
    }
}
