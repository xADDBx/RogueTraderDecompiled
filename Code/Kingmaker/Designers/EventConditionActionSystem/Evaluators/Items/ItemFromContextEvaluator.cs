using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Items;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators.Items;

[TypeId("e15a36f4553344e0a318d6f8124b6c4a")]
public class ItemFromContextEvaluator : ItemEvaluator
{
	public override string GetCaption()
	{
		return "Item from context";
	}

	protected override ItemEntity GetValueInternal()
	{
		return ContextData<ItemEntity.ContextData>.Current?.Item;
	}
}
