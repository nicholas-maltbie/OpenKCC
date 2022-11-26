// Copyright (C) 2022 Nicholas Maltbie
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING
// BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
// CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Reflection;

namespace nickmaltbie.OpenKCC.Utils
{
    public static class UtilityExtensions
    {
        public static bool EqualOrNull(this object obj, object other)
        {
            return (obj == null && other == null) || (obj != null && other != null && obj.Equals(other));
        }

        public static object EvaluateMember(this object obj, string memberName)
        {
            MemberInfo memberInfo = obj.GetType().GetMember(memberName)[0];
            switch (memberInfo.MemberType)
            {
                case MemberTypes.Field:
                    return (memberInfo as FieldInfo).GetValue(obj);
                case MemberTypes.Property:
                    return (memberInfo as PropertyInfo).GetValue(obj);
                default:
                    throw new NotImplementedException($"Could not parse field: {memberInfo} because unexpected member type: {memberInfo.MemberType} for object {obj}");
            }
        }
    }
}
