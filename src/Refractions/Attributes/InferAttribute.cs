// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

namespace Refractions.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class InferAttribute : Attribute
{
    public string Infer { get; }

    public InferAttribute(string infer)
    {
        Infer = infer;
    }
}