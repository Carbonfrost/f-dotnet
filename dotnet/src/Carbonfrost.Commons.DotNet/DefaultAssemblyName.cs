//
// Copyright 2017 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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
using ReflectionAssemblyName = System.Reflection.AssemblyName;

namespace Carbonfrost.Commons.DotNet {

    class DefaultAssemblyName : AssemblyName {

        private readonly Blob _publicKeyBlob;
        private readonly Blob _pkt;
        private readonly string _name;
        private readonly Version _version;
        private readonly string _cultureName;
        private readonly TargetArchitecture _arch;

        public override string CultureName { get { return _cultureName; } }
        public override Version Version { get { return _version; } }
        public override TargetArchitecture Architecture { get { return _arch; } }
        public override string Name { get { return _name; } }

        public override Blob PublicKey {
            get { return _publicKeyBlob; }
        }

        public override Blob PublicKeyToken { get { return _pkt; } }

        internal DefaultAssemblyName(string name,
                              Blob publicKeyBlob,
                              string cultureName = null,
                              Version version = null,
                              TargetArchitecture architecture = null,
                              Blob publicKeyToken = null) {
            _name = name;
            _cultureName = cultureName;
            _version = version;
            _arch = architecture;

            _publicKeyBlob = publicKeyBlob;
            _pkt = publicKeyToken ?? (publicKeyBlob == null ? null : publicKeyBlob.Token);
        }

        internal DefaultAssemblyName(ReflectionAssemblyName assemblyName) {
            _name = assemblyName.Name;
            if (string.IsNullOrEmpty(assemblyName.CultureName)) {
                _cultureName = "neutral";
            } else {
                _cultureName = assemblyName.CultureName;
            }

            var pk = assemblyName.GetPublicKey();
            if (pk != null) {
                _publicKeyBlob = new Blob(pk);
            }

            pk = assemblyName.GetPublicKeyToken();
            if (pk != null) {
                _pkt = new Blob(pk);
            }

            _version = assemblyName.Version;
        }
    }
}
