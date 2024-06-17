using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Dialog;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.DialogSystem.Blueprints;

[TypeId("52ca6275d0a87bf428811a8c24d89de4")]
public class BlueprintAnswersList : BlueprintAnswerBase
{
	public bool ShowOnce;

	[NotNull]
	public ConditionsChecker Conditions = new ConditionsChecker();

	[NotNull]
	public List<BlueprintAnswerBaseReference> Answers = new List<BlueprintAnswerBaseReference>();

	public bool CanSelect()
	{
		if (ShowOnce && Game.Instance.Player.Dialog.ShownAnswerLists.Contains(this))
		{
			DialogDebug.Add(this, "(show once) was selected before", Color.red);
			return false;
		}
		if (!Conditions.Check(this))
		{
			DialogDebug.Add(this, "conditions failed", Color.red);
			return false;
		}
		DialogDebug.Add(this, "answer list selected", Color.green);
		return true;
	}
}
