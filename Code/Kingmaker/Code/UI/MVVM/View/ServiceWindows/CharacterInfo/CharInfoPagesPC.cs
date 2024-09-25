using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo;
using Owlcat.Runtime.UI.MVVM;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo;

public class CharInfoPagesPC : BaseDisposable
{
	public List<CharInfoPageType> PagesOrder => new List<CharInfoPageType>
	{
		CharInfoPageType.Summary,
		CharInfoPageType.Features,
		CharInfoPageType.LevelProgression,
		CharInfoPageType.FactionsReputation,
		CharInfoPageType.Biography
	};

	private Dictionary<CharInfoPageType, CharInfoPage> m_PagesContent => new Dictionary<CharInfoPageType, CharInfoPage>
	{
		{
			CharInfoPageType.Summary,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameAndPortrait,
					CharInfoComponentType.LevelClassScores,
					CharInfoComponentType.SkillsAndWeapons,
					CharInfoComponentType.Summary
				},
				ComponentsForMainCharacter = new List<CharInfoComponentType>(),
				ComponentsForCompanions = new List<CharInfoComponentType>(),
				ComponentsForPets = new List<CharInfoComponentType>()
			}
		},
		{
			CharInfoPageType.Features,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameAndPortrait,
					CharInfoComponentType.LevelClassScores,
					CharInfoComponentType.SkillsAndWeapons,
					CharInfoComponentType.Abilities
				}
			}
		},
		{
			CharInfoPageType.PsykerPowers,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameAndPortrait,
					CharInfoComponentType.LevelClassScores,
					CharInfoComponentType.AttackMain,
					CharInfoComponentType.DefenceMain,
					CharInfoComponentType.Martial,
					CharInfoComponentType.AttackMartial
				}
			}
		},
		{
			CharInfoPageType.LevelProgression,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameAndPortrait,
					CharInfoComponentType.LevelClassScores,
					CharInfoComponentType.SkillsAndWeapons,
					CharInfoComponentType.Progression
				}
			}
		},
		{
			CharInfoPageType.Biography,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameFullPortrait,
					CharInfoComponentType.AlignmentWheel
				},
				ComponentsForMainCharacter = new List<CharInfoComponentType> { CharInfoComponentType.AlignmentHistory },
				ComponentsForCompanions = new List<CharInfoComponentType> { CharInfoComponentType.BiographyStories },
				ComponentsForPets = new List<CharInfoComponentType> { CharInfoComponentType.BiographyStories }
			}
		},
		{
			CharInfoPageType.FactionsReputation,
			new CharInfoPage
			{
				ComponentsForAll = new List<CharInfoComponentType>
				{
					CharInfoComponentType.NameAndPortrait,
					CharInfoComponentType.LevelClassScores,
					CharInfoComponentType.SkillsAndWeapons,
					CharInfoComponentType.FactionsReputation
				}
			}
		}
	};

	public List<CharInfoComponentType> GetComponentsList(CharInfoPageType page, UnitType unit)
	{
		return m_PagesContent[page].GetComponentsListForUnitType(unit);
	}

	protected override void DisposeImplementation()
	{
	}
}
