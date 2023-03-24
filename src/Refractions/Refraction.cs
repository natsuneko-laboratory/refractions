// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Linq.Expressions;
using System.Reflection;

using Refractions.Attributes;

namespace Refractions;

public class Refraction<TProxy>
{
    private readonly BindingFlags _flags;
    private readonly object? _instance;
    private readonly Type _t;

    internal Refraction(Type t, object? instance = null, BindingFlags flags = BindingFlags.Default)
    {
        _t = t;
        _instance = instance;
        _flags = flags;
    }

    private MethodInfo FindMethod(MethodBase mi)
    {
        var name = mi.Name;
        var parameters = mi.GetParameters().Select(w => w.ParameterType).ToArray();

        return _t.GetMethod(name, _flags, null, parameters, null) ?? throw new MissingMemberException();
    }

    private (MemberInfo Metadata, bool IsProperty) FindMember(MemberInfo mi)
    {
        var name = mi.Name;

        if (mi.GetCustomAttribute<FieldAttribute>() != null)
            return (_t.GetField(name, _flags), false);

        var t = (PropertyInfo)mi;
        var type = t.PropertyType;
        var types = t.GetIndexParameters().Select(w => w.ParameterType).ToArray();
        return (_t.GetProperty(name, _flags, null, type, types, null), true);
    }

    private bool IsStaticBinding()
    {
        return (_flags & BindingFlags.Static) == BindingFlags.Static;
    }

    public void ProxyInvoke(Expression<Action<TProxy>> obj)
    {
        var call = (MethodCallExpression)obj.Body;
        var method = FindMethod(call.Method);
        var parameter = Expression.Parameter(typeof(object), "_instance");
        var body = Expression.Call(IsStaticBinding() ? null : Expression.Convert(parameter, _t), method, call.Arguments);
        var lambda = Expression.Lambda(body, false, parameter);
        var func = (Action<object?>)lambda.Compile();

        func(_instance);
    }

    public TReturn ProxyInvoke<TReturn>(Expression<Func<TProxy, TReturn>> obj)
    {
        var call = (MethodCallExpression)obj.Body;
        var method = FindMethod(call.Method);
        var parameter = Expression.Parameter(typeof(object), "_instance");
        var body = Expression.Call(IsStaticBinding() ? null : Expression.Convert(parameter, _t), method, call.Arguments);
        var lambda = Expression.Lambda(body, false, parameter);
        var func = (Func<object?, TReturn>)lambda.Compile();

        return func(_instance);
    }

    public Refraction<TProxy> ProxyInvoke(Expression<Func<TProxy, TProxy>> obj)
    {
        var call = (MethodCallExpression)obj.Body;
        var method = FindMethod(call.Method);
        var parameter = Expression.Parameter(typeof(object), "_instance");
        var body = Expression.Call(IsStaticBinding() ? null : Expression.Convert(parameter, _t), method, call.Arguments);
        var lambda = Expression.Lambda(body, false, parameter);
        var func = (Func<object?, object>)lambda.Compile();
        var instance = func(_instance);

        return new Refraction<TProxy>(_t, instance, _flags);
    }

    public void ProxySet<TValue>(Expression<Func<TProxy, TValue>> obj, TValue value)
    {
        var call = (MemberExpression)obj.Body;
        var member = FindMember(call.Member);
        var parameter = Expression.Parameter(typeof(object), "_instance");

        if (member.IsProperty)
        {
            var left = Expression.Property(IsStaticBinding() ? null : Expression.Convert(parameter, _t), (PropertyInfo)member.Metadata);
            var right = Expression.Parameter(typeof(TValue), "value");
            var body = Expression.Assign(left, right);
            var lambda = Expression.Lambda(body, parameter, right);
            var func = (Func<object?, TValue, TValue>)lambda.Compile();
            func(_instance, value);
        }
        else
        {
            var left = Expression.Field(IsStaticBinding() ? null : Expression.Convert(parameter, _t), (FieldInfo)member.Metadata);
            var right = Expression.Parameter(typeof(TValue), "value");
            var body = Expression.Assign(left, right);
            var lambda = Expression.Lambda(body, parameter, right);
            var func = (Func<object?, TValue, TValue>)lambda.Compile();
            func(_instance, value);
        }
    }

    public TReturn ProxyGet<TReturn>(Expression<Func<TProxy, TReturn>> obj)
    {
        var call = (MemberExpression)obj.Body;
        var member = FindMember(call.Member);
        var parameter = Expression.Parameter(typeof(object), "_instance");

        if (member.IsProperty)
        {
            var body = Expression.Property(IsStaticBinding() ? null : Expression.Convert(parameter, _t), (PropertyInfo)member.Metadata);
            var lambda = Expression.Lambda(body, parameter);
            var func = (Func<object?, TReturn>)lambda.Compile();
            return func(_instance);
        }
        else
        {
            var body = Expression.Field(IsStaticBinding() ? null : Expression.Convert(parameter, _t), (FieldInfo)member.Metadata);
            var lambda = Expression.Lambda(body, parameter);
            var func = (Func<object?, TReturn>)lambda.Compile();
            return func(_instance);
        }
    }

    public Refraction<TProxy> Public()
    {
        return new Refraction<TProxy>(_t, _instance, _flags | BindingFlags.Public);
    }

    public Refraction<TProxy> NonPublic()
    {
        return new Refraction<TProxy>(_t, _instance, _flags | BindingFlags.NonPublic);
    }

    public Refraction<TProxy> Instance(object? instance)
    {
        return new Refraction<TProxy>(_t, instance, _flags | BindingFlags.Instance);
    }

    public Refraction<TProxy> Static()
    {
        return new Refraction<TProxy>(_t, _instance, _flags | BindingFlags.Static);
    }

    public Refraction<TProxy> DeclaredOnly()
    {
        return new Refraction<TProxy>(_t, _instance, _flags | BindingFlags.DeclaredOnly);
    }
}