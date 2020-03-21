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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Carbonfrost.Commons.Core;
using Carbonfrost.Commons.Core.Runtime;

namespace Carbonfrost.Commons.DotNet {

    public sealed class Blob : IReadOnlyList<byte> {

        private readonly byte[] _data;

        public static readonly Blob Empty = new Blob(Array.Empty<byte>());

        public bool IsEmpty {
            get {
                return Length == 0;
            }
        }

        public int Length {
            get {
                return _data.Length;
            }
        }

        public Blob Token {
            get {
                byte[] result = SHA1.Create().ComputeHash(_data);
                byte[] sha1 = new byte[8];

                Array.Copy(result, result.Length - 8, sha1, 0, 8);
                Array.Reverse(sha1);
                return new Blob(sha1);
            }
        }

        int IReadOnlyCollection<byte>.Count {
            get {
                return Length;
            }
        }

        public byte this[int index] {
            get {
                return _data[index];
            }
        }

        [ActivationConstructor]
        public Blob(params byte[] data) {
            if (data == null) {
                throw new ArgumentNullException("data");
            }

            _data = data;
        }

        public byte[] ToByteArray() {
            return (byte[]) _data.Clone();
        }

        public static Blob FromStream(Stream input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }

            MemoryStream ms = new MemoryStream();
            input.CopyTo(ms);
            return new Blob(ms.ToArray());
        }

        public static Blob FromFile(string fileName) {
            return FromStream(StreamContext.FromFile(fileName).OpenRead());
        }

        public static Blob FromSource(Uri uri) {
            return FromStream(StreamContext.FromSource(uri).OpenRead());
        }

        public static Blob FromStreamContext(StreamContext input) {
            if (input == null) {
                throw new ArgumentNullException("input");
            }
            return FromStream(input.OpenRead());
        }

        public static implicit operator Blob(byte[] s) {
            return new Blob(s);
        }

        public override bool Equals(object obj) {
            if (obj is Blob other) {
                return _data.Length == other.Length && _data.SequenceEqual(other._data);
            }
            return false;
        }

        public override int GetHashCode() {
            int hashCode = 0;
            unchecked {
                hashCode += 1000000007 * _data.GetHashCode();
            }

            return hashCode;
        }

        public static bool operator ==(Blob lhs, Blob rhs) {
            if (ReferenceEquals(lhs, rhs)) {
                return true;
            }

            if (ReferenceEquals(lhs, null) || ReferenceEquals(rhs, null)) {
                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Blob lhs, Blob rhs) {
            return !(lhs == rhs);
        }

        public static Blob Parse(string text) {
            return Utility.Parse<Blob>(text, _TryParse);
        }

        public static bool TryParse(string text, out Blob result) {
            return _TryParse(text, out result) == null;
        }

        static Exception _TryParse(string text, out Blob result) {
            if (text != null) {
                text = text.Trim();
                if (text == "null") {
                    result = Blob.Empty;
                    return null;
                }

                try {
                    byte[] data = HexToBytes(text);
                    result = new Blob(data);
                    return null;
                } catch (FormatException) {
                }
            }

            result = null;
            return Failure.NotParsable("text", typeof(Blob));
        }

        public Stream OpenRead() {
            return new MemoryStream(_data, false);
        }

        public override string ToString() {
            if (_data.Length == 0) {
                return "null";
            }
            return string.Concat(_data.Select(t => t.ToString("x2")));
        }

        private static byte[] HexToBytes(string text) {
            if (string.IsNullOrEmpty(text)) {
                return Array.Empty<byte>();
            }

            if ((text.Length % 2) == 1) {
                throw new FormatException();
            }

            byte[] buffer = new byte[text.Length / 2];
            for (int i = 0; i < buffer.Length; i++) {
                buffer[i] = byte.Parse(text.Substring(i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return buffer;
        }

        public IEnumerator<byte> GetEnumerator() {
            return ((IReadOnlyList<byte>) _data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
    }
}
