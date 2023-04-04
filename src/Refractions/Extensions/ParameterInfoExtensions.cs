// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Reflection;

namespace Refractions.Extensions;

internal static class ParameterInfoExtensions
{
    public static bool HasCustomAttribute<T>(this ParameterInfo element) where T : Attribute
    {
        return element.GetCustomAttribute<T>() != null;
    }
}