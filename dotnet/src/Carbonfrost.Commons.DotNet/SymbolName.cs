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

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public sealed class SymbolName {

        private readonly FieldName _field;
        private readonly PropertyName _property;
        private readonly EventName _event;
        private readonly MethodName _method;
        private readonly TypeName _type;
        private readonly NamespaceName _namespace;
        private readonly ModuleName _module;
        private readonly AssemblyName _assembly;

        private readonly String _text;
        private SymbolTypes _typesCache;

        internal SymbolName(string text, out bool success) {
            _text = text;
            success = false;
            success |= MethodName.TryParse(text, out _method);
            success |= FieldName.TryParse(text, out _field);
            success |= PropertyName.TryParse(text, out _property);
            success |= EventName.TryParse(text, out _event);
            success |= TypeName.TryParse(text, out _type);
            success |= ModuleName.TryParse(text, out _module);
            success |= AssemblyName.TryParse(text, out _assembly);
            success |= NamespaceName.TryParse(text, out _namespace);
        }

        public bool IsAssembly { get { return Assembly != null; } }
        public bool IsEvent { get { return Event != null; } }
        public bool IsField { get { return Field != null; } }
        public bool IsMethod { get { return Method != null; } }
        public bool IsModule { get { return Module != null; } }
        public bool IsNamespace { get { return Namespace != null; } }
        public bool IsProperty { get { return Property != null; } }
        public bool IsType { get { return Type != null; } }

        public SymbolTypes SymbolTypes {
            get {
                if (_typesCache == null) {
                    _typesCache = GenerateSymbolTypes();
                }

                return _typesCache;
            }
        }

        public FieldName Field {
            get {
                return _field;
            }
        }

        public PropertyName Property {
            get {
                return _property;
            }
        }

        public EventName Event {
            get {
                return _event;
            }
        }

        public MethodName Method {
            get {
                return _method;
            }
        }

        public NamespaceName Namespace {
            get {
                return _namespace;
            }
        }

        public AssemblyName Assembly {
            get {
                return _assembly;
            }
        }

        public ModuleName Module {
            get {
                return _module;
            }
        }

        public TypeName Type {
            get {
                return _type;
            }
        }

        public string OriginalString {
            get { return _text; }
        }

        public override string ToString() {
            return _text;
        }

        public static SymbolName Parse(string text) {
            return Utility.Parse<SymbolName>(text, _TryParse);
        }

        public static bool TryParse(string text, out SymbolName result) {
            return _TryParse(text, out result) == null;
        }

        public MetadataName ConvertTo(SymbolType type) {
            switch (type) {
                case SymbolType.Field:
                    return Field;

                case SymbolType.Property:
                    return Property;

                case SymbolType.Event:
                    return Event;

                case SymbolType.Method:
                    return Method;

                case SymbolType.Type:
                    return Type;

                case SymbolType.Namespace:
                    return Namespace;

                case SymbolType.Module:
                    return Module;

                case SymbolType.Assembly:
                    return Assembly;

                case SymbolType.Parameter:
                case SymbolType.InternedLocation:
                case SymbolType.Resource:
                case SymbolType.Local:
                case SymbolType.Alias:
                case SymbolType.Attribute:
                case SymbolType.Unknown:
                case SymbolType.Label:
                    // throw DotNetFailure.CannotConvertToSymbolType("type", type);
                default:
                    throw Failure.NotDefinedEnum("type", type);
            }
        }

        private SymbolTypes GenerateSymbolTypes() {
            SymbolTypes result = new SymbolTypes(false);

            result.Assembly = IsAssembly;
            result.Event = IsEvent;
            result.Field = IsField;
            result.Method = IsMethod;
            result.Module = IsModule;
            result.Namespace = IsNamespace;
            result.Property = IsProperty;
            result.Type = IsType;

            result.MakeReadOnly();
            return result;
        }

        static Exception _TryParse(string text, out SymbolName result) {
            result = null;

            if (text == null) {
                return new ArgumentNullException("text");
            }
            if (text.Length == 0) {
                return Failure.EmptyString("text");
            }

            bool success;
            result = new SymbolName(text, out success);

            if (success) {
                return null;
            } else {
                return Failure.NotParsable("text", typeof(SymbolName));
            }
        }

    }
}
