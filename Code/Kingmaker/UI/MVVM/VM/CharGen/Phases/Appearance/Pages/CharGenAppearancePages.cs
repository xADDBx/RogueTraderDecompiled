using System.Collections.Generic;

namespace Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;

public static class CharGenAppearancePages
{
	public static readonly List<CharGenAppearancePageType> PagesOrder = new List<CharGenAppearancePageType>
	{
		CharGenAppearancePageType.Portrait,
		CharGenAppearancePageType.General,
		CharGenAppearancePageType.Hair,
		CharGenAppearancePageType.Tattoo,
		CharGenAppearancePageType.Implants,
		CharGenAppearancePageType.NavigatorMutations,
		CharGenAppearancePageType.Voice
	};

	private static readonly Dictionary<CharGenAppearancePageType, List<CharGenAppearancePageComponent>> s_PagesContent = new Dictionary<CharGenAppearancePageType, List<CharGenAppearancePageComponent>>
	{
		{
			CharGenAppearancePageType.Portrait,
			new List<CharGenAppearancePageComponent> { CharGenAppearancePageComponent.Portraits }
		},
		{
			CharGenAppearancePageType.General,
			new List<CharGenAppearancePageComponent>
			{
				CharGenAppearancePageComponent.Gender,
				CharGenAppearancePageComponent.FaceType,
				CharGenAppearancePageComponent.BodyType,
				CharGenAppearancePageComponent.SkinColour
			}
		},
		{
			CharGenAppearancePageType.Hair,
			new List<CharGenAppearancePageComponent>
			{
				CharGenAppearancePageComponent.HairType,
				CharGenAppearancePageComponent.HairColour,
				CharGenAppearancePageComponent.EyebrowType,
				CharGenAppearancePageComponent.EyebrowColour,
				CharGenAppearancePageComponent.BeardType,
				CharGenAppearancePageComponent.BeardColour
			}
		},
		{
			CharGenAppearancePageType.Tattoo,
			new List<CharGenAppearancePageComponent>
			{
				CharGenAppearancePageComponent.ScarsType,
				CharGenAppearancePageComponent.Tattoo,
				CharGenAppearancePageComponent.TattooColor
			}
		},
		{
			CharGenAppearancePageType.Implants,
			new List<CharGenAppearancePageComponent>
			{
				CharGenAppearancePageComponent.PortType1,
				CharGenAppearancePageComponent.PortType2
			}
		},
		{
			CharGenAppearancePageType.NavigatorMutations,
			new List<CharGenAppearancePageComponent> { CharGenAppearancePageComponent.NavigatorMutations }
		},
		{
			CharGenAppearancePageType.Voice,
			new List<CharGenAppearancePageComponent> { CharGenAppearancePageComponent.VoiceType }
		},
		{
			CharGenAppearancePageType.Servoskull,
			new List<CharGenAppearancePageComponent> { CharGenAppearancePageComponent.ServoSkullType }
		}
	};

	public static List<CharGenAppearancePageComponent> GetComponentsList(CharGenAppearancePageType page)
	{
		return s_PagesContent[page];
	}
}
