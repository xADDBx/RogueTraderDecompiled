using System;
using Code.GameCore.ElementsSystem;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.ElementsSystem;

[Serializable]
[TypeId("e3badf65f1cb42259d6eab0521f42dbd")]
public abstract class GameAction : Element, ICanBeLogContext
{
	public void Run([CanBeNull] ElementsList list = null)
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(list, this);
		try
		{
			RunAction();
			elementsDebugger?.SetResult(1);
		}
		catch (Exception exception)
		{
			Element.LogException(exception);
			elementsDebugger?.SetException(exception);
			throw;
		}
	}

	protected abstract void RunAction();
}
