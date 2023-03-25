# Refractions

The .NET reflection library for .NET Standard 2.0 (for Unity backward compatibles).

## Examples

```csharp
// 1st: create refraction instance
var refraction = Refractions.FromType<TestClass>();

// 2nd: create mock interface
public interface ITestClass
{
  // if method has `static` and `public` access modifiers
  [Public]
  [Static]
  void StaticWithReturnsVoid();

  // if method has `static` and non-`public` access modifiers
  [NonPublic]
  [Static]
  string StaticWithReturnsString();

  [NonPublic]
  [Static]
  string StaticWithArgsAndReturnsString(string msg);

  // if method has `public` access modifiers and instance method
  [Public]
  [Instance]
  void InstanceWithReturnsVoid();

  // if method has non-`public` access modifiers and instance method
  [NonPublic]
  [Instance]
  string InstanceWithReturnsString();

  [Public]
  [Instance]
  string InstanceWithArgsAndReturnsString(string msg);

  // return self instance
  [Public]
  [Instance]
  ITestClass Test();

  // interface could not declare field, set FieldAttribute
  [Public]
  [Instance]
  [Field]
  string field { get; set; }

  [Public]
  [Instance]
  string Property { get; set; }
}

// 3rd: create instance with target class
var t = refraction.GetLaxity<ITestClass>("TestClass");
var t = refraction.Get<ITestClass>("Namespace.TestClass");

var c = new TestClass();

// 4th: set member instance if needed
var accessible = t.Instance(c);

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
