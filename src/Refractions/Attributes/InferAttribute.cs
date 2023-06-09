﻿// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Reflection;

namespace Refractions.Attributes;

[AttributeUsage(AttributeTargets.Parameter)]
public class InferAttribute : Attribute
{
    public string Infer { get; }

    public Assembly? ResolvingAssembly { get; }

    public InferAttribute(string infer, Type? assembly = null)
    {
        Infer = infer;
        ResolvingAssembly = assembly?.Assembly;
    }
}