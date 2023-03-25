// ------------------------------------------------------------------------------------------
//  Copyright (c) Natsuneko. All rights reserved.
//  Licensed under the MIT License. See LICENSE in the project root for license information.
// ------------------------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

using Refractions.Attributes;
using Refractions.Extensions;

namespace Refractions;

public class Refraction<TProxy>
{
    private readonly Type _t;

    public object? InnerInstance { get; }

    internal Refraction(Type t, object? instance = null)
    {
        _t = t;
        InnerInstance = instance;
    }

    private MethodInfo FindMethod(MethodBase mi)
    {
        var name = mi.Name;
        var parameters = mi.GetParameters().Select(w => w.ParameterType).ToArray();
        var flags = BindingFlags.Default;

        if (mi.HasCustomAttribute<PublicAttribute>())
            flags |= BindingFlags.Public;
        if (mi.HasCustomAttribute<NonPublicAttribute>())
            flags |= BindingFlags.NonPublic;
        if (mi.HasCustomAttribute<InstanceAttribute>())
            flags |= BindingFlags.Instance;
        if (mi.HasCustomAttribute<StaticAttribute>())
            flags |= BindingFlags.Static;

        return _t.GetMethod(name, flags, null, parameters, null) ?? throw new MissingMemberException(_t.Name, name);
    }

    private (MemberInfo Metadata, bool IsProperty) FindMember(MemberInfo mi)
    {
        var name = mi.Name;
        var flags = BindingFlags.Default;

        if (mi.HasCustomAttribute<PublicAttribute>())
            flags |= BindingFlags.Public;
        if (mi.HasCustomAttribute<NonPublicAttribute>())
            flags |= BindingFlags.NonPublic;
        if (mi.HasCustomAttribute<InstanceAttribute>())
            flags |= BindingFlags.Instance;
        if (mi.HasCustomAttribute<StaticAttribute>())
            flags |= BindingFlags.Static;

        if (mi.GetCustomAttribute<FieldAttribute>() != null)
        {
            var field = _t.GetField(name, flags) ?? throw new MissingMemberException(_t.Name, name);
            return (field, false);
        }

        var t = (PropertyInfo)mi;
        var type = t.PropertyType;
        var types = t.GetIndexParameters().Select(w => w.ParameterType).ToArray();
        var property = _t.GetProperty(name, flags, null, type, types, null) ?? throw new MissingMemberException(_t.Name, name);
        return (property, true);
    }


    private TAction BuildProxyLambda<TAction>(MethodCallExpression expr) where TAction : Delegate
    {
        // ProxyInvoke(i => i.WriteLine(msg1, msg2)) --> (instance, args) => instance.WriteLine(args[0], args[1]);
        var method = FindMethod(expr.Method);
        var accessor = Expression.Parameter(typeof(object), nameof(InnerInstance));
        var parameters = Expression.Parameter(typeof(object[]), "parameters");
        var arguments = expr.Arguments.Select((w, i) => Expression.Convert(Expression.ArrayAccess(parameters, Expression.Constant(i)), w.Type));
        var body = Expression.Call(IsStaticBinding(expr.Method) ? null : Expression.Convert(accessor, _t), method, arguments);
        var lambda = Expression.Lambda(body, false, accessor, parameters);

        return (TAction)lambda.Compile();
    }

    private static bool IsStaticBinding(MemberInfo mi)
    {
        return mi.HasCustomAttribute<StaticAttribute>();
    }

    private static object[] GetValueOfArguments(IReadOnlyCollection<Expression> args)
    {
        return args.Count == 0 ? new object[] { } : args.Select(GetValue).ToArray();
    }

    private static object GetValue(Expression expr)
    {
        return expr switch
        {
            ConstantExpression constant => constant.Value,
            MemberExpression member => Expression.Lambda(Expression.Convert(member, typeof(object))).Compile().DynamicInvoke(),
            MethodCallExpression call => Expression.Lambda(call).Compile().DynamicInvoke(),
            UnaryExpression unary => unary.NodeType == ExpressionType.Convert ? Convert.ChangeType(GetValue(unary.Operand), Nullable.GetUnderlyingType(unary.Type) ?? unary.Type) : GetValue(unary.Operand),
            ConditionalExpression conditional => (bool)GetValue(conditional.Test) ? GetValue(conditional.IfTrue) : GetValue(conditional.IfFalse),
            ListInitExpression list => Expression.Lambda(list).Compile().DynamicInvoke(),
            NewExpression @new => Expression.Lambda(@new).Compile().DynamicInvoke(),
            _ => throw new NotSupportedException("unsupported type of expression specified in value")
        };
    }

    public void ProxyInvoke(Expression<Action<TProxy>> obj)
    {
        var call = (MethodCallExpression)obj.Body;

        if (ExpressionActionCaches<Action<object?, object[]>>.CachedActions.TryGetValue(call.Method, out var c))
        {
            c(InnerInstance, GetValueOfArguments(call.Arguments));
            return;
        }

        var func = BuildProxyLambda<Action<object?, object[]>>(call);

        ExpressionActionCaches<Action<object?, object[]>>.CachedActions.TryAdd(call.Method, func);
        func(InnerInstance, GetValueOfArguments(call.Arguments));
    }

    public TReturn ProxyInvoke<TReturn>(Expression<Func<TProxy, TReturn>> obj)
    {
        var call = (MethodCallExpression)obj.Body;

        if (ExpressionActionCaches<Func<object?, object[], TReturn>>.CachedActions.TryGetValue(call.Method, out var c))
            return c(InnerInstance, GetValueOfArguments(call.Arguments));

        var func = BuildProxyLambda<Func<object?, object[], TReturn>>(call);

        ExpressionActionCaches<Func<object?, object[], TReturn>>.CachedActions.TryAdd(call.Method, func);
        return func(InnerInstance, GetValueOfArguments(call.Arguments));
    }

    public Refraction<TProxy> ProxyInvoke(Expression<Func<TProxy, TProxy>> obj)
    {
        var call = (MethodCallExpression)obj.Body;

        if (ExpressionActionCaches<Func<object?, object[], object>>.CachedActions.TryGetValue(call.Method, out var c))
        {
            var i = c(InnerInstance, GetValueOfArguments(call.Arguments));
            return new Refraction<TProxy>(_t, i);
        }

        var func = BuildProxyLambda<Func<object?, object[], object>>(call);
        var instance = func(InnerInstance, GetValueOfArguments(call.Arguments));

        ExpressionActionCaches<Func<object?, object[], object>>.CachedActions.TryAdd(call.Method, func);
        return new Refraction<TProxy>(_t, instance);
    }

    public void ProxySet<TValue>(Expression<Func<TProxy, TValue>> obj, TValue value)
    {
        var call = (MemberExpression)obj.Body;

        if (ExpressionActionCaches<Func<object?, TValue, TValue>>.CachedActions.TryGetValue(call.Member, out var c))
        {
            c(InnerInstance, value);
            return;
        }

        var member = FindMember(call.Member);
        var parameter = Expression.Parameter(typeof(object), "_instance");
        var accessor = IsStaticBinding(call.Member) ? null : Expression.Convert(parameter, _t);
        var left = member.IsProperty ? Expression.Property(accessor, (PropertyInfo)member.Metadata) : Expression.Field(accessor, (FieldInfo)member.Metadata);
        var right = Expression.Parameter(typeof(TValue), "value");
        var body = Expression.Assign(left, right);
        var lambda = Expression.Lambda(body, parameter, right);
        var func = (Func<object?, TValue, TValue>)lambda.Compile();

        ExpressionActionCaches<Func<object?, TValue, TValue>>.CachedActions.TryAdd(call.Member, func);
        func(InnerInstance, value);
    }

    public TReturn ProxyGet<TReturn>(Expression<Func<TProxy, TReturn>> obj)
    {
        var call = (MemberExpression)obj.Body;

        if (ExpressionActionCaches<Func<object?, TReturn>>.CachedActions.TryGetValue(call.Member, out var c))
            return c(InnerInstance);

        var member = FindMember(call.Member);
        var parameter = Expression.Parameter(typeof(object), "_instance");
        var accessor = IsStaticBinding(call.Member) ? null : Expression.Convert(parameter, _t);
        var body = member.IsProperty ? Expression.Property(accessor, (PropertyInfo)member.Metadata) : Expression.Field(accessor, (FieldInfo)member.Metadata);
        var lambda = Expression.Lambda(body, parameter);
        var func = (Func<object?, TReturn>)lambda.Compile();

        ExpressionActionCaches<Func<object?, TReturn>>.CachedActions.TryAdd(call.Member, func);
        return func(InnerInstance);
    }

    public Refraction<TProxy> Instance(object? instance)
    {
        return new Refraction<TProxy>(_t, instance);
    }

    private static class ExpressionActionCaches<TValue>
    {
        public static readonly ConcurrentDictionary<MemberInfo, TValue> CachedActions = new();
    }
}