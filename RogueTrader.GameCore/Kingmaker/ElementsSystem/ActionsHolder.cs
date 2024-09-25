using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.ElementsSystem;

[Serializable]
[TypeId("63bb90208198450095c55997ebc6ae0a")]
public class ActionsHolder : ElementsScriptableObject
{
	[NotNull]
	public ActionList Actions = new ActionList();

	public bool HasActions => Actions.HasActions;

	public void Run()
	{
		Actions.Run();
	}
}
