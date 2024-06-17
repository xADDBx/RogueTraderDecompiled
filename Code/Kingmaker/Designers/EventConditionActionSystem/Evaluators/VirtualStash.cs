using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Items;

namespace Kingmaker.Designers.EventConditionActionSystem.Evaluators;

[TypeId("a2669f6dc5d84c6fa081f66d69c06056")]
public class VirtualStash : ItemsCollectionEvaluator
{
	public BlueprintItemsStashReference StashReference;

	public override string GetDescription()
	{
		return "Возвращает предметы из виртуального хранилища " + StashReference?.NameSafe();
	}

	public override string GetCaption()
	{
		return "VirtualStash " + StashReference?.NameSafe();
	}

	protected override ItemsCollection GetValueInternal()
	{
		if (Game.Instance.Player.VirtualStashes.TryGetValue(StashReference, out var value))
		{
			return value;
		}
		Game.Instance.Player.VirtualStashes[StashReference] = new ItemsCollection(Game.Instance.Player);
		return Game.Instance.Player.VirtualStashes[StashReference];
	}
}
