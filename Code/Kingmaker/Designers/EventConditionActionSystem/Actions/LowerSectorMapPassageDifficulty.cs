using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.SectorMap;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("0ef23848acd648b3a4ea8fd6b2ae8962")]
public class LowerSectorMapPassageDifficulty : GameAction
{
	public BlueprintSectorMapPointReference PassageFrom;

	public BlueprintSectorMapPointReference PassageTo;

	public int RequiredNavigatorResource;

	[CanBeNull]
	public BlueprintItemReference RequiredItem;

	public int Quantity;

	public SectorMapPassageEntity.PassageDifficulty Difficulty;

	public override string GetCaption()
	{
		return $"Lower difficulty of passage from {PassageFrom} to {PassageTo}";
	}

	protected override void RunAction()
	{
		if (RequiredNavigatorResource > Game.Instance.Player.WarpTravelState.NavigatorResource)
		{
			return;
		}
		SectorMapObjectEntity sectorMapObjectEntity = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity entity) => entity.Blueprint == PassageFrom.Get());
		SectorMapObjectEntity sectorMapObjectEntity2 = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity entity) => entity.Blueprint == PassageTo.Get());
		if (sectorMapObjectEntity != null && sectorMapObjectEntity2 != null)
		{
			Game.Instance.SectorMapController.FindPassageBetween(sectorMapObjectEntity, sectorMapObjectEntity2)?.LowerDifficulty(Difficulty);
			Game.Instance.SectorMapController.ChangeNavigatorResourceCount(-RequiredNavigatorResource);
			if (RequiredItem != null && Game.Instance.Player.Inventory.Contains(RequiredItem, Quantity))
			{
				Game.Instance.Player.Inventory.Remove(RequiredItem.Get(), Quantity);
			}
		}
	}
}
