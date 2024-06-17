using System;
using Owlcat.Runtime.Core.Logging;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class GameAction : Element, ICanBeLogContext
{
	public abstract void RunAction();
}
