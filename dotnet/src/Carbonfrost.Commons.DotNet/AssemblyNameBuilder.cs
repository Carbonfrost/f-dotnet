//
// Copyright 2013, 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

namespace Carbonfrost.Commons.DotNet {

    public class AssemblyNameBuilder {

        public string CultureName { get; set; }
        public Version Version { get; set; }
        public Blob PublicKey { get; set; }
        public Blob PublicKeyToken { get; set; }
        public TargetArchitecture Architecture { get; set; }
        public string Name { get; set; }

        public string FullName {
            get {
                return Build().FullName;
            }
            set {
                AssemblyName result;
                if (AssemblyName.TryParse(value, out result)) {
                    InitializeFrom(result);
                } else {
                    Reset();
                }

            }
        }

        public AssemblyNameBuilder() {}

        public AssemblyNameBuilder(AssemblyNameBuilder other)  {
            if (other == null) {
                return;
            }

            CultureName = other.CultureName;
            Version = other.Version;
            PublicKey = other.PublicKey;
            PublicKeyToken = other.PublicKeyToken;
            Architecture = other.Architecture;
            Name = other.Name;
        }

        public AssemblyNameBuilder(AssemblyName other)  {
            InitializeFrom(other);
        }

        public AssemblyName Build() {
            return new DefaultAssemblyName(Name, PublicKey, CultureName, Version, Architecture, PublicKeyToken);
        }

        void InitializeFrom(AssemblyName other) {
            if (other == null) {
                return;
            }

            CultureName = other.CultureName;
            Version = other.Version;
            PublicKey = other.PublicKey;
            PublicKeyToken = other.PublicKeyToken;
            Architecture = other.Architecture;
            Name = other.Name;
        }

        void Reset() {
            CultureName = null;
            Version = null;
            PublicKey = null;
            Architecture = default(TargetArchitecture);
            Name = null;
        }

        public string ToString(string format, IFormatProvider formatProvider = null) {
            return Build().ToString(format, formatProvider);
        }

        internal Exception ParseDictionary(IDictionary<string, string> d) {
            string s = null;
            if (d.TryGetValue("Version", out s)) {
                this.Version = new Version(s);
                d.Remove("Version");
            }

            if (d.TryGetValue("Culture", out s)) {
                if (string.IsNullOrEmpty(CultureName) || s == "neutral" || s == "null") {
                    CultureName = "neutral";
                } else {
                    CultureName = s;
                }
                d.Remove("Culture");
            }

            if (d.TryGetValue("Architecture", out s)) {
                this.Architecture = TargetArchitecture.Parse(s);
                d.Remove("Architecture");
            }

            if (d.TryGetValue("PublicKeyToken", out s)) {
                this.PublicKeyToken = Blob.Parse(s);
                d.Remove("PublicKeyToken");
            }

            if (d.TryGetValue("PublicKey", out s)) {
                this.PublicKey = Blob.Parse(s);
                d.Remove("PublicKey");
            }

            // TODO Consider error on additional keys; key and token
            if (d.Count > 0) {}
            if (this.PublicKey != null && this.PublicKeyToken != null) {}

            return null;
        }

    }
}
