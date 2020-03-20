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
using System.IO;
using System.Reflection;
using Carbonfrost.Commons.DotNet.Resources;
using Carbonfrost.Commons.Core;

namespace Carbonfrost.Commons.DotNet {

    static class DotNetFailure {

        public static ArgumentException NotSupportedCodeReferenceConversion(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.NotSupportedCodeReferenceConversion(), argumentName));
        }

        public static Exception GenericParametersLengthMismatch(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.GenericParametersLengthMismatch(), argumentName));
        }

        public static InvalidOperationException AmbiguousMatch() {
            return Failure.Prepare(new InvalidOperationException(SR.AmbiguousMatch()));
        }

        public static NotSupportedException NotSupportedBySpecifications() {
            return Failure.Prepare(new NotSupportedException(SR.NotSupportedBySpecifications()));
        }

        public static ArgumentException ArgumentCannotBeSpecificationType(string argumentName) {
            return Failure.Prepare(new ArgumentException(SR.ArgumentCannotBeSpecificationType(), argumentName));
        }

        public static ArgumentOutOfRangeException UnknownConversionOperatorCannotBeUsed(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.UnknownConversionOperatorCannotBeUsed()));
        }

        public static ArgumentOutOfRangeException BinaryOperatorRequired(string argumentName, OperatorType operatorType) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, operatorType, SR.BinaryOperatorRequired()));
        }

        public static ArgumentOutOfRangeException ConversionOperatorExpected(string argumentName, OperatorType operatorType) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, operatorType, SR.ConversionOperatorExpected()));
        }

        public static ArgumentOutOfRangeException UnaryOperatorExpected(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.UnaryOperatorExpected()));
        }

        public static ArgumentException CannotBindGenericParameterName() {
            return Failure.Prepare(new ArgumentException(SR.CannotBindGenericParameterName()));
        }

        public static InvalidOperationException CannotMakeByReferenceType() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotMakeByReferenceType()));
        }

        public static InvalidOperationException CannotMakeNullableType() {
            return Failure.Prepare(new InvalidOperationException(SR.CannotMakeNullableType()));
        }

        public static ArgumentOutOfRangeException CannotConvertToSymbolType(string argumentName, object argumentValue) {
            return Failure.Prepare(new ArgumentOutOfRangeException(argumentName, argumentValue, SR.CannotConvertToSymbolType()));
        }

    }
}
