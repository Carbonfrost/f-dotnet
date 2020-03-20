
// This file was automatically generated.  DO NOT EDIT or else
// your changes could be lost!

#pragma warning disable 1570

using System;
using System.Globalization;
using System.Resources;
using System.Reflection;

namespace Carbonfrost.Commons.DotNet.Resources {

    /// <summary>
    /// Contains strongly-typed string resources.
    /// </summary>
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("srgen", "1.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    internal static partial class SR {

        private static global::System.Resources.ResourceManager _resources;
        private static global::System.Globalization.CultureInfo _currentCulture;
        private static global::System.Func<string, string> _resourceFinder;

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(_resources, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Carbonfrost.Commons.DotNet.Automation.SR", typeof(SR).GetTypeInfo().Assembly);
                    _resources = temp;
                }
                return _resources;
            }
        }

        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return _currentCulture;
            }
            set {
                _currentCulture = value;
            }
        }

        private static global::System.Func<string, string> ResourceFinder {
            get {
                if (object.ReferenceEquals(_resourceFinder, null)) {
                    try {
                        global::System.Resources.ResourceManager rm = ResourceManager;
                        _resourceFinder = delegate (string s) {
                            return rm.GetString(s);
                        };
                    } catch (global::System.Exception ex) {
                        _resourceFinder = delegate (string s) {
                            return string.Format("localization error! {0}: {1} ({2})", s, ex.GetType(), ex.Message);
                        };
                    }
                }
                return _resourceFinder;
            }
        }


  /// <summary>Multiple values matched the search criteria.</summary>
    internal static string AmbiguousMatch(
    
    ) {
        return string.Format(Culture, ResourceFinder("AmbiguousMatch") );
    }

  /// <summary>The argument cannot be a type specification.</summary>
    internal static string ArgumentCannotBeSpecificationType(
    
    ) {
        return string.Format(Culture, ResourceFinder("ArgumentCannotBeSpecificationType") );
    }

  /// <summary>A binary operator is required.</summary>
    internal static string BinaryOperatorRequired(
    
    ) {
        return string.Format(Culture, ResourceFinder("BinaryOperatorRequired") );
    }

  /// <summary>Cannot bind an unbound generic parameter name because its index is out of range.</summary>
    internal static string CannotBindGenericParameterName(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotBindGenericParameterName") );
    }

  /// <summary>Cannot convert to specified symbol type</summary>
    internal static string CannotConvertToSymbolType(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotConvertToSymbolType") );
    }

  /// <summary>Type cannot be made into a byref type.</summary>
    internal static string CannotMakeByReferenceType(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotMakeByReferenceType") );
    }

  /// <summary>Type cannot be made into a nullable type.</summary>
    internal static string CannotMakeNullableType(
    
    ) {
        return string.Format(Culture, ResourceFinder("CannotMakeNullableType") );
    }

  /// <summary>Conversion operator is expected.</summary>
    internal static string ConversionOperatorExpected(
    
    ) {
        return string.Format(Culture, ResourceFinder("ConversionOperatorExpected") );
    }

  /// <summary>Number of generic arguments does not match the number of parameters defined.</summary>
    internal static string GenericParametersLengthMismatch(
    
    ) {
        return string.Format(Culture, ResourceFinder("GenericParametersLengthMismatch") );
    }

  /// <summary>Type specifications do not support this operation.</summary>
    internal static string NotSupportedBySpecifications(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotSupportedBySpecifications") );
    }

  /// <summary>Specified metadata name does not support conversion to a code reference</summary>
    internal static string NotSupportedCodeReferenceConversion(
    
    ) {
        return string.Format(Culture, ResourceFinder("NotSupportedCodeReferenceConversion") );
    }

  /// <summary>A unary operator type is expected.</summary>
    internal static string UnaryOperatorExpected(
    
    ) {
        return string.Format(Culture, ResourceFinder("UnaryOperatorExpected") );
    }

  /// <summary>Unknown or conversion operator type cannot be specified for this argument.</summary>
    internal static string UnknownConversionOperatorCannotBeUsed(
    
    ) {
        return string.Format(Culture, ResourceFinder("UnknownConversionOperatorCannotBeUsed") );
    }

    }
}
