using JetBrains.Annotations;

namespace Kingmaker.ElementsSystem.Interfaces;

public interface IEvaluator<T>
{
	[NotNull]
	T GetValue();

	bool CanEvaluate();

	bool TryGetValue([CanBeNull] out T value);
}
