using System;
using System.Linq.Expressions;

namespace UnityEx.Math;

public static class Operator<T>
{
	private static readonly ParameterExpression x = Expression.Parameter(typeof(T), "x");

	private static readonly ParameterExpression y = Expression.Parameter(typeof(T), "y");

	private static readonly ParameterExpression z = Expression.Parameter(typeof(T), "z");

	private static readonly ParameterExpression w = Expression.Parameter(typeof(T), "w");

	public static readonly Func<T, T, T> Add = Lambda(Expression.Add);

	public static readonly Func<T, T, T> Subtract = Lambda(Expression.Subtract);

	public static readonly Func<T, T, T> Multiply = Lambda(Expression.Multiply);

	public static readonly Func<T, T, T> Divide = Lambda(Expression.Divide);

	public static readonly Func<T, T> Plus = Lambda(Expression.UnaryPlus);

	public static readonly Func<T, T> Negate = Lambda(Expression.Negate);

	public static readonly Func<T, T, bool> Equal = LambdaComp(Expression.Equal);

	public static readonly Func<T, T, bool> NotEqual = LambdaComp(Expression.NotEqual);

	public static readonly Func<T, T, bool> GreaterThan = LambdaComp(Expression.GreaterThan);

	public static readonly Func<T, T, bool> GreaterThanOrEqual = LambdaComp(Expression.GreaterThanOrEqual);

	public static readonly Func<T, T, bool> LessThan = LambdaComp(Expression.LessThan);

	public static readonly Func<T, T, bool> LessThanOrEqual = LambdaComp(Expression.LessThanOrEqual);

	public static readonly Func<T, T, T, T, T> ProductSum = Expression.Lambda<Func<T, T, T, T, T>>(Expression.Add(Expression.Multiply(x, y), Expression.Multiply(z, w)), new ParameterExpression[4] { x, y, z, w }).Compile();

	public static readonly Func<T, T, T, T, T> ProductDifference = Expression.Lambda<Func<T, T, T, T, T>>(Expression.Subtract(Expression.Multiply(x, y), Expression.Multiply(z, w)), new ParameterExpression[4] { x, y, z, w }).Compile();

	public static Func<T, T, T> Lambda(Func<ParameterExpression, ParameterExpression, BinaryExpression> op)
	{
		return Expression.Lambda<Func<T, T, T>>(op(x, y), new ParameterExpression[2] { x, y }).Compile();
	}

	public static Func<T, T> Lambda(Func<ParameterExpression, UnaryExpression> op)
	{
		return Expression.Lambda<Func<T, T>>(op(x), new ParameterExpression[1] { x }).Compile();
	}

	public static Func<T, T, bool> LambdaComp(Func<ParameterExpression, ParameterExpression, BinaryExpression> op)
	{
		return Expression.Lambda<Func<T, T, bool>>(op(x, y), new ParameterExpression[2] { x, y }).Compile();
	}
}
