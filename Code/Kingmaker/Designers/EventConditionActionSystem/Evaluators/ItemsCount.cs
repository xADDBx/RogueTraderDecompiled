using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;
using Owlcat.QA.Validation;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[ComponentName("Evaluators/ItemsCount")]
[AllowMultipleComponents]
[TypeId("f37fe502735e8eb47ac9393cc5b73af0")]
public class ItemsCount : IntEvaluator
{
	[ValidateNotNull]
	[SerializeField]
	[FormerlySerializedAs("Item")]
	private BlueprintItemReference m_Item;

	public BlueprintItem Item => m_Item?.Get();

	protected override int GetValueInternal()
	{
		int num = 0;
		foreach (ItemEntity item in GameHelper.GetPlayerCharacter().Inventory)
		{
			if (item.Blueprint == Item)
			{
				num += item.Count;
			}
		}
		return num;
	}

	public override string GetCaption()
	{
		return $"{Item} count";
	}
}
