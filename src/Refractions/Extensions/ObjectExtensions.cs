﻿// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

namespace Refractions.Extensions;

public static class ObjectExtensions
{
    public static RefractionResolver ToRefract(this object obj)
    {
        return RefractionResolver.FromType(obj.GetType());
    }

    public static Refraction<T> ToInstantiate<T>(this object obj, Refraction<T> refraction)
    {
        return refraction.Instance(obj);
    }
}