//
// Copyright 2016, 2019 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using System.Reflection;
using System.Text.RegularExpressions;
using Carbonfrost.Commons.Spec;
using Carbonfrost.Commons.DotNet;

namespace Carbonfrost.UnitTests.DotNet {

    public class MemberNameTests {

        [Fact]
        public void FromMember_should_apply_to_members_of_given_types_method() {
            var method = typeof(string).GetMethod("CopyTo");
            Assert.IsInstanceOf(typeof(MethodName), MemberName.FromMemberInfo(method));
        }

        [Fact]
        public void FromMember_should_apply_to_members_of_given_types_property() {
            var property = typeof(string).GetProperty("Length");
            Assert.IsInstanceOf(typeof(PropertyName), MemberName.FromMemberInfo(property));
        }

        [Fact]
        public void FromMember_should_apply_to_members_of_given_types_field() {
            var field = typeof(string).GetField("Empty");
            Assert.IsInstanceOf(typeof(FieldName), MemberName.FromMemberInfo(field));
        }

        class C {
#pragma warning disable 0067
            public event EventHandler E;            
        }

        [Fact]
        public void FromMember_should_apply_to_members_of_given_types_event() {
            var evt = typeof(C).GetEvent("E");
            Assert.IsInstanceOf(typeof(EventName), MemberName.FromMemberInfo(evt));
        }

        [Fact]
        public void FromMember_should_apply_to_members_of_given_types_type() {
            Assert.IsInstanceOf(typeof(TypeName), MemberName.FromMemberInfo(typeof(string).GetTypeInfo()));
        }
    }
}
