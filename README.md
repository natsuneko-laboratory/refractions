# Refractions

The .NET reflection library for .NET Standard 2.0 (for Unity backward compatibles).

## Examples

```csharp
// 1st: create refraction instance
var refraction = Refractions.FromType<TestClass>();

// 2nd: create mock interface
public interface ITestClass
{
  void StaticWithReturnsVoid();

  string StaticWithReturnsString();

  string StaticWithArgsAndReturnsString(string msg);

  void InstanceWithReturnsVoid();

  string InstanceWithReturnsString();

  string InstanceWithArgsAndReturnsString(string msg);

  // return self instance
  ITestClass Test();

  // interface could not declare field, set FieldAttribute
  [Field]
  string field { get; set; }

  string Property { get; set; }
}

// 3rd: create instance with target class
var t = refraction.GetLaxity<ITestClass>("TestClass");
var t = refraction.Get<ITestClass>("Namespace.TestClass");

// 4th: set member find blags
var @static = t.Static();
var instance = t.Instance(obj);
var accessible = instance.Public();
var inaccessible = instance.NonPublic();

// 5th: invoke methods
accessible.ProxyInvoke(w => w.StaticWithReturnsVoid());
accessible.ProxyInvoke(w => w.StaticWithReturnsString()).Dump();
accessible.ProxyInvoke(w => w.StaticWithArgsAndReturnsString(arg)).Dump();
accessible.ProxyInvoke(w => w.StaticWithArgsAndReturnsString("inline")).Dump();
accessible.ProxyInvoke(w => w.InstanceWithReturnsVoid());
accessible.ProxyInvoke(w => w.InstanceWithReturnsString()).Dump();
accessible.ProxyInvoke(w => w.InstanceWithArgsAndReturnsString(arg)).Dump();
accessible.ProxyInvoke(w => w.InstanceWithArgsAndReturnsString("inline")).Dump();

// 6th: property / field access
accessible.ProxySet(w => w.Field, "SetValue");
accessible.ProxyGet(w => w.Field).Dump();
accessible.ProxySet(w => w.Property, "Minecraft");
accessible.ProxyGet(w => w.Property).Dump();
```

## License

MIT by [@6jz](https://twitter.com/6jz)
