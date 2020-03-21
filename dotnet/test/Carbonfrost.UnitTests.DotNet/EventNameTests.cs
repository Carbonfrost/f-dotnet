//
// Copyright 2016, 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class EventNameTests {

        [InlineData("Empty", EventNameComponents.Name)]
        [InlineData("Empty:String", EventNameComponents.Name | EventNameComponents.EventType)]
        [InlineData("String.Empty:String", EventNameComponents.Name | EventNameComponents.EventType
            | EventNameComponents.DeclaringType)]
        [Theory]
        public void Components(string text, EventNameComponents expected) {
            Assert.Equal(expected, EventName.Parse(text).Components);
        }

        [Fact]
        public void Create_should_create_event_name_from_name() {
            var a = EventName.Create("Hello");
            var b = EventName.Parse("Hello");
            Assert.Equal(a, b);
        }

        [Fact]
        public void Create_should_create_event_name_from_name_and_type() {
            var a = EventName.Create("Hello", TypeName.Create("System", "EventHandler"));
            var b = EventName.Parse("Hello:System.EventHandler");
            Assert.Equal(a, b);
        }
    }
}
