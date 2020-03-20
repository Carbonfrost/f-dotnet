//
// Copyright 2016, 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Text;
using System.Text.RegularExpressions;

namespace Carbonfrost.Commons.DotNet {

    partial class SignatureParser {

        private static readonly Regex QUALIFIED_TYPE = new Regex
            (@"^(?<Namespace> (\w+\.)+)?
                (?<Type> \w+ (`\d+)? )
                (?<Nested> ([\+/] \w+ (`\d+)?)+)?$", RegexOptions.IgnorePatternWhitespace);

        // Mangle implies a type name but a dot is used after it
        private static readonly Regex IMPLIED_NESTED_QUALIFIED_TYPE = new Regex
            (@"^(?<Namespace> (\w+\.)+)?
                (?<Type> \w+ (`\d+) )
                (?<Nested> (\. \w+ (`\d+)?)+)?$", RegexOptions.IgnorePatternWhitespace);
        private readonly Scanner _s;
        private readonly bool _preferGenericParams;
        private readonly bool _preferGenericParamsInMethods;

        internal TokenType Type {
            get {
                return _s.Type;
            }
        }

        public SignatureParser(string text,
                               bool preferGenericParams = false,
                               bool preferGenericParamsInMethods = false) {
            _s = new Scanner(text);
            _preferGenericParams = preferGenericParams;
            _preferGenericParamsInMethods = preferGenericParamsInMethods;
        }

        public TypeName ParseType() {
            _s.RequireMoveNext();

            AssemblyName asm = ParseLeadAssemblyNameOpt();

            if (_s.Type == TokenType.GenericPosition) {
                var gp = (GenericPositionToken) _s.Current;
                MoveNext();
                return gp.IsMethod ? MethodName.GenericParameter(gp.Position)
                                   : TypeName.GenericParameter(gp.Position);
            }

            if (_s.Type == TokenType.Identifier) {
                var type = ParseTypeWithSpecifiers();

                // Could have the case where T<U,V>.N, a nested type in generics
                while (type.IsGenericType && (_s.Type == TokenType.Plus || _s.Type == TokenType.Dot || _s.Type == TokenType.Slash)) {
                    _s.MoveNext();
                    var nested = ParseTypeWithSpecifiers();
                    type = nested.WithDeclaringType(type);
                }

                var trailAsm = ParseAssemblyNameOpt();
                if (asm != null && trailAsm != null) {
                    throw FormatError();
                }
                ParseEOF();
                return type.WithAssembly(asm ?? trailAsm);
            }

            throw FormatError();
        }

        public MethodName ParseMethod() {
            _s.RequireMoveNext();

            string name = null;
            int sz = 0; // of generics
            TypeName declaringType = ParseDeclaringTypeOpt(out name);
            ParameterData[] pms = null;
            TypeName returnType = null;
            AssemblyName asm = null;

            if (_s.Type == TokenType.DoubleColon) {
                throw new NotImplementedException();
            }

            name = MethodName.StripMangle(name, out sz);

            bool hasGenericParams = _s.Type == TokenType.LessThan;
            var generics = ParseTypeParametersOpt();
            if (hasGenericParams) {
                MoveNext();
            }

            if (_s.Type == TokenType.LeftParen) {
                pms = ParseParameters(TokenType.RightParen).ToArray();
                _s.MoveToContent();
            }

            returnType = ParseReturnTypeOpt();
            asm = ParseAssemblyNameOpt();
            ParseEOF();

            if (declaringType != null) {
                declaringType = declaringType.WithAssembly(asm);
            }

            if (!hasGenericParams && sz > 0) {
                // Method``2(A,B) -- mangled but no type parameter syntax
                var result = new DefaultMethodName(declaringType, name, returnType);
                result.FinalizeGenerics(sz);
                result.FinalizeParameters(pms);

                return result;

            } else if (generics.MustBeParameters || (generics.CouldBeParameters && _preferGenericParamsInMethods)) {
                var result = new DefaultMethodName(declaringType, name, returnType);
                result.FinalizeGenerics(generics.ConvertToGenerics(true).ToArray());
                result.FinalizeParameters(pms);
                // TODO Could have discrepancy between generics declared with position and
                // named in type parameter list (sz != generics.Raw.Count)
                return result;

            } else {
                var elementName = new DefaultMethodName(declaringType, name, returnType);

                elementName.FinalizeGenerics(generics.Raw.Count);
                elementName.FinalizeParameters(pms);
                return new GenericInstanceMethodName(elementName, generics.Raw.ToArray());
            }
        }

        public PropertyName ParseProperty() {
            _s.RequireMoveNext();

            string name = null;
            TypeName declaringType = ParseDeclaringTypeOpt(out name);
            ParameterData[] pms = null;
            TypeName returnType = null;
            AssemblyName asm = null;

            if (_s.Type == TokenType.DoubleColon) {
                throw new NotImplementedException();
            }

            if (_s.Type == TokenType.LeftBracket) {
                pms = ParseParameters(TokenType.RightBracket).ToArray();
                _s.MoveToContent();

                // Even if no parameters are specified, it implies one
                // index parameter ... like Chars[]
                if (pms.Length == 0) {
                    pms = new [] { ParameterData.Empty };
                }
            }

            returnType = ParseReturnTypeOpt();
            asm = ParseAssemblyNameOpt();
            ParseEOF();

            if (declaringType != null) {
                declaringType = declaringType.WithAssembly(asm);
            }

            return new PropertyName(declaringType, name, returnType, pms);
        }

        internal TypeName ParseDeclaringTypeOpt(out string memberName) {
            var name = ParseQualifiedNameOpt();
            if (name == null || name == ".ctor" || name == ".cctor") {
                memberName = name ?? string.Empty;
                return null;
            }

            if (name.EndsWith("..ctor", StringComparison.Ordinal)) {
                memberName = ".ctor";
                return FromFullName(name.Substring(0, name.Length - "..ctor".Length));
            }
            if (name.EndsWith("..cctor", StringComparison.Ordinal)) {
                memberName = ".cctor";
                return FromFullName(name.Substring(0, name.Length - "..cctor".Length));
            }

            string typeName;
            TypeName.Split(name, out typeName, out memberName);
            return FromFullName(typeName);
        }

        internal IEnumerable<ParameterData> ParseParameters(TokenType ends) {
            if (_s.Type != TokenType.LeftBracket && _s.Type != TokenType.RightBracket
                && _s.Type != TokenType.LeftParen && _s.Type != TokenType.RightParen) {
                throw new NotImplementedException();
            }
            bool first = true;
            while (MoveToContent()) {
                if (first && _s.Type == ends) {
                    yield break;
                }
                var pm = ParseParameter();
                first = false;
                yield return pm;
                if (_s.Type == ends) {
                    yield break;
                }
            }
            throw FormatError();
        }

        internal TypeParameters ParseTypeParametersOpt() {
            if (_s.Type != TokenType.LessThan) {
                return new TypeParameters(Empty<TypeName>.Array);
            }
            var result = new List<TypeName>();
            bool first = true;
            while (MoveToContent()) {
                if (first && _s.Type == TokenType.GreaterThan) {
                    result.Add(ParseTypeParameter());
                    return new TypeParameters(result);
                }
                var pm = ParseTypeParameter();
                first = false;
                result.Add(pm);

                if (_s.Type == TokenType.GreaterThan) {
                    return new TypeParameters(result);
                }
            }
            throw FormatError();
        }

        private TypeName ParseTypeParameter() {
            var nameOrType = ParseParameterTypeOpt();
            SkipWS();
            switch (_s.Type) {
                case TokenType.GreaterThan:
                case TokenType.Comma:
                    return nameOrType;
            }

            throw FormatError();
        }

        private string MoveTo(TokenType t) {
            var buffer = new StringBuilder();
            while (MoveNext() && _s.Type != t) {
                buffer.Append(_s.Value);
            }
            return buffer.ToString();
        }

        // internal for tests
        internal bool MoveNext() {
            return _s.MoveNext();
        }

        private bool MoveToContent() {
            return _s.MoveToContent();
        }

        private void MoveToEnd() {
            MoveTo(TokenType.EndOfInput);
        }

        internal TypeName ParseParameterTypeOpt() {
            AssemblyName asm = ParseLeadAssemblyNameOpt();
            if (_s.Type == TokenType.GenericPosition) {
                var gp = (GenericPositionToken) _s.Current;
                MoveNext();

                var result = gp.IsMethod ? MethodName.GenericParameter(gp.Position)
                                         : TypeName.GenericParameter(gp.Position);
                return ParseTypeSpecifiersOpt(result);
            }

            if (_s.Type == TokenType.Identifier) {
                return ParseTypeWithSpecifiers().WithAssembly(asm);
            }

            if (asm != null) {
                throw FormatError();
            }
            return null;
        }

        internal TypeName ParseTypeWithSpecifiers() {
            var ident = ParseQualifiedName();
            return ParseTypeSpecifiersOpt(ParseQualifiedType(ident));
        }

        internal static TypeName ParseQualifiedType(string ident) {
            var match = QUALIFIED_TYPE.Match(ident);
            if (!match.Success) {
                match = IMPLIED_NESTED_QUALIFIED_TYPE.Match(ident);
            }
            if (!match.Success) {
                throw new NotImplementedException();
            }

            int sz; // of generics
            string name = TypeName.StripMangle(match.Groups["Type"].Value, out sz);
            TypeName result = new DefaultTypeName(null,
                                                  name,
                                                  match.Groups["Namespace"].Value.Trim('.'),
                                                  sz);

            var nestedTypes = match.Groups["Nested"].Value.Trim('.');
            if (nestedTypes.Length > 0) {
                foreach (var nested in nestedTypes.Split(new [] { '+', '/' }, StringSplitOptions.RemoveEmptyEntries)) {
                    result = result.GetNestedType(nested);
                }
            }

            return result;
        }

        private AssemblyName ParseLeadAssemblyNameOpt() {
            if (_s.Type == TokenType.LeftBracket) {
                // Assembly qualifier [mscorlib] String
                string asmText = MoveTo(TokenType.RightBracket);
                var asm = AssemblyName.Parse(asmText);
                MoveToContent(); // ']' and whitespace
                return asm;
            }
            return null;
        }

        // Should be positioned on the parameter
        // internal for tests
        internal ParameterData ParseParameter() {
            // name : Type
            // Type <space> name
            // <empty> (with ']', ',', ')' following)
            var nameOrType = ParseParameterTypeOpt();

            // TODO Name could parse as a specified type and we could optimize here

            // Separator - either space or ':'
            bool sawSpace = false;
            if (_s.Type == TokenType.Space) {
                MoveNext();
                sawSpace = true;
            }

            if (_s.Type == TokenType.Colon) {
                MoveToContent();
                return new ParameterData(nameOrType == null ? string.Empty : nameOrType.ToString(),
                                         ParseParameterTypeOpt());
            }
            if (sawSpace) {
                // Could have ident <space> specifiers, so try that first
                var specifiedType = ParseTypeSpecifiersOpt(nameOrType);
                if (specifiedType != nameOrType) {
                    MoveToContent();
                    return new ParameterData(ParseQualifiedNameOpt(), specifiedType);
                }

                // Appears to be type <space> name, type is a simple type
                string name = ParseQualifiedNameOpt(); // TODO Too loose (here and above)
                return new ParameterData(name, nameOrType);
            }

            switch (_s.Type) {
                case TokenType.RightBracket:
                case TokenType.RightParen:
                    return new ParameterData(string.Empty, nameOrType);
                case TokenType.Comma:
                    return new ParameterData(string.Empty, nameOrType);
            }

            throw FormatError();
        }

        private TypeName ParseReturnTypeOpt() {
            if (_s.Type != TokenType.Colon) {
                return null;
            }
            MoveToContent();
            return ParseParameterTypeOpt();
        }

        private string ParseQualifiedNameOpt() {
            if (_s.Type == TokenType.Identifier) {
                return ParseQualifiedName();
            }
            return null;
        }

        internal string ParseQualifiedName() {
            // ident ( '.'/'/'/'+' <ident> )
            if (_s.Type != TokenType.Identifier) {
                throw FormatError();
            }

            // accumulate qualified name
            return _s.Value + ParseRestQualifiedNameNext();
        }

        private string ParseRestQualifiedNameNext() {
            // ( '.'/'/'/'+' <ident> ) +
            var sb = new StringBuilder();
            while (_s.MoveNext()
                && (_s.Type == TokenType.Dot || _s.Type == TokenType.Slash || _s.Type == TokenType.Plus
                    || _s.Type == TokenType.GenericPosition)) {

                sb.Append(_s.Value);

                if (_s.Type == TokenType.GenericPosition) {
                    continue;
                }

                if (!_s.MoveNext() || _s.Type != TokenType.Identifier) {
                    throw FormatError();
                }

                sb.Append(_s.Value);
            }
            return sb.ToString();
        }

        private TypeName ParseTypeSpecifiersOpt(TypeName type) {
            if (_s.Type == TokenType.LessThan) {
                var typeArgsOrParams = ParseTypeParametersOpt();

                // TODO Could have a mangled name and differing number of arguments: T`1<A,B>
                //      Right now, the type parameters win as-is
                if (type.GenericParameterCount > 0 || typeArgsOrParams.MustBeParameters || (typeArgsOrParams.CouldBeParameters && _preferGenericParams)) {
                    type = type.SetGenericParameters(typeArgsOrParams.ConvertToGenerics(false));
                } else {
                    // Closed generic type
                    type = type.WithGenericParameters(typeArgsOrParams.Raw.Count())
                               .MakeGenericType(typeArgsOrParams.Raw);
                }

                _s.MoveNext();
            }

            do {
                if (_s.Type == TokenType.LeftBracket) {
                    type = type.MakeArrayType(ParseArrayDimensions());
                }
                else if (_s.Type == TokenType.Asterisk) {
                    type = type.MakePointerType();

                } else if (_s.Type == TokenType.And) {
                    type = type.MakeByReferenceType();

                } else {
                    break;
                }

            } while (MoveNext());

            return type;
        }

        private IEnumerable<ArrayDimension> ParseArrayDimensions() {
            if (_s.Type != TokenType.LeftBracket) {
                throw new NotImplementedException();
            }
            while (MoveToContent()) {
                var pm = ArrayDimension.Unsized;
                yield return pm;
                if (_s.Type == TokenType.RightBracket) {
                    yield break;
                }
            }
        }

        private AssemblyName ParseAssemblyNameOpt()  {
            if (_s.Type == TokenType.Comma) {
                AssemblyName asm;
                if (AssemblyName.TryParse(_s.Rest, out asm)) {
                    MoveToEnd();
                    return asm;
                }
            }
            return null;
        }

        private void ParseEOF() {
            if (_s.Type != TokenType.EndOfInput) {
                throw FormatError();
            }
        }

        private bool SkipWS() {
            bool any = false;
            while (_s.Type == TokenType.Space) {
                any = true;
                MoveNext();
            }
            return any;
        }

        private static TypeName FromFullName(string name) {
            if (name.Length == 0) {
                return null;
            }
            return TypeName.Parse(name);
        }

        private FormatException FormatError() {
            return new FormatException();
        }
    }

}
