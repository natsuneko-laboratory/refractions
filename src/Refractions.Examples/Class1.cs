// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using Refractions.Attributes;

namespace Refractions.Examples;

public class Class1
{
    private static void Run()
    {
        // same
        var a = Refractions.FromFullyQualifiedAssemblyName("NatsunekoLaboratory.Refractions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");
        var b = Refractions.FromType(typeof(Refractions));
        var c = Refractions.FromAssembly(typeof(Refractions).Assembly);

        // generics version
        var d = Refractions.FromType<Refractions>();

        // same
        var h = a.Get<IMockRefractions>("NatsunekoLaboratory.Refractions.Refractions");
        var i = b.GetLaxity<IMockRefractions>("Refractions");
        var j = c.GetStrictly<IMockRefractions>("NatsunekoLaboratory.Refractions.Refractions, NatsunekoLaboratory.Refractions, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

        var name = "Unity";

        // methods
        h.ProxyInvoke(w => w.TestMethod());
        i.ProxyInvoke(w => w.TestMethodWithArg(name));
        var z = j.ProxyInvoke(w => w.TestMethodWithReturn(name));

        // properties or fields
        h.ProxyGet(w => w.Property);
        i.ProxySet(w => w.Property, name);

        h.ProxyGet(w => w.Field);
        i.ProxySet(w => w.Field, name);

        // explicit invoke public methods
        h.Public().ProxyInvoke(w => w.TestMethod());

        // explicit invoke non-public methods
        h.NonPublic().ProxyInvoke(w => w.TestMethod());

        // explicit invoke instance methods
        h.Instance(null).ProxyInvoke(w => w.TestMethod());

        // explicit invoke static methods
        h.Static().ProxyInvoke(w => w.TestMethod());

        // combination
        h.Public().Instance(null).ProxyInvoke(w => w.TestMethod());
    }
}

public interface IMockRefractions
{
    string Property { get; set; }

    [Field]
    string Field { get; set; }

    void TestMethod();

    void TestMethodWithArg(string name);

    string TestMethodWithReturn(string name);
}