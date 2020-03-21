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
using System.Text;
using System.Text.RegularExpressions;

using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.DotNet {

    [Providers]
    public partial class MetadataNameFormat {

        public static readonly MetadataNameFormat Default = CreateDefault();

        internal static readonly MetadataNameFormat CompactFormat = new CompactFormatImpl();
        internal static readonly MetadataNameFormat FullNameFormat = new FullNameFormatImpl(false);
        internal static readonly MetadataNameFormat NameOverloadsFormat = new FullNameFormatImpl(true);
        internal static readonly MetadataNameFormat RoundtrippableFormat = new RoundtrippableFormatImpl();

        private bool _includeTypeConstraints;
        private bool _useGenericParameterPositions;
        private bool _includeTypeParameters;
        private bool _includeVariance;
        private SymbolTypes _includeReturnTypes = new SymbolTypes();
        private SymbolTypes _includeModifiers = new SymbolTypes();
        private SymbolTypes _includeAttributes = new SymbolTypes();
        private readonly SymbolTypeMap<string> _defaultFormat = new SymbolTypeMap<string>("G");

        public SymbolTypeMap<string> DefaultFormatString {
            get {
                return _defaultFormat;
            }
        }

        public SymbolTypes IncludeModifiers {
            get {
                return _includeModifiers;
            }
            set {
                ThrowIfReadOnly();
                _includeModifiers = value;
            }
        }

        public SymbolTypes IncludeAttributes {
            get {
                return _includeAttributes;
            }
            set {
                ThrowIfReadOnly();
                _includeAttributes = value;
            }
        }

        public bool IncludeTypeParameters {
            get {
                return _includeTypeParameters;
            }
            set {
                ThrowIfReadOnly();
                _includeTypeParameters = value;
            }
        }

        public SymbolTypes IncludeReturnTypes {
            get {
                return _includeReturnTypes;
            }
            set {
                ThrowIfReadOnly();
                _includeReturnTypes = value;
            }
        }

        public bool IncludeTypeConstraints {
            get {
                return _includeTypeConstraints;
            }
            set {
                ThrowIfReadOnly();
                _includeTypeConstraints = value;
            }
        }

        public bool UseGenericParameterPositions {
            get {
                return _useGenericParameterPositions;
            }
            set {
                ThrowIfReadOnly();
                _useGenericParameterPositions = value;
            }
        }

        public bool IncludeVariance {
            get {
                return _includeVariance;
            }
            set {
                ThrowIfReadOnly();
                _includeVariance = value;
            }
        }

        internal MetadataNameFormat(bool dummy) {}

        public MetadataNameFormat() : this(null) {
        }

        public MetadataNameFormat(MetadataNameFormat options) {
            if (options == null) {
                options = MetadataNameFormat.Default;
            }

            DefaultFormatString.CopyBuffer(options.DefaultFormatString);
            _includeAttributes = options.IncludeAttributes.Clone();
            _includeModifiers = options.IncludeModifiers.Clone();
            _includeReturnTypes = options.IncludeReturnTypes.Clone();
            _includeTypeConstraints = options.IncludeTypeConstraints;
            _includeTypeParameters = options.IncludeTypeParameters;
            _includeVariance = options.IncludeVariance;
        }

        public string Format(string format, MetadataName name, IFormatProvider formatProvider = null) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            var options = this.GetFormatHelper(formatProvider);

            if (string.IsNullOrEmpty(format)) {
                format = options.DefaultFormatString[name.SymbolType] ?? "G";
            }

            return name.Accept(this, format, options);
        }

        public static MetadataNameFormat Parse(string text) {
            switch (GetBasicFormat(text)) {
                case BasicFormat.Compact:
                    return CompactFormat.Clone();

                case BasicFormat.Name:
                    return new MetadataNameFormat();

                case BasicFormat.NameOverloads:
                    return NameOverloadsFormat.Clone();

                case BasicFormat.Full:
                    return FullNameFormat.Clone();

                case BasicFormat.Roundtrip:
                default:
                    return RoundtrippableFormat.Clone();
            }
        }

        public string Format(MetadataName name, IFormatProvider formatProvider = null) {
            return Format(null, name, formatProvider);
        }

        protected internal virtual string FormatArrayType(string format, ArrayTypeName name, IFormatProvider formatProvider) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            // TODO Support formatting arrays with sizes
            return Format(format, name.ElementType, formatProvider) + "[]";
        }

        protected internal virtual string FormatByReferenceType(string format, ByReferenceTypeName name, IFormatProvider formatProvider) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            return Format(format, name.ElementType, formatProvider) + "&";
        }

        protected internal virtual string FormatType(string format, TypeName name, IFormatProvider formatProvider) {
            if (name == null) {
                throw new ArgumentNullException(nameof(name));
            }

            var dtn = (DefaultTypeName) name;
            var basic = GetBasicFormat(format);

            if (basic == BasicFormat.Full || basic == BasicFormat.Roundtrip) {
                StringBuilder sb = new StringBuilder();
                BuildFullName(sb, dtn, formatProvider);
                return sb.ToString();

            } else {
                var options = GetFormatHelper(formatProvider);
                return dtn.FormatName(this.UseGenericParameterPositions || !options.IncludeTypeParameters, this.UseGenericParameterPositions, false);
            }
        }

        protected internal virtual string FormatEvent(string format, EventName name, IFormatProvider formatProvider) {
            return DefaultFormat(format, name);
        }

        protected internal virtual string FormatField(string format, FieldName name, IFormatProvider formatProvider) {
            return DefaultFormat(format, name);
        }

        protected internal virtual string FormatFunctionPointerType(string format, FunctionPointerTypeName name, IFormatProvider formatProvider) {
            if (name == null)
                throw new ArgumentNullException("name");

            StringBuilder sb = new StringBuilder();
            sb.Append(name.ReturnType.FullName);
            sb.Append(" *");
            this.AppendMethodParameters(sb, name.Parameters);
            return sb.ToString();
        }

        protected internal virtual string FormatNamespace(string format, NamespaceName name, IFormatProvider formatProvider) {
            return DefaultFormat(format, name);
        }

        protected internal virtual string FormatRequiredModifier(string format, TypeName type, IFormatProvider formatProvider) {
            return string.Format("modreq({0})", FormatType(format, type, formatProvider));
        }

        protected internal virtual string FormatOptionalModifier(string format, TypeName type, IFormatProvider formatProvider) {
            return string.Format("modopt({0})", FormatType(format, type, formatProvider));
        }

        protected internal virtual string FormatProperty(string format, PropertyName name, IFormatProvider formatProvider) {
            if (name == null)
                throw new ArgumentNullException("name");

            StringBuilder sb = new StringBuilder();

            switch (GetBasicFormat(format)) {
                case BasicFormat.Compact:
                case BasicFormat.Name:
                    sb.Append(name.Name);
                    break;

                case BasicFormat.NameOverloads:
                    sb.Append(name.Name);
                    AppendPropertyParameters(sb, name, this.GetDefaultFormat(formatProvider, SymbolType.Parameter), formatProvider);
                    break;

                case BasicFormat.Full:
                case BasicFormat.Roundtrip:
                default:
                    // TODO Support Roundtrip format for property names
                    if (name.DeclaringType != null) {
                        sb.Append(name.DeclaringType.FullName);
                        AppendNameSeparator(sb, name.Name);
                    }

                    sb.Append(name.Name);
                    AppendPropertyParameters(sb, name, this.GetDefaultFormat(formatProvider, SymbolType.Parameter), formatProvider);
                    break;
            }

            var options = GetFormatHelper(formatProvider);
            if (options.IncludeReturnTypes.Property) {
                string returnTypeFormat = string.Concat((options.DefaultFormatString.Parameter ?? string.Empty).Take(1));
                sb.Append(":");
                sb.Append(Format(returnTypeFormat, name.PropertyType, formatProvider));
            }

            return sb.ToString();
        }

        protected internal virtual string FormatGenericInstanceType(string format, GenericInstanceTypeName name, IFormatProvider formatProvider) {
            if (name == null)
                throw new ArgumentNullException("name");

            var options = GetFormatHelper(formatProvider);

            switch (GetBasicFormat(format)) {
                case BasicFormat.Compact:
                case BasicFormat.Name:
                    if (!options.IncludeTypeParameters)
                        return name.Name;

                    var withoutMangle = Regex.Replace(name.ElementType.Name, @"`\d", string.Empty);

                    StringBuilder sb = new StringBuilder(withoutMangle);
                    options.PrintGenericArgs(sb, name.GenericArguments);
                    return sb.ToString();

                case BasicFormat.Full:
                case BasicFormat.Roundtrip:
                default:
                    return FullNameFormat.FormatGenericInstanceType(null, name, GetFormatHelper(formatProvider));
            }
        }

        protected internal virtual string FormatModule(string format, ModuleName name, IFormatProvider formatProvider) {
            return DefaultFormat(format, name);
        }

        protected internal virtual string FormatGenericParameter(string format, GenericParameterName name, IFormatProvider formatProvider) {
            switch (GetBasicFormat(format)) {
                case BasicFormat.Compact:
                case BasicFormat.Name:
                case BasicFormat.Full:
                    if (GetFormatHelper(formatProvider).UseGenericParameterPositions)
                        return (name.IsMethodGenericParameter ? "``" : "`" ) + name.Position;
                    else
                        return name.Name;

                case BasicFormat.Roundtrip:
                default:
                    StringBuilder buffer = new StringBuilder();

                    // System.IComparer`1!T, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
                    BuildFullName(buffer, name.DeclaringType, formatProvider);
                    buffer.Append('!');
                    buffer.Append(name.Name);
                    if (name.Assembly != null) {
                        buffer.Append(", ");
                        buffer.Append(name.Assembly.FullName);
                    }
                    return buffer.ToString();
            }
        }

        protected internal virtual string FormatPointerType(string format, PointerTypeName name, IFormatProvider formatProvider) {
            if (name == null)
                throw new ArgumentNullException("name");

            return FormatType(format, name.ElementType, formatProvider) + "*";
        }

        protected internal virtual string FormatParameter(string format, ParameterName name, IFormatProvider formatProvider) {
            if (name == null)
                throw new ArgumentNullException("name");

            if (string.IsNullOrEmpty(format))
                format = GetDefaultFormat(formatProvider, SymbolType.Parameter);

            else if (format.Length > 2)
                throw new FormatException();

            // Valid formats:
            // {G|C|N|F|U} {V|v}?
            char c = format[0];
            var modifiers = name.Modifiers.Count == 0
                ? ""
                : name.Modifiers.ToString() + " ";

            string over = modifiers + "{1} {0}"; // Beans cool
            if (format.Length == 2) {
                if (format[1] == 'V')
                    over = "{0}:{1}"; // cool:Beans
                else if (format[1] == 'v')
                    over = "{1}"; // Beans
                else
                    throw new FormatException();
            }

            if (string.IsNullOrEmpty(name.Name))
                over = "{1}"; // Beans

            switch (c) {
                case 'G':
                case 'C':
                case 'N':
                case 'F':
                case 'U':
                    if (string.IsNullOrEmpty(name.Name) && name.ParameterType == null) {
                        return string.Empty;
                    }
                    string pmType = string.Empty;
                    if (name.ParameterType != null) {
                        pmType = name.ParameterType.ToString(c.ToString(), formatProvider);
                    }
                    return string.Format(over, name.Name, pmType);

                default:
                    throw new FormatException();
            }
        }

        protected internal virtual string FormatAssembly(string format, AssemblyName name, IFormatProvider provider) {
            if (name == null)
                throw new ArgumentNullException("name");
            switch (GetBasicFormat(format)) {
                case BasicFormat.Compact:
                case BasicFormat.Name:
                case BasicFormat.Full:
                    return name.GenerateString(false);
                case BasicFormat.Roundtrip:
                default:
                    return name.GenerateString(true);
            }
        }

        protected internal virtual string FormatGenericInstanceMethod(string format, GenericInstanceMethodName name, IFormatProvider formatProvider) {
            return DefaultFormat(format, name);
        }

        protected internal virtual string FormatMethod(string format, MethodName name, IFormatProvider formatProvider) {
            StringBuilder sb = new StringBuilder();

            var options = GetFormatHelper(formatProvider);
            sb.Append(ComputeMethodDeclName(GetBasicFormat(format), name, options));

            if (options.IncludeReturnTypes.Method && name.ReturnType != null) {
                string returnTypeFormat = string.Concat((options.DefaultFormatString.Parameter ?? string.Empty).Take(1));
                sb.Append(":");
                sb.Append(Format(returnTypeFormat, name.ReturnType, formatProvider));
            }

            return sb.ToString();
        }

        private static string ComputeMethodDeclName(BasicFormat f,
                                                    MethodName name,
                                                    MetadataNameFormat formatProvider)
        {
            switch (f) {
                case BasicFormat.Compact:
                    return CompactFormat.FormatMethod(null, name, formatProvider);

                case BasicFormat.Name:
                    return name.Name;

                case BasicFormat.NameOverloads:
                    return NameOverloadsFormat.FormatMethod(null, name, formatProvider);

                case BasicFormat.Full:
                    return FullNameFormat.FormatMethod(null, name, formatProvider);

                case BasicFormat.Roundtrip:
                default:
                    return RoundtrippableFormat.FormatMethod(null, name, formatProvider);
            }
        }

        private void AppendPropertyParameters(StringBuilder sb,
                                              PropertyName name,
                                              string format = null,
                                              IFormatProvider formatProvider = null)
        {
            if (name.Parameters.Count == 0)
                return;

            this.AppendParameterList(sb,
                                     name.Parameters,
                                     "[]",
                                     format,
                                     formatProvider);
        }

        private void AppendMethodParameters(StringBuilder sb,
                                            IEnumerable<ParameterName> parms,
                                            string format = null,
                                            IFormatProvider formatProvider = null)
        {
            this.AppendParameterList(sb,
                                     parms,
                                     "()",
                                     format,
                                     formatProvider);
        }

        protected virtual void AppendParameters(StringBuilder buffer,
                                                IEnumerable<ParameterName> parameters,
                                                string format = null,
                                                IFormatProvider formatProvider = null) {
            if (buffer == null)
                throw new ArgumentNullException("buffer");
            if (parameters == null)
                throw new ArgumentNullException("parameters");

            bool needComma = false;
            foreach (var p in parameters) {
                if (needComma)
                    buffer.Append(", ");

                string text = p.ToString(format, formatProvider);
                if (needComma && text.Length == 0)
                    buffer.Length--;

                buffer.Append(text);
                needComma = true;
            }
        }

        internal void AppendParameterList(StringBuilder buffer,
                                          IEnumerable<ParameterName> parameters,
                                          string listChar = "()",
                                          string format = null,
                                          IFormatProvider formatProvider = null) {

            buffer.Append(listChar[0]);
            AppendParameters(buffer, parameters, format, formatProvider);
            buffer.Append(listChar[1]);
        }

        static MetadataNameFormat CreateDefault() {
            var result = new MetadataNameFormat(false);
            result.MakeReadOnly();
            return result;
        }

        string GetDefaultFormat(IFormatProvider formatProvider, SymbolType symbolType) {
            var options = GetFormatHelper(formatProvider);
            return options.DefaultFormatString[symbolType] ?? "G";
        }

        MetadataNameFormat GetFormatHelper(IFormatProvider formatProvider) {
            if (formatProvider == null)
                return this;

            var options = formatProvider.GetFormat(typeof(MetadataNameFormat)) as MetadataNameFormat;
            if (options == null)
                return this;
            else
                return options;
        }

        private void ThrowIfReadOnly() {
            if (this.IsReadOnly)
                throw Failure.Sealed();
        }

        private static string DefaultFormat(string format, MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            switch (GetBasicFormat(format)) {
                case BasicFormat.Compact:
                case BasicFormat.Name:
                case BasicFormat.NameOverloads:
                    return name.Name;

                case BasicFormat.Full:
                case BasicFormat.Roundtrip:
                default:
                    return name.FullName;
            }
        }

        static void AppendNameSeparator(StringBuilder buffer, string name) {
            if (name.Length > 0 && name.IndexOf('.', 1) >= 0)
                buffer.Append("::");
            else
                buffer.Append('.');
        }

        enum BasicFormat {
            Compact,
            Name,
            NameOverloads,
            Full,
            Roundtrip,
        }

        static BasicFormat GetBasicFormat(string format) {
            format = format ?? string.Empty;
            if (format.Length > 1)
                throw new FormatException();

            switch (format) {
                case "C":
                    return BasicFormat.Compact;
                case "N":
                    return BasicFormat.Name;
                case "M":
                    return BasicFormat.NameOverloads;
                case "F":
                case "G":
                case "":
                case null:
                    return BasicFormat.Full;
                case "U":
                    return BasicFormat.Roundtrip;
                default:
                    throw new FormatException();
            }
        }

        private void PrintGenericArgs(StringBuilder buffer, IEnumerable<TypeName> genericArguments) {
            // TODO We use Parameter to decide formatting of generics (probably need to
            // introduce an alternative format)
            var options = GetFormatHelper(this);
            var defaultFormat = GetDefaultFormat(options, SymbolType.Parameter).Substring(0, 1);
            var types = genericArguments;

            if (types != null && types.Any()) {
                buffer.Append('<');
                bool needComma = false;
                foreach (var s in types) {
                    if (needComma)
                        buffer.Append(", ");

                    buffer.Append(s.ToString(defaultFormat, options));
                    needComma = true;
                }
                buffer.Append('>');
            }
        }

        private void PrintGenericParamsOrArgs(
            StringBuilder buffer,
            MethodName name,
            bool usePositional) {

            IReadOnlyList<GenericParameterName> genericParameters = name.GenericParameters;
            IEnumerable<TypeName> genericArguments = name.GenericArguments;

            IEnumerable<TypeName> types = genericParameters;
            if (types == null || !types.Any()) {
                types = genericArguments;
            }

            if (genericParameters != null && genericParameters.Count > 0) {
                usePositional |= genericParameters[0].IsPositional;
            }

            // TODO We use Parameter to decide formatting of generics (probably need to
            // introduce an alternative format)
            var options = GetFormatHelper(this);
            var defaultFormat = GetDefaultFormat(options, SymbolType.Parameter).Substring(0, 1);

            if (types != null && types.Any()) {
                if (usePositional) {
                    buffer.Append("``" + types.Count());
                    return;
                }

                buffer.Append('<');
                bool needComma = false;
                foreach (var s in types) {
                    if (needComma) {
                        buffer.Append(", ");
                    }

                    buffer.Append(s.ToString(defaultFormat, this));
                    needComma = true;
                }
                buffer.Append('>');
            }
        }

        private void BuildFullName(StringBuilder buffer,
                                   TypeName name,
                                   IFormatProvider formatProvider) {
            if (name.DeclaringType == null) {
                if (name.Namespace.Length > 0) {
                    buffer.Append(name.Namespace);
                    buffer.Append('.');
                }

            } else {
                BuildFullName(buffer, name.DeclaringType, formatProvider);
                buffer.Append('+');
            }

            var options = GetFormatHelper(formatProvider);
            var dtn = name as DefaultTypeName;
            if (dtn != null) {
                buffer.Append(dtn.FormatName(options.UseGenericParameterPositions || !options.IncludeTypeParameters,
                                             options.UseGenericParameterPositions,
                                             false));
                return;
            }

            var git = (GenericInstanceTypeName) name;
            var withoutMangle = Regex.Replace(git.ElementType.Name, @"`\d", string.Empty);
            buffer.Append(withoutMangle);
            options.PrintGenericArgs(buffer, git.GenericArguments);
        }
    }
}
