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
using System.Globalization;
using System.Linq;

using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    partial class MetadataNameFormat : IFormatProvider, ICustomFormatter {

        string ICustomFormatter.Format(string format, object arg, IFormatProvider formatProvider) {
            return Format(format, (MetadataName) arg, formatProvider);
        }

        public object GetFormat(Type formatType) {
            if (formatType == null)
                throw new ArgumentNullException("formatType");
            if (formatType == typeof(MetadataNameFormat))
                return this;
            else
                return CultureInfo.CurrentCulture.GetFormat(formatType);
        }

        public MetadataNameFormat Clone() {
            MetadataNameFormat result = (MetadataNameFormat) MemberwiseClone();
            result.IsReadOnly = false;
            return result;
        }

        public bool IsReadOnly {
            get;
            private set;
        }

        internal void MakeReadOnly() {
            this.IsReadOnly = true;
            this.IncludeAttributes.MakeReadOnly();
            this.IncludeModifiers.MakeReadOnly();
            this.DefaultFormatString.MakeReadOnly();
        }
    }
}
