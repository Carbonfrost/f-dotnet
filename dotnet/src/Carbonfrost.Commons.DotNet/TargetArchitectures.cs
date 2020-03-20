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

namespace Carbonfrost.Commons.DotNet {

    public static class TargetArchitectures {

        public static readonly TargetArchitecture I386 = new TargetArchitecture("I386");
        public static readonly TargetArchitecture Amd64 = new TargetArchitecture("Amd64");
        public static readonly TargetArchitecture IA64 = new TargetArchitecture("IA64");
        public static readonly TargetArchitecture Arm = new TargetArchitecture("Arm");
    }
}
