using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Quests;
using Kingmaker.UI.Models.Tooltip.Base;
using UnityEngine;

namespace Kingmaker.Code.Blueprints.Quests;

[TypeId("10f09e1ac5154b57a19ff614406abb9c")]
public class BlueprintQuestContract : BlueprintQuest, IUIDataProvider
{
	public string Name => Title;

	public new string Description => base.Description;

	public Sprite Icon => null;

	public string NameForAcronym => null;
}
