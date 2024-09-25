using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.Interfaces;

namespace Kingmaker.ElementsSystem;

[TypeId("bd548178e6934d7b8fcbe99370510f79")]
public abstract class GenericEvaluator<T> : Evaluator, IEvaluator<T>
{
	[CanBeNull]
	protected abstract T GetValueInternal();

	public T GetValue()
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(null, this);
		try
		{
			T valueInternal = GetValueInternal();
			if (valueInternal == null)
			{
				throw new FailToEvaluateException(this);
			}
			elementsDebugger?.SetResult((!(valueInternal is int num)) ? 1 : num);
			return valueInternal;
		}
		catch (Exception exception)
		{
			elementsDebugger?.SetException(exception);
			throw;
		}
	}

	public bool CanEvaluate()
	{
		try
		{
			return GetValueInternal() != null;
		}
		catch (FailToEvaluateException)
		{
			return false;
		}
	}

	public bool TryGetValue([CanBeNull] out T value)
	{
		try
		{
			value = GetValueInternal();
			return value != null;
		}
		catch (FailToEvaluateException)
		{
			value = default(T);
			return false;
		}
	}
}
