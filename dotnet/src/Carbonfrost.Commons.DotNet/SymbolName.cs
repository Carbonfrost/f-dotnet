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

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public readonly struct SymbolName {

        private readonly Flags _flags;

        private readonly FieldName _field;
        private readonly PropertyName _property;
        private readonly EventName _event;
        private readonly MethodName _method;
        private readonly TypeName _type;
        private readonly NamespaceName _namespace;
        private readonly ModuleName _module;
        private readonly AssemblyName _assembly;

        private readonly String _text;

        internal SymbolName(string text) {
            _text = text;
            _flags = (MethodName.TryParse(text, out _method) ? Flags.MethodName : 0)
                | (FieldName.TryParse(text, out _field) ? Flags.FieldName : 0)
                | (PropertyName.TryParse(text, out _property) ? Flags.PropertyName : 0)
                | (EventName.TryParse(text, out _event) ? Flags.EventName : 0)
                | (TypeName.TryParse(text, out _type) ? Flags.TypeName : 0)
                | (ModuleName.TryParse(text, out _module) ? Flags.ModuleName : 0)
                | (AssemblyName.TryParse(text, out _assembly) ? Flags.AssemblyName : 0)
                | (NamespaceName.TryParse(text, out _namespace) ? Flags.NamespaceName : 0);
        }

        public bool IsAssembly {
            get {
                return _flags.HasFlag(Flags.AssemblyName);
            }
        }

        public bool IsEvent {
            get {
                return _flags.HasFlag(Flags.EventName);
            }
        }

        public bool IsField {
            get {
                return _flags.HasFlag(Flags.FieldName);
            }
        }

        public bool IsMethod {
            get {
                return _flags.HasFlag(Flags.MethodName);
            }
        }

        public bool IsModule {
            get {
                return _flags.HasFlag(Flags.ModuleName);
            }
        }

        public bool IsNamespace {
            get {
                return _flags.HasFlag(Flags.NamespaceName);
            }
        }

        public bool IsProperty {
            get {
                return _flags.HasFlag(Flags.PropertyName);
            }
        }

        public bool IsType {
            get {
                return _flags.HasFlag(Flags.TypeName);
            }
        }

        public SymbolTypes SymbolTypes {
            get {
                return new SymbolTypes(false) {
                    Assembly = IsAssembly,
                    Event = IsEvent,
                    Field = IsField,
                    Method = IsMethod,
                    Module = IsModule,
                    Namespace = IsNamespace,
                    Property = IsProperty,
                    Type = IsType,
                };
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
            get {
                return _text;
            }
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
                    throw DotNetFailure.CannotConvertToSymbolType(nameof(type), type);

                default:
                    throw Failure.NotDefinedEnum(nameof(type), type);
            }
        }

        static Exception _TryParse(string text, out SymbolName result) {
            result = default(SymbolName);

            if (text == null) {
                return new ArgumentNullException(nameof(text));
            }
            if (text.Length == 0) {
                return Failure.EmptyString(nameof(text));
            }
            result = new SymbolName(text);

            if (result._flags == 0) {
                return Failure.NotParsable(nameof(text), typeof(SymbolName));
            }
            return null;
        }

        enum Flags {
            FieldName = 1 << 1,
            PropertyName = 1 << 2,
            EventName = 1 << 3,
            MethodName = 1 << 4,
            TypeName = 1 << 5,
            NamespaceName = 1 << 6,
            ModuleName = 1 << 7,
            AssemblyName = 1 << 8,
        }
    }
}
