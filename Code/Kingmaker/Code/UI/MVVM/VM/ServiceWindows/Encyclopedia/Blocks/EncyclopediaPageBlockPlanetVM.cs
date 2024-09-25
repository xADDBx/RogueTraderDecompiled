using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Encyclopedia.Blocks;

public class EncyclopediaPageBlockPlanetVM : EncyclopediaPageBlockVM
{
	public readonly ReactiveProperty<string> Title = new ReactiveProperty<string>();

	public readonly ReactiveProperty<Sprite> PlanetIcon = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<bool> AdminKnowAboutIt = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> SystemName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<int> Security = new ReactiveProperty<int>();

	public readonly ReactiveProperty<bool> HaveColony = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HaveQuest = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> HaveRumour = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<string> QuestObjectiveName = new ReactiveProperty<string>();

	public readonly ReactiveProperty<string> RumourObjectiveName = new ReactiveProperty<string>();

	public new readonly float FontMultiplier = FontSizeMultiplier;

	private static float FontSizeMultiplier => SettingsRoot.Accessiability.FontSizeMultiplier;

	public EncyclopediaPageBlockPlanetVM(BlueprintEncyclopediaPlanetTypePage.PlanetBlock block)
		: base(block)
	{
		Title.Value = (string.IsNullOrWhiteSpace(block.Planet.Name) ? "Empty Name" : block.Planet.Name);
		PlanetIcon.Value = UIConfig.Instance.UIIcons.GetPlanetIcon(block.Planet.Type);
		AdminKnowAboutIt.Value = block.IsReportedToAdministratum;
		SystemName.Value = block.StarSystem.Name;
		List<QuestObjective> questsForPlanet = UIUtilitySpaceQuests.GetQuestsForPlanet(block.Planet);
		HaveQuest.Value = !questsForPlanet.Empty() && questsForPlanet != null;
		if (HaveQuest.Value)
		{
			List<string> list = questsForPlanet?.Where((QuestObjective quest) => !string.IsNullOrWhiteSpace(quest.Blueprint.GetTitile())).Select((QuestObjective quest, int index) => $"{index + 1}. " + quest.Blueprint.GetTitile()).ToList();
			if (list != null && list.Any())
			{
				QuestObjectiveName.Value = string.Join(Environment.NewLine, list);
			}
		}
		List<QuestObjective> rumoursForPlanet = UIUtilitySpaceQuests.GetRumoursForPlanet(block.Planet);
		HaveRumour.Value = !rumoursForPlanet.Empty() && rumoursForPlanet != null;
		if (HaveRumour.Value)
		{
			List<string> list2 = rumoursForPlanet?.Where((QuestObjective rumour) => !string.IsNullOrWhiteSpace(rumour.Blueprint.GetTitile())).Select((QuestObjective rumour, int index) => $"{index + 1}. " + rumour.Blueprint.GetTitile()).ToList();
			if (list2 != null && list2.Any())
			{
				RumourObjectiveName.Value = string.Join(Environment.NewLine, list2);
			}
		}
		if (block.Colony != null)
		{
			HaveColony.Value = true;
			Security.Value = block.Colony.Security.Value;
		}
		else
		{
			HaveColony.Value = false;
		}
	}

	protected override void DisposeImplementation()
	{
	}
}
