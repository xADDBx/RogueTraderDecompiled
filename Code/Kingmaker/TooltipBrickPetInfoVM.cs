using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.UI.Tooltips;
using UniRx;
using UnityEngine.Video;

namespace Kingmaker;

public class TooltipBrickPetInfoVM : TooltipBaseBrickVM
{
	private PetKeystoneInfoComponent m_PetKeystoneInfoComponent;

	public ReactiveProperty<int> MovementPoints = new ReactiveProperty<int>();

	public ReactiveProperty<VideoClip> PetVideo = new ReactiveProperty<VideoClip>();

	public ReactiveProperty<string> PetDescription = new ReactiveProperty<string>();

	public ReactiveProperty<int> CoreAbilitiesCount = new ReactiveProperty<int>();

	public List<TooltipBrickIconAndNameVM> CoreAbilities = new List<TooltipBrickIconAndNameVM>();

	public List<TooltipBrickShortLabelVM> KeyStats = new List<TooltipBrickShortLabelVM>();

	public CharInfoAbilityScoresBlockVM AbilityScores;

	public TooltipBrickPetInfoVM(PetKeystoneInfoComponent petKeystoneInfoComponent, BaseUnitEntity pet)
	{
		m_PetKeystoneInfoComponent = petKeystoneInfoComponent;
		PetVideo.Value = m_PetKeystoneInfoComponent.VideoToShow.Load();
		PetDescription.Value = m_PetKeystoneInfoComponent.DescriptionReference.Get().Description;
		CoreAbilitiesCount.Value = m_PetKeystoneInfoComponent.CoreAbilitiesReferences.Count;
		MovementPoints.Value = pet.CombatState.WarhammerInitialAPBlue.BaseValue;
		foreach (BlueprintAbilityReference coreAbilitiesReference in m_PetKeystoneInfoComponent.CoreAbilitiesReferences)
		{
			BlueprintAbility blueprintAbility = coreAbilitiesReference.Get();
			TooltipBrickIconAndNameVM item = new TooltipBrickIconAndNameVM(blueprintAbility.Icon, blueprintAbility.Name, TooltipBrickElementType.Medium, frame: true, new TooltipTemplateAbility(blueprintAbility));
			CoreAbilities.Add(item);
		}
		if (m_PetKeystoneInfoComponent.KeyStats != null)
		{
			foreach (PetKeyStat keyStat in m_PetKeystoneInfoComponent.KeyStats)
			{
				TooltipBrickShortLabelVM item2 = new TooltipBrickShortLabelVM(UIUtilityTexts.GetStatShortName(keyStat.StatType));
				KeyStats.Add(item2);
			}
		}
		AbilityScores = new CharInfoAbilityScoresBlockVM(new ReactiveProperty<BaseUnitEntity>(pet));
	}
}
