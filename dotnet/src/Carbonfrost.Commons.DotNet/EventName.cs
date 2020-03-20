//
// Copyright 2013, 2017, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;

namespace Carbonfrost.Commons.DotNet {

    public sealed partial class EventName : MemberName {

        private readonly string _name;

        public EventNameComponents Components {
            get {
                var result = EventNameComponents.Name;
                if (DeclaringType != null) {
                    result |= EventNameComponents.DeclaringType;
                }
                if (EventType != null) {
                    result |= EventNameComponents.EventType;
                }
                return result;
            }
        }

        public MethodName AddMethod {
            get {
                return AccessorMethod("add_");
            }
        }

        public MethodName RemoveMethod {
            get {
                return AccessorMethod("remove_");
            }
        }

        public MethodName RaiseMethod {
            get {
                return AccessorMethod("raise_");
            }
        }

        public TypeName EventType {
            get;
            private set;
        }

        public override SymbolType SymbolType {
            get {
                return SymbolType.Event;
            }
        }

        internal EventName(TypeName type, string name, TypeName eventType) : base(type) {
            _name = name;
            EventType = eventType;
        }

        public static EventName FromEventInfo(EventInfo eventInfo) {
            if (eventInfo == null) {
                throw new ArgumentNullException("eventInfo");
            }

            return new EventName(TypeName.FromType(eventInfo.DeclaringType),
                                 eventInfo.Name,
                                 TypeName.FromType(eventInfo.EventHandlerType));
        }

        public override string Name {
            get {
                return _name;
            }
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.SymbolType == SymbolType.Event && Matches((EventName) name);
        }

        protected override MemberName WithDeclaringTypeOverride(TypeName declaringType) {
            return new EventName(declaringType, _name, EventType);
        }

        protected override MemberName WithAssemblyOverride(AssemblyName assembly) {
            return WithDeclaringType(DeclaringType.WithAssembly(assembly));
        }

        public bool Matches(EventName name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }

            return name.Name == this.Name
                && TypeName.SafeMatch(this.EventType, name.EventType);
        }

        public static EventName Parse(string text) {
            return Utility.Parse<EventName>(text, _TryParse);
        }

        public static bool TryParse(string text, out EventName result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out EventName result) {
            // System.ComponentModel.Component.Disposing:EventHandler
            return TypeName.ParseSimpleMember(
                text, out result, (a, b, c) => (new EventName(a, b, c)));
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatEvent(format, this, provider);
        }

        private MethodName AccessorMethod(string prefix) {
            if (EventType == null) {
                return DeclaringType.GetMethod(prefix + Name).WithParametersUnspecified();
            }
            return DeclaringType.GetMethod(prefix + Name, EventType);
        }
    }
}
