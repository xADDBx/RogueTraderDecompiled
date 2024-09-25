using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.Versioning.PlayerUpgraderOnlyActions;

[Serializable]
[PlayerUpgraderAllowed(true)]
[TypeId("d8629817a929412f93e0507903e9f1d8")]
public class RemoveOrReplaceItemInLoot : PlayerUpgraderOnlyAction
{
	[SerializeReference]
	[SerializeField]
	[ValidateNotNull]
	private ItemsCollectionEvaluator m_Loot;

	[SerializeField]
	[ValidateNotNull]
	private BlueprintItemReference m_RemoveItem;

	[SerializeField]
	private BlueprintItemReference m_ReplaceItem;

	[NotNull]
	public BlueprintItem RemoveItem => m_RemoveItem;

	[CanBeNull]
	public BlueprintItem ReplaceItem => m_ReplaceItem;

	public override string GetCaption()
	{
		if (ReplaceItem == null)
		{
			return $"Remove item {RemoveItem} in {m_Loot}";
		}
		return $"Replace item {RemoveItem} with {ReplaceItem} in {m_Loot}";
	}

	protected override void RunActionOverride()
	{
		ItemsCollection value = m_Loot.GetValue();
		int num = value.RemoveAll(RemoveItem);
		if (ReplaceItem != null && num > 0)
		{
			value.Add(ReplaceItem, num);
		}
	}
}
