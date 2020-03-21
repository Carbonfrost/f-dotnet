//
// Copyright 2016 Carbonfrost Systems, Inc. (http://carbonfrost.com)
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

    public class MethodNameTests {

        [InlineData("ToString", MethodNameComponents.Name)]
        [InlineData("ToString()", MethodNameComponents.Name | MethodNameComponents.ParametersSpecified
            | MethodNameComponents.ParameterTypes | MethodNameComponents.ParameterNames)]
        [InlineData("ToString(name:)", MethodNameComponents.Name | MethodNameComponents.ParametersSpecified
            | MethodNameComponents.ParameterNames)]
        [InlineData("ToString(String)", MethodNameComponents.Name | MethodNameComponents.ParametersSpecified
            | MethodNameComponents.ParameterTypes)]
        [InlineData("ToString(:Type):String", MethodNameComponents.Name
            | MethodNameComponents.ParametersSpecified | MethodNameComponents.ParameterTypes
            | MethodNameComponents.ReturnType)]
        [InlineData("String.ToString", MethodNameComponents.Name
            | MethodNameComponents.DeclaringType)]
        [Theory]
        public void Components(string text, MethodNameComponents expected) {
            Assert.Equal(expected, MethodName.Parse(text).Components);
        }

        [Fact]
        public void AddParameter_should_apply_new_parameter_names_and_types() {
            var method = MethodName.Parse("TryParse");
            var newMethod = method.AddParameter("text", TypeName.FromType(typeof(string)))
                                  .AddParameter("value", TypeName.Parse("Byte&"));
            Assert.Equal("TryParse(System.String text, Byte& value)", newMethod.ToString());
        }

        [Fact]
        public void SetParameter_should_apply_new_parameter_names_and_types() {
            var method = MethodName.Parse("TryParse(text:)");
            var newMethod = method.SetParameter(0, "text", TypeName.FromType(typeof(string)));
            Assert.Equal("TryParse(System.String text)", newMethod.ToString());
        }

        [Fact]
        public void SetParameter_should_apply_modifiers() {
            var method = MethodName.Parse("TryParse(text:)");
            var newMethod = method.SetParameter(0, "text", TypeName.FromType(typeof(string)),
                    new [] { TypeName.Parse("Const") }, new [] { TypeName.Parse("Volatile") });
            Assert.Equal("TryParse(modreq(Const) modopt(Volatile) System.String text)", newMethod.ToString());
        }

        [Fact]
        public void SetParameter_should_apply_on_generics() {
            var method = MethodName.Parse("Create``2(``0,``1)");
            Assert.Equal("Create", method.Name);
            var newMethod = method.SetParameter(0, "text", TypeName.FromType(typeof(string)));
            Assert.Equal("Create", newMethod.Name);
            Assert.Equal("Create``2(System.String text, ``1)", newMethod.ToString());
        }

        [Theory]
        [InlineData("TryParse()")]
        [InlineData("TryParse")]
        [InlineData("TryParse(String)")]
        public void RemoveParameters_should_apply_to_names(string name) {
            var method = MethodName.Parse(name);;
            var newMethod = method.RemoveParameters();
            Assert.Equal("TryParse()", newMethod.ToString());
        }

        [Theory]
        [InlineData("TryParse()")]
        [InlineData("TryParse")]
        [InlineData("TryParse(String)")]
        public void WithParametersUnspecified_should_apply_to_names(string name) {
            var method = MethodName.Parse(name);;
            var newMethod = method.WithParametersUnspecified();
            Assert.False(newMethod.HasParametersSpecified);
            Assert.Equal("TryParse", newMethod.ToString());
        }

        [Fact]
        public void SetGenericParameter_should_apply_on_generics() {
            var method = MethodName.Parse("Create``2(``0,``1):``1");
            var newMethod = method.SetGenericParameter(0, "TFirst")
                                  .SetGenericParameter(1, "TSecond");

            Assert.Equal("Create", newMethod.Name);
            Assert.Equal(2, newMethod.GenericParameterCount);
            var format = new MetadataNameFormat();
            format.IncludeReturnTypes.All = true;

            Assert.Equal("Create<TFirst, TSecond>(TFirst, TSecond):TSecond", newMethod.ToString(format));
        }

        [Fact]
        public void SetGenericParameter_should_apply_on_generics_no_return_type() {
            var method = MethodName.Parse("Create``2(``0,``1)");
            var newMethod = method.SetGenericParameter(0, "TFirst")
                                  .SetGenericParameter(1, "TSecond");

            Assert.Equal("Create", newMethod.Name);
            Assert.Equal(2, newMethod.GenericParameterCount);
            var format = new MetadataNameFormat();
            format.IncludeReturnTypes.All = true;

            Assert.Equal("Create<TFirst, TSecond>(TFirst, TSecond)", newMethod.ToString(format));
        }

        [Fact, Skip("Can't yet parse complex method names")]
        public void ToString_formatting_with_complex_names() {
            MethodName id = MethodName.Parse("System.String::System.IConvertible.ToUInt32(), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.Equal("System.IConvertible.ToUInt32", id.ToString("N"));
            Assert.Equal("String::System.IConvertible.ToUInt32()", id.ToString("C"));
            Assert.Equal("System.String::System.IConvertible.ToUInt32()", id.ToString("G"));
            Assert.Equal("System.String::System.IConvertible.ToUInt32()", id.ToString("F"));
            Assert.Equal("System.String::System.IConvertible.ToUInt32(), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", id.ToString("U"));
        }

        [Fact]
        public void ToString_formatting() {
            MethodName id = MethodName.Parse("System.IComparer.CompareTo(x:System.Object, y:System.Object), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089");

            Assert.Equal("CompareTo", id.ToString("N"));
            Assert.Equal("IComparer.CompareTo(Object, Object)", id.ToString("C"));
            Assert.Equal("System.IComparer.CompareTo(System.Object x, System.Object y)", id.ToString("G"));
            Assert.Equal("System.IComparer.CompareTo(System.Object x, System.Object y)", id.ToString("F"));
            Assert.Equal("System.IComparer.CompareTo(System.Object x, System.Object y), mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", id.ToString("U"));

            // TODO Roundtrip assembly names on parameter types...

            // C -- compact form (minimal qualification in names)
            // G -- general form
            // N -- the plain name
            // F -- the full name
            // U -- the roundtrippable name
        }

        [Fact]
        public void FromMethod_nominal() {
            MethodName m = MethodName.FromMethodInfo(typeof(string).GetTypeInfo().GetMethod("CopyTo"));
            Assert.Equal("System.String.CopyTo(System.Int32 sourceIndex, System.Char[] destination, System.Int32 destinationIndex, System.Int32 count)", m.FullName);
        }

        // N.B. Curiously, C# 'out' parameters are considered ByReference type specs,
        // and so are 'ref' parameters -- BUT only 'out' parameters should
        // use IsOut

        [Fact]
        public void FromMethod_out_parameter_nominal() {
            MethodName m = MethodName.FromMethodInfo(typeof(char).GetTypeInfo().GetMethod("TryParse"));
            Assert.Equal("System.Char.TryParse(System.String s, System.Char& result)", m.FullName);
            Assert.True(m.Parameters[1].ParameterType.IsByReference);
        }

        [Fact]
        public void FromMethod_ref_parameter_nominal() {
            MethodName m = MethodName.FromMethodInfo(typeof(Array).GetTypeInfo().GetMethod("Resize"));

            Assert.Equal("System.Array.Resize<T>(T[]& array, System.Int32 newSize)", m.FullName);
            Assert.True(m.Parameters[0].ParameterType.IsByReference);
        }

        [Fact]
        public void FromMethod_generic_method() {
            // Array.Sort<TKey, TValue>(TKey[],TValue[],int,int)

            var method = typeof(Array).GetTypeInfo().GetMethods().Where(t => t.GetGenericArguments().Length == 2 && t.GetParameters().Length == 4).Single();
            MethodName m = MethodName.FromMethodInfo(method);

            Assert.Equal(method.Name, m.Name);
            Assert.Equal("Sort", m.Name);
            Assert.Equal("System.Array.Sort<TKey, TValue>(TKey[] keys, TValue[] items, System.Int32 index, System.Int32 length)", m.FullName);
            Assert.Equal("Void", m.ReturnType.Name);
        }

        class S<T> {
            public static T From() { return default(T); }
        }

        [Fact]
        public void FromMethod_open_generic_method_return_type() {
            var method = typeof(S<>).GetTypeInfo().GetMethods().Where(t => t.IsStatic).Single();
            MethodName m = MethodName.FromMethodInfo(method);

            Assert.Equal(method.Name, m.Name);
            Assert.Equal("From", m.Name);
            Assert.Equal("T", m.ReturnType.Name);
        }

        [Fact]
        public void FromMethod_closed_generic_method_return_type() {
            var method = typeof(S<int>).GetTypeInfo().GetMethods().Where(t => t.IsStatic).Single();
            MethodName m = MethodName.FromMethodInfo(method);

            Assert.Equal(method.Name, m.Name);
            Assert.Equal("From", m.Name);
            Assert.Equal("Int32", m.ReturnType.Name);
        }

        [Fact]
        public void FromMethod_generic_arguments_binding() {
            MethodName concatMethod = MethodName.FromMethodInfo(
                typeof(string).GetTypeInfo().GetMethods().Where(t => t.Name == "Concat" && t.IsGenericMethod).Single());

            // Concat<T>(IEnumerable<T>)
            var gits = (GenericInstanceTypeName) concatMethod.Parameters[0].ParameterType;
            Assert.Equal(concatMethod,
                        ((GenericParameterName) gits.GenericArguments[0]).DeclaringMethod);
        }

        [Fact]
        public void AssociatedMember_creates_property_name_semantics() {
            var getter = MethodName.Parse("System.Array.get_Length()");
            var setter = MethodName.Parse("System.Array.set_Length(value:Int32)");
            var prop = PropertyName.Parse("System.Array.Length:Int32");

            Assert.Equal(prop, getter.AssociatedProperty);
            Assert.Equal(prop, setter.AssociatedProperty);
            Assert.Equal(getter, prop.GetMethod);
            Assert.Equal(setter, prop.SetMethod);

            Assert.Equal(prop, getter.AssociatedMember);
            Assert.Equal(prop, setter.AssociatedMember);
        }

        [Theory]
        [InlineData("(EventHandler)", "INotifyPropertyChanged.PropertyChanged:EventHandler")]
        [InlineData("", "INotifyPropertyChanged.PropertyChanged")]
        public void AssociatedMember_creates_event_name_accessor_methods(string parameters, string eventName) {
            // When parameters are unspecified in an accessor, the generated event is likewise
            // unspecified.  If we have a parameter, we can infer the event type
            var addon = MethodName.Parse("INotifyPropertyChanged.add_PropertyChanged" + parameters);
            var remover = MethodName.Parse("INotifyPropertyChanged.remove_PropertyChanged" + parameters);
            var raiser = MethodName.Parse("INotifyPropertyChanged.raise_PropertyChanged" + parameters);
            var evt = EventName.Parse(eventName);

            Assert.Equal(addon, evt.AddMethod);
            Assert.Equal(remover, evt.RemoveMethod);
            Assert.Equal(raiser, evt.RaiseMethod);
        }

        [Theory]
        [InlineData("INotifyPropertyChanged.add_PropertyChanged(EventHandler)")]
        [InlineData("INotifyPropertyChanged.remove_PropertyChanged(EventHandler)")]
        [InlineData("INotifyPropertyChanged.raise_PropertyChanged(EventHandler)")]
        public void AssociatedMember_creates_event_name_semantics(string methodName) {
            // We know the event's type based on the signature of the associated accessor
            var accessor = MethodName.Parse(methodName);
            var evt = EventName.Parse("INotifyPropertyChanged.PropertyChanged:EventHandler");

            Assert.Equal(evt, accessor.AssociatedEvent);
            Assert.Equal(evt, accessor.AssociatedMember);
        }

        [Theory]
        [InlineData("INotifyPropertyChanged.add_PropertyChanged")]
        [InlineData("INotifyPropertyChanged.remove_PropertyChanged")]
        [InlineData("INotifyPropertyChanged.raise_PropertyChanged")]
        public void AssociatedMember_creates_event_name_unspecified_parameters(string methodName) {
            var accessor = MethodName.Parse(methodName);
            var evt = EventName.Parse("INotifyPropertyChanged.PropertyChanged");

            Assert.Equal(evt, accessor.AssociatedEvent);
            Assert.Equal(evt, accessor.AssociatedMember);
        }

        // TODO Test has mangle and args:
        // Compare`2<TInput, TOutput> ==> invalid

        [Fact]
        public void WithName_should_rename_methods() {
            var method = MethodName.Parse("GetTotal(int, int, double)");
            Assert.Equal("GetSum(int, int, double)", method.WithName("GetSum").ToString());
        }

        [Fact]
        public void ToString_GenericInstanceType_receiver_should_contain_generic_parameters() {
            var method = typeof(Comparison<int>).GetTypeInfo().GetMethod("Invoke");
            Assert.Equal("System.Comparison<System.Int32>.Invoke(System.Int32 x, System.Int32 y)",
                    MethodName.FromMethodInfo(method).ToString());
        }

        [Theory]
        [InlineData("System.Class`2..ctor(`0,`1)")]
        [InlineData("System.Class.Invoke``2(``0,``1)")]
        public void Equals_should_apply_to_parsed_elements(string text) {
            var a = MethodName.Parse(text);
            var b = MethodName.Parse(text);
            Assert.Equal(b, a);
        }

        [Fact]
        public void Create_should_create_method_name_from_name() {
            var a = MethodName.Create("Hello");
            var b = MethodName.Parse("Hello");
            Assert.Equal(a, b);
        }
    }
}
