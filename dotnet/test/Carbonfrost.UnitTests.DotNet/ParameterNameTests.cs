//
// Copyright 2020 Carbonfrost Systems, Inc. (https://carbonfrost.com)
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

using Carbonfrost.Commons.DotNet;
using Carbonfrost.Commons.Spec;

namespace Carbonfrost.UnitTests.DotNet {

    public class ParameterNameTests {

        [Fact]
        public void FromParameterInfo_should_obtain_correct_parameter() {
            var pm = ParameterName.FromParameterInfo(
                typeof(string).GetMethod("Insert").GetParameters()[1]
            );

            Assert.Equal("value", pm.Name);
            Assert.Equal("Insert", pm.DeclaringMethod.Name);
        }
    }
}
