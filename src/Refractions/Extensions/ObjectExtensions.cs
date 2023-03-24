// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

namespace Refractions.Extensions;

public static class ObjectExtensions
{
    public static Refractions ToRefract(this object obj)
    {
        return Refractions.FromType(obj.GetType());
    }
}