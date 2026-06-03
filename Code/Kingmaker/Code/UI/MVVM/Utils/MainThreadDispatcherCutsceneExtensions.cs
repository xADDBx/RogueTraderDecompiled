using System;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.Utils;

public static class MainThreadDispatcherCutsceneExtensions
{
	public static IObservable<T> PauseDuringCutscene<T>(this IObservable<T> source)
	{
		return source.Where((T _) => !CutsceneUIState.IsCutsceneActive.Value);
	}
}
