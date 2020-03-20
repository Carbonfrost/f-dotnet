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
using System.Reflection;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    public sealed class TargetArchitecture : IEquatable<TargetArchitecture>, IComparable<TargetArchitecture> {

        public static readonly TargetArchitecture Amd64 = new TargetArchitecture("Amd64");
        public static readonly TargetArchitecture I386 = new TargetArchitecture("I386");
        public static readonly TargetArchitecture IA64 = new TargetArchitecture("IA64");
        public static readonly TargetArchitecture MSIL = new TargetArchitecture("MSIL");
        public static readonly TargetArchitecture X86 = new TargetArchitecture("X86");
        public static readonly TargetArchitecture Arm = new TargetArchitecture("Arm");

        private readonly string _name;

        public string Name { get { return _name; } }

        public TargetArchitecture(string name) {
            if (name == null) {
                throw new ArgumentNullException("name");
            }
            if (string.IsNullOrEmpty(name)) {
                throw Failure.EmptyString("name");
            }
            _name = name;
        }

        public ProcessorArchitecture ToProcessorArchitecture() {
            switch (Name) {
                case "Amd64":
                    return ProcessorArchitecture.Amd64;
                case "IA64":
                    return ProcessorArchitecture.IA64;
                case "MSIL":
                    return ProcessorArchitecture.MSIL;
                case "Arm":
                    return ProcessorArchitecture.Arm;
                case "I386":
                case "X86":
                    return ProcessorArchitecture.X86;
                default:
                    return ProcessorArchitecture.None;
            }
        }

        public static TargetArchitecture Parse(string text) {
            return Utility.Parse<TargetArchitecture>(text, _TryParse);
        }

        public static bool TryParse(string text, out TargetArchitecture result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out TargetArchitecture result) {
            result = null;
            if (text == null) {
                return new ArgumentNullException("text");
            }

            text = text.Trim();
            if (text.Length == 0) {
                return Failure.AllWhitespace("text");
            }

            result = new TargetArchitecture(text);
            return null;
        }

        public bool Equals(TargetArchitecture other) {
            return other.Name == Name;
        }

        public int CompareTo(TargetArchitecture other) {
            return Name.CompareTo(other.Name);
        }

        public override string ToString() {
            return Name;
        }

    }
}
