#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using StronglyTyped.Newtonsoft.Json.Serialization;

#if !HAVE_LINQ
using StronglyTyped.Newtonsoft.Json.Utilities.LinqBridge;
#endif

namespace StronglyTyped.Newtonsoft.Json.Utilities
{
    internal abstract class ReflectionDelegateFactory
    {
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public Func<T, object?> CreateGet<T>(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                // https://github.com/dotnet/corefx/issues/26053
                if (propertyInfo.PropertyType.IsByRef)
                {
                    throw new InvalidOperationException("Could not create getter for {0}. ByRef return values are not supported.".FormatWith(CultureInfo.InvariantCulture, propertyInfo));
                }

                return CreateGet<T>(propertyInfo);
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                return CreateGet<T>(fieldInfo);
            }

            throw new Exception("Could not create getter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
        }

        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public Action<T, object?> CreateSet<T>(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                return CreateSet<T>(propertyInfo);
            }

            if (memberInfo is FieldInfo fieldInfo)
            {
                return CreateSet<T>(fieldInfo);
            }

            throw new Exception("Could not create setter for {0}.".FormatWith(CultureInfo.InvariantCulture, memberInfo));
        }

        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract MethodCall<T, object?> CreateMethodCall<T>(MethodBase method);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract ObjectConstructor<object> CreateParameterizedConstructor(MethodBase method);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract Func<T> CreateDefaultConstructor<T>(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors | DynamicallyAccessedMemberTypes.NonPublicConstructors)]
            Type type);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract Func<T, object?> CreateGet<T>(PropertyInfo propertyInfo);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract Func<T, object?> CreateGet<T>(FieldInfo fieldInfo);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract Action<T, object?> CreateSet<T>(FieldInfo fieldInfo);
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        public abstract Action<T, object?> CreateSet<T>(PropertyInfo propertyInfo);
    }
}