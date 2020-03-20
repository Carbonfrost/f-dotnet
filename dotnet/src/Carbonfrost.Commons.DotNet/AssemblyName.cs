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
using System.Collections.Generic;
using System.Text;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

using ReflectionAssemblyName = System.Reflection.AssemblyName;

namespace Carbonfrost.Commons.DotNet {

    [Builder(typeof(AssemblyNameBuilder))]
    public abstract class AssemblyName : MetadataName {

        static readonly Blob EcmaPub = Blob.Parse("00000000000000000400000000000000");

        public abstract string CultureName { get; }
        public abstract Version Version { get; }
        public abstract TargetArchitecture Architecture { get; }

        internal bool IsCorlib {
            get {
                return Name == "mscorlib"
                    && (PublicKey == EcmaPub || PublicKeyToken == EcmaPub.Token);
            }
        }

        public abstract Blob PublicKey { get; }
        public abstract Blob PublicKeyToken { get; }

        public AssemblyNameComponents Components {
            get {
                AssemblyNameComponents a = AssemblyNameComponents.Name;
                if (PublicKey != null || PublicKeyToken != null) {
                    a |= AssemblyNameComponents.PublicKeyOrToken;
                }
                if (Version != null) {
                    a |= AssemblyNameComponents.Version;
                }
                if (CultureName != null) {
                    a |= AssemblyNameComponents.Culture;
                }
                if (Architecture != null) {
                    a |= AssemblyNameComponents.Architecture;
                }
                return a;
            }
        }

        public AssemblyName Update(string name = null,
                                   Version version = null,
                                   Blob publicKey = null,
                                   Blob publicKeyToken = null,
                                   string cultureName = null,
                                   TargetArchitecture architecture = null) {
            if ((name == null || name == Name)
                && (version == null || version == Version)
                && (publicKey == null || publicKey == PublicKey)
                && (publicKeyToken == null || publicKeyToken == PublicKeyToken)
                && (cultureName == null || cultureName == CultureName)
                && (architecture == null || architecture == Architecture)
               ) {
                return this;
            }

            return new DefaultAssemblyName(name ?? Name, publicKey ?? PublicKey, cultureName ?? CultureName,
                                           version ?? Version, architecture ?? Architecture,
                                           publicKeyToken ?? PublicKeyToken);
        }

        protected AssemblyName() {}

        public static bool TryParse(string text, out AssemblyName result) {
            return _TryParse(text, out result) == null;
        }

        public static AssemblyName Parse(string text) {
            AssemblyName result;
            Exception ex = _TryParse(text, out result);
            if (ex == null)
                return result;
            else
                throw ex;
        }

        internal static Exception _TryParse(string text, out AssemblyName result) {
            result = null;

            if (text == null)
                return new ArgumentNullException("text");

            text = text.Trim();

            if (text.Length == 0)
                return Failure.AllWhitespace("text");

            string[] items = text.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            try {
                string name = items[0];
                if (items.Length == 1) {
                    result = new DefaultAssemblyName(name, null);
                    return null;
                }

                var lookup = new Dictionary<string, string>();
                for (int i = 1; i < items.Length; i++) {
                    string s = items[i];
                    int index = s.IndexOf('=');

                    if (index < 0) {
                        throw Failure.NotParsable("text", typeof(AssemblyName));
                    }

                    string key = s.Substring(0, index).Trim();
                    string value = s.Substring(index + 1).Trim();
                    lookup.Add(key, value);
                }

                AssemblyNameBuilder builder = new AssemblyNameBuilder();
                builder.Name = name;
                Exception exception = builder.ParseDictionary(lookup);

                if (exception == null) {
                    result = builder.Build();
                    return null;
                }
                return exception;
            }

            catch (ArgumentException a) {
                return Failure.NotParsable("text", typeof(AssemblyName), a);
            }
            catch (FormatException f) {
                return Failure.NotParsable("text", typeof(AssemblyName), f);
            }
        }

        public sealed override string FullName {
            get { return GenerateString(false); }
        }

        public abstract override string Name { get; }

        public sealed override SymbolType SymbolType {
            get { return SymbolType.Assembly; } }

        internal override MetadataName AddRight(MetadataName name) {
            if (name.SymbolType == SymbolType.Type) {
                return ((TypeName) name).WithAssembly(this);
            }
            return base.AddRight(name);
        }

        public sealed override bool Matches(MetadataName name) {
            if (name == null)
                throw new ArgumentNullException("name");

            return name.SymbolType == SymbolType.Assembly && Matches((AssemblyName) name);
        }

        public bool Matches(AssemblyName name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (object.ReferenceEquals(this, name))
                return true;

            return this.Name == name.Name
                && BlobMatches(this, name)
                && CultureMatches(CultureName, name.CultureName)
                && VersionMatches(this.Version, name.Version);
        }

        public static AssemblyName Create(string name) {
            return Create(name, null);
        }

        public static AssemblyName Create(string name, Version version) {
            return new DefaultAssemblyName(name,
                                           null,
                                           null,
                                           version,
                                           null,
                                           null);
        }

        public AssemblyName WithPublicKey(Blob publicKey) {
            return Update(publicKey: publicKey);
        }

        public AssemblyName WithPublicKeyToken(Blob publicKeyToken) {
            return Update(publicKeyToken: publicKeyToken);
        }

        public AssemblyName WithVersion(Version version) {
            return Update(version: version);
        }

        public AssemblyName WithName(string name) {
            return Update(name: name);
        }

        static bool CultureMatches(string a, string b) {
            return a == null || (a.Equals(b));
        }

        static bool BlobMatches(AssemblyName a, AssemblyName b) {
            if (a.PublicKey == null && a.PublicKeyToken == null)
                return true;
            if (a.PublicKey == null) {
                return a.PublicKeyToken.Equals(b.PublicKeyToken);
            } else {
                return a.PublicKey.Equals(b.PublicKey)
                    || a.PublicKey.Token.Equals(b.PublicKeyToken);
            }
        }

        static bool VersionMatches(Version a, Version b) {
            return a== null || (a.Major == b.Major
                                && a.Minor == b.Minor
                                && (a.Build == -1 || a.Build == b.Build)
                                && (a.Revision == -1 || a.Revision == b.Revision));
        }

        internal override string Accept(MetadataNameFormat formatter, string format, IFormatProvider provider) {
            return formatter.FormatAssembly(format, this, provider);
        }

        public TypeName GetType(string fullName) {
            if (fullName == null)
                throw new ArgumentNullException("fullName");
            if (fullName.Length == 0)
                throw Failure.EmptyString("fullName");

            TypeName tn;
            if (!TypeName.TryParse(fullName, out tn))
                throw Failure.NotParsable("fullName", typeof(TypeName));
            if (tn.IsTypeSpecification)
                throw DotNetFailure.ArgumentCannotBeSpecificationType("fullName");

            return tn.WithAssembly(this);
        }

        public TypeName GetType(string ns, string name) {
            if (name == null)
                throw new ArgumentNullException("name");
            if (name.Length == 0)
                throw Failure.EmptyString("name");

            return DefaultTypeName.FromFullName(ns, name, this);
        }

        public static AssemblyName FromAssemblyName(ReflectionAssemblyName assemblyName) {
            if (assemblyName == null) {
                throw new ArgumentNullException("assemblyName");
            }

            return new DefaultAssemblyName(assemblyName);
        }

        public AssemblyName GetComponents(AssemblyNameComponents components) {
            throw new NotImplementedException();
        }

        internal string GenerateString(bool useFullKey) {
            StringBuilder sb = new StringBuilder();
            sb.Append(Name);
            if (Version != null)
                sb.AppendFormat(", Version={0}", Version);

            if (string.IsNullOrEmpty(CultureName)) {
                // Skip
            } else if (CultureName == "neutral" || CultureName == "null") {
                sb.AppendFormat(", Culture=neutral");
            } else {
                sb.AppendFormat(", Culture={0}", CultureName);
            }

            if (useFullKey && PublicKey != null) {
                sb.AppendFormat(", PublicKey={0}", PublicKey);
            } else if (PublicKeyToken != null) {
                sb.AppendFormat(", PublicKeyToken={0}", PublicKeyToken);
            }

            return sb.ToString();
        }

    }
}
