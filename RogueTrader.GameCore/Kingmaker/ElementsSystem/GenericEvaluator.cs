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
		return GetValueInternal() ?? throw new FailToEvaluateException(this);
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
