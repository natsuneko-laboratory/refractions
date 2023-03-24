// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Reflection;

namespace Refractions;

public class Refractions
{
    private readonly Assembly _assembly;

    private Refractions(Assembly assembly)
    {
        _assembly = assembly;
    }

    public Refraction<T> Get<T>(string fullname) where T : class
    {
        var t = _assembly.GetType(fullname);
        return new Refraction<T>(t);
    }

    public Refraction<T> GetLaxity<T>(string name) where T : class
    {
        var t = _assembly.GetTypes().First(w => w.Name == name);
        return new Refraction<T>(t);
    }

    public Refraction<T> GetStrictly<T>(string fullyQualifiedTypeName) where T : class
    {
        var t = _assembly.GetTypes().First(w => w.AssemblyQualifiedName == fullyQualifiedTypeName);
        return new Refraction<T>(t);
    }

    #region Instance Factories

    public static Refractions FromAssembly(Assembly assembly)
    {
        return new Refractions(assembly);
    }

    public static Refractions FromType(Type t)
    {
        return new Refractions(t.Assembly);
    }

    public static Refractions FromType<T>() where T : class
    {
        return FromType(typeof(T));
    }

    private static readonly ConcurrentDictionary<string, Assembly> NameToAssemblyDictionary = new();

    public static Refractions FromTypeFullName(string fullname)
    {
        if (NameToAssemblyDictionary.TryGetValue(fullname, out var o))
            return FromAssembly(o);

        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(w => w.GetTypes());
        var assembly = types.First(w => w.FullName == fullname).Assembly;

        NameToAssemblyDictionary.TryAdd(fullname, assembly);
        return FromAssembly(assembly);
    }

    public static Refractions FromFullyQualifiedTypeName(string fullyQualifiedTypeName)
    {
        if (NameToAssemblyDictionary.TryGetValue(fullyQualifiedTypeName, out var o))
            return FromAssembly(o);

        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(w => w.GetTypes());
        var assembly = types.First(w => w.AssemblyQualifiedName == fullyQualifiedTypeName).Assembly;

        NameToAssemblyDictionary.TryAdd(fullyQualifiedTypeName, assembly);
        return FromAssembly(assembly);
    }

    public static Refractions FromFullyQualifiedAssemblyName(string fullyQualifiedAssemblyName)
    {
        if (NameToAssemblyDictionary.TryGetValue(fullyQualifiedAssemblyName, out var o))
            return FromAssembly(o);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = assemblies.First(w => w.FullName == fullyQualifiedAssemblyName);

        NameToAssemblyDictionary.TryAdd(fullyQualifiedAssemblyName, assembly);
        return FromAssembly(assembly);
    }

    #endregion
}