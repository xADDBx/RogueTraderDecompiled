using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.SectorMap.AllSystemsInformationWindow;

public class SystemInfoAllSystemsInformationWindowVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly SectorMapObjectEntity SectorMapObject;

	public readonly string SystemName;

	public readonly ReactiveProperty<Colony> Colony = new ReactiveProperty<Colony>();

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> RumourObjectiveName = new ReactiveProperty<string>();

	public readonly StringReactiveProperty ResourcesHint = new StringReactiveProperty();

	public BlueprintStarSystemMap StarSystem => SectorMapObject?.StarSystemArea;

	public SystemInfoAllSystemsInformationWindowVM(SectorMapObjectEntity sectorMapObjectEntity)
	{
		SectorMapObject = sectorMapObjectEntity;
		SystemName = sectorMapObjectEntity.Name;
	}

	protected override void DisposeImplementation()
	{
	}

	public void SetCameraOnSystem()
	{
		CameraRig.Instance.ScrollTo(SectorMapObject.Position);
	}

	public bool CheckColonization()
	{
		Colony colony = Game.Instance.ColonizationController.GetColony(SectorMapObject.View);
		if (colony != null)
		{
			Colony.Value = colony;
		}
		return colony != null;
	}

	public bool CheckQuests()
	{
		List<QuestObjective> questsForSystem = UIUtilitySpaceQuests.GetQuestsForSystem(SectorMapObject.View);
		List<QuestObjective> questsForSpaceSystem = UIUtilitySpaceQuests.GetQuestsForSpaceSystem(StarSystem);
		int num;
		if (questsForSystem == null || questsForSystem.Empty())
		{
			if (questsForSpaceSystem != null)
			{
				num = ((!questsForSpaceSystem.Empty()) ? 1 : 0);
				if (num != 0)
				{
					goto IL_003d;
				}
			}
			else
			{
				num = 0;
			}
			goto IL_0063;
		}
		num = 1;
		goto IL_003d;
		IL_0063:
		return (byte)num != 0;
		IL_003d:
		List<string> questsStringList = UIUtilitySpaceQuests.GetQuestsStringList(questsForSystem, questsForSpaceSystem);
		if (questsStringList.Any())
		{
			QuestObjectiveName.Value = string.Join(Environment.NewLine, questsStringList);
		}
		goto IL_0063;
	}

	public bool CheckRumours()
	{
		List<QuestObjective> rumoursForSystem = UIUtilitySpaceQuests.GetRumoursForSystem(SectorMapObject.View);
		if (rumoursForSystem != null && rumoursForSystem.Any())
		{
			List<string> list = rumoursForSystem.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list.Any())
			{
				RumourObjectiveName.Value = string.Join(Environment.NewLine, list);
			}
		}
		return rumoursForSystem?.Any() ?? false;
	}

	public bool CheckExtractum()
	{
		List<ColoniesState.MinerData> miners = Game.Instance.Player.ColoniesState.Miners;
		List<BlueprintPlanet> planets = (SectorMapObject.View.StarSystemBlueprint.StarSystemToTransit?.Get() as BlueprintStarSystemMap)?.Planets.EmptyIfNull().Dereference().ToList();
		if (planets == null || !planets.Any())
		{
			return false;
		}
		List<ColoniesState.MinerData> allMiners = miners.Where((ColoniesState.MinerData miner) => planets.Any((BlueprintPlanet p) => p == miner.Sso)).ToList();
		List<ResourceData> source = (from resource in planets.SelectMany((BlueprintPlanet planet) => planet.Resources)
			where !allMiners.Any((ColoniesState.MinerData miner) => miner.Resource == resource.Resource.Get())
			select resource).ToList();
		if (!source.Any() && !allMiners.Any())
		{
			return false;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (source.Any())
		{
			stringBuilder.AppendLine(UIStrings.Instance.ExplorationTexts.DiscoveredResources.Text + ":");
			IEnumerable<string> values = source.Select((ResourceData resource) => $"{resource.Resource.Get().Name}: {resource.Count}");
			stringBuilder.AppendLine(string.Join(Environment.NewLine, values));
		}
		if (allMiners.Any())
		{
			if (source.Any())
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.AppendLine(UIStrings.Instance.ExplorationTexts.ResourceMining.Text + ":");
			IEnumerable<string> values2 = allMiners.Select((ColoniesState.MinerData miner) => $"{miner.Resource.Name}: {ColoniesStateHelper.GetResourceFromMinerCountWithProductivity(miner)}");
			stringBuilder.AppendLine(string.Join(Environment.NewLine, values2));
		}
		if (string.IsNullOrWhiteSpace(stringBuilder.ToString()))
		{
			return false;
		}
		ResourcesHint.Value = stringBuilder.ToString().Trim();
		return true;
	}
}
