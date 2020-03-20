//
// Copyright 2013 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Linq;
using Carbonfrost.Commons.DotNet;
using Carbonfrost.Commons.DotNet.Documentation;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.DotNet.Documentation {

    public class EventCodeReferenceTests {

        [Fact]
        public void parse_event_nominal() {
            var cr = CodeReference.Parse("E:System.Json.JsonValue.Changed");
            Assert.Equal(SymbolType.Event, cr.MetadataName.SymbolType);
            EventName name = (EventName) cr.MetadataName;
            Assert.Equal("System.Json.JsonValue", name.DeclaringType.FullName);
            Assert.Equal("Changed", name.Name);

            Assert.Equal("E:System.Json.JsonValue.Changed", cr.ToString());
        }

        [Fact]
        public void parse_event_explicit_interface() {
            var cr = CodeReference.Parse("E:System.Collections.ObjectModel.ObservableCollection`1.System#ComponentModel#INotifyPropertyChanged#PropertyChanged");
            Assert.Equal(SymbolType.Event, cr.MetadataName.SymbolType);

            EventName name = (EventName) cr.MetadataName;
            Assert.Equal("System.Collections.ObjectModel.ObservableCollection`1::System.ComponentModel.INotifyPropertyChanged.PropertyChanged", name.FullName);
            Assert.Equal("System.Collections.ObjectModel.ObservableCollection`1", name.DeclaringType.FullName);
            Assert.Equal("System.ComponentModel.INotifyPropertyChanged.PropertyChanged", name.Name);

            Assert.Equal("E:System.Collections.ObjectModel.ObservableCollection`1.System#ComponentModel#INotifyPropertyChanged#PropertyChanged", cr.ToString());
        }

    }

}
