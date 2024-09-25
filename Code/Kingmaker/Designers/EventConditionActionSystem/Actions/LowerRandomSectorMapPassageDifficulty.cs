using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("461df2474ffe423e928241c6645b57db")]
public class LowerRandomSectorMapPassageDifficulty : GameAction
{
	public BlueprintSectorMapPointReference SectorMapPoint;

	public int RequiredNavigatorResource;

	[CanBeNull]
	public BlueprintItemReference RequiredItem;

	public int Quantity;

	public override string GetCaption()
	{
		return $"Lower difficulty of passage from {SectorMapPoint} to random system";
	}

	protected override void RunAction()
	{
		if (RequiredNavigatorResource > Game.Instance.Player.WarpTravelState.NavigatorResource)
		{
			return;
		}
		SectorMapObjectEntity sectorMapObjectEntity = Game.Instance.State.SectorMapObjects.FirstOrDefault((SectorMapObjectEntity entity) => entity.Blueprint == SectorMapPoint.Get());
		if (sectorMapObjectEntity == null)
		{
			return;
		}
		IEnumerable<SectorMapPassageEntity> enumerable = from pass in Game.Instance.SectorMapController.AllPassagesForSystem(sectorMapObjectEntity)
			where pass.CurrentDifficulty > SectorMapPassageEntity.PassageDifficulty.Safe
			select pass;
		if (enumerable.Any())
		{
			PersistentRandom.Generator generator = PersistentRandom.Seed(Game.Instance.Player.GameId.GetHashCode()).MakeGenerator("random_passage_from_" + SectorMapPoint.NameSafe());
			enumerable.Random(PFStatefulRandom.Designers, ((PersistentRandom.Generator)generator).NextRange)?.LowerDifficulty(SectorMapPassageEntity.PassageDifficulty.Safe);
			Game.Instance.SectorMapController.ChangeNavigatorResourceCount(-RequiredNavigatorResource);
			if (RequiredItem != null && Game.Instance.Player.Inventory.Contains(RequiredItem, Quantity))
			{
				Game.Instance.Player.Inventory.Remove(RequiredItem.Get(), Quantity);
			}
		}
	}
}
