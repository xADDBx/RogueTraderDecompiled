using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;

namespace Code.GameCore.ElementsSystem.Actions;

[Serializable]
[TypeId("f225557a3df442c0b7f3f8ca4d6fb3b6")]
public class ThrowExceptionAction : GameAction
{
	public string Message = "test";

	public override string GetCaption()
	{
		return "Throw exception: " + Message;
	}

	protected override void RunAction()
	{
	}
}
