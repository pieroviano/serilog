// Copyright 2013-2015 Serilog Contributors
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

namespace Serilog.Capturing;

static class GetablePropertyFinder
{
    internal static IEnumerable<PropertyInfo> GetPropertiesRecursive(this Type type)
    {
        var seenNames = new HashSet<string>();

#if !NET35 && !NET40
        var currentTypeInfo = type
            .GetTypeInfo();
#else
        var currentTypeInfo = type;
#endif
        while (
#if !NET35 && !NET40
            currentTypeInfo.AsType()
#else
            currentTypeInfo
#endif
            != typeof(object))
        {
            var unseenProperties = currentTypeInfo
#if !NET35 && !NET40
                .DeclaredProperties
#else
                .GetProperties()
#endif
                .Where(p => p.CanRead &&
#if !NET35 && !NET40
                                                                                 p.GetMethod!.IsPublic && !p.GetMethod.IsStatic &&
#else
                                                                                 p.GetGetMethod()!.IsPublic && !p.GetGetMethod().IsStatic &&
#endif
                                                                                 (p.Name != "Item" || p.GetIndexParameters().Length == 0) && !seenNames.Contains(p.Name));

            foreach (var propertyInfo in unseenProperties)
            {
                seenNames.Add(propertyInfo.Name);
                yield return propertyInfo;
            }

            var baseType = currentTypeInfo.BaseType;
            if (baseType == null)
            {
                yield break;
            }

#if !NET35 && !NET40
            currentTypeInfo = baseType
                .GetTypeInfo();
#else
            currentTypeInfo = baseType;
#endif
        }
    }
}
