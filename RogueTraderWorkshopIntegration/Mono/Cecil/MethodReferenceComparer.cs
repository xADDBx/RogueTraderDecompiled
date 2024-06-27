using System;
using System.Collections.Generic;

namespace Mono.Cecil;

internal sealed class MethodReferenceComparer : EqualityComparer<MethodReference>
{
	[ThreadStatic]
	private static List<MethodReference> xComparisonStack;

	[ThreadStatic]
	private static List<MethodReference> yComparisonStack;

	public override bool Equals(MethodReference x, MethodReference y)
	{
		return AreEqual(x, y);
	}

	public override int GetHashCode(MethodReference obj)
	{
		return GetHashCodeFor(obj);
	}

	public static bool AreEqual(MethodReference x, MethodReference y)
	{
		if (x == y)
		{
			return true;
		}
		if (x.HasThis != y.HasThis)
		{
			return false;
		}
		if (x.HasParameters != y.HasParameters)
		{
			return false;
		}
		if (x.HasGenericParameters != y.HasGenericParameters)
		{
			return false;
		}
		if (x.Parameters.Count != y.Parameters.Count)
		{
			return false;
		}
		if (x.Name != y.Name)
		{
			return false;
		}
		if (!TypeReferenceEqualityComparer.AreEqual(x.DeclaringType, y.DeclaringType))
		{
			return false;
		}
		GenericInstanceMethod genericInstanceMethod = x as GenericInstanceMethod;
		GenericInstanceMethod genericInstanceMethod2 = y as GenericInstanceMethod;
		if (genericInstanceMethod != null || genericInstanceMethod2 != null)
		{
			if (genericInstanceMethod == null || genericInstanceMethod2 == null)
			{
				return false;
			}
			if (genericInstanceMethod.GenericArguments.Count != genericInstanceMethod2.GenericArguments.Count)
			{
				return false;
			}
			for (int i = 0; i < genericInstanceMethod.GenericArguments.Count; i++)
			{
				if (!TypeReferenceEqualityComparer.AreEqual(genericInstanceMethod.GenericArguments[i], genericInstanceMethod2.GenericArguments[i]))
				{
					return false;
				}
			}
		}
		MethodDefinition methodDefinition = x.Resolve();
		MethodDefinition methodDefinition2 = y.Resolve();
		if (methodDefinition != methodDefinition2)
		{
			return false;
		}
		if (methodDefinition == null)
		{
			if (xComparisonStack == null)
			{
				xComparisonStack = new List<MethodReference>();
			}
			if (yComparisonStack == null)
			{
				yComparisonStack = new List<MethodReference>();
			}
			for (int j = 0; j < xComparisonStack.Count; j++)
			{
				if (xComparisonStack[j] == x && yComparisonStack[j] == y)
				{
					return true;
				}
			}
			xComparisonStack.Add(x);
			try
			{
				yComparisonStack.Add(y);
				try
				{
					for (int k = 0; k < x.Parameters.Count; k++)
					{
						if (!TypeReferenceEqualityComparer.AreEqual(x.Parameters[k].ParameterType, y.Parameters[k].ParameterType))
						{
							return false;
						}
					}
				}
				finally
				{
					yComparisonStack.RemoveAt(yComparisonStack.Count - 1);
				}
			}
			finally
			{
				xComparisonStack.RemoveAt(xComparisonStack.Count - 1);
			}
		}
		return true;
	}

	public static bool AreSignaturesEqual(MethodReference x, MethodReference y, TypeComparisonMode comparisonMode = TypeComparisonMode.Exact)
	{
		if (x.HasThis != y.HasThis)
		{
			return false;
		}
		if (x.Parameters.Count != y.Parameters.Count)
		{
			return false;
		}
		if (x.GenericParameters.Count != y.GenericParameters.Count)
		{
			return false;
		}
		for (int i = 0; i < x.Parameters.Count; i++)
		{
			if (!TypeReferenceEqualityComparer.AreEqual(x.Parameters[i].ParameterType, y.Parameters[i].ParameterType, comparisonMode))
			{
				return false;
			}
		}
		if (!TypeReferenceEqualityComparer.AreEqual(x.ReturnType, y.ReturnType, comparisonMode))
		{
			return false;
		}
		return true;
	}

	public static int GetHashCodeFor(MethodReference obj)
	{
		if (obj is GenericInstanceMethod genericInstanceMethod)
		{
			int num = GetHashCodeFor(genericInstanceMethod.ElementMethod);
			for (int i = 0; i < genericInstanceMethod.GenericArguments.Count; i++)
			{
				num = num * 486187739 + TypeReferenceEqualityComparer.GetHashCodeFor(genericInstanceMethod.GenericArguments[i]);
			}
			return num;
		}
		return TypeReferenceEqualityComparer.GetHashCodeFor(obj.DeclaringType) * 486187739 + obj.Name.GetHashCode();
	}
}
