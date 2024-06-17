using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;
using Kingmaker.UI.Models.Log.ContextFlag;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("534ebb3f68b54f32a4189a719349633d")]
public class UnstashItemsFromVirtualStash : GameAction
{
	public BlueprintItemsStashReference SourceStash;

	[SerializeReference]
	public ItemsCollectionEvaluator TargetContainer;

	public bool Silent;

	public override string GetCaption()
	{
		return "Перемещает все предметы из " + SourceStash?.NameSafe() + " в " + TargetContainer?.NameSafe();
	}

	public override void RunAction()
	{
		if (TargetContainer == null || !Game.Instance.Player.VirtualStashes.TryGetValue(SourceStash, out var value))
		{
			return;
		}
		using (ContextData<GameLogDisabled>.RequestIf(Silent))
		{
			ItemsCollection value2 = TargetContainer.GetValue();
			for (int num = value.Items.Count - 1; num >= 0; num--)
			{
				value.Transfer(value.Items[num], value2);
			}
			Game.Instance.Player.VirtualStashes.Remove(SourceStash);
		}
	}
}
