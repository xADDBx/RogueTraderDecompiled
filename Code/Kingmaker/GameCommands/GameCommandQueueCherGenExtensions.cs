using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Controllers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.GameCommands.Contexts;
using Kingmaker.Networking;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM.VM.CharGen.Phases;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Components.Portrait;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Appearance.Pages;
using Kingmaker.UnitLogic.Levelup.CharGen;
using Kingmaker.UnitLogic.Levelup.Selections;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.UniRx;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.GameCommands;

public static class GameCommandQueueCherGenExtensions
{
	private static bool IsMainMenu => Game.Instance.SceneLoader.LoadedUIScene == GameScenes.MainMenu;

	public static void CharGenChangePhase([NotNull] this GameCommandQueue gameCommandQueue, CharGenPhaseType phaseType)
	{
		gameCommandQueue.CharGenCommand(new CharGenChangePhaseGameCommand(phaseType));
	}

	public static void CharGenClose([NotNull] this GameCommandQueue gameCommandQueue, bool withComplete)
	{
		gameCommandQueue.CharGenCommand(new CharGenCloseGameCommand(withComplete));
	}

	public static void CharGenSelectItem([NotNull] this GameCommandQueue gameCommandQueue, FeatureGroup featureGroup, BlueprintFeature feature)
	{
		gameCommandQueue.CharGenCommand(new CharGenSelectItemGameCommand(featureGroup, feature));
	}

	public static void CharGenTryAdvanceStat([NotNull] this GameCommandQueue gameCommandQueue, StatType statType, bool advance)
	{
		gameCommandQueue.CharGenCommand(new CharGenTryAdvanceStatGameCommand(statType, advance));
	}

	public static void CharGenSetPregen([NotNull] this GameCommandQueue gameCommandQueue, [CanBeNull] BaseUnitEntity unit)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetPregenGameCommand(unit));
	}

	public static void CharGenSelectCareerPath([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintCareerPath careerPath)
	{
		gameCommandQueue.CharGenCommand(new CharGenSelectCareerPathGameCommand(careerPath));
	}

	public static void CharGenSetShip([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintStarship blueprintStarship)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetShipGameCommand(blueprintStarship));
	}

	public static void CharGenSetShipName([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] string name)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetShipNameGameCommand(name));
	}

	public static void CharGenSetName([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] string name, bool force = false)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetNameGameCommand(name), force);
	}

	public static void CharGenSetEquipmentColor([NotNull] this GameCommandQueue gameCommandQueue, int primaryIndex, int secondaryIndex)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetEquipmentColorGameCommand(primaryIndex, secondaryIndex));
	}

	public static void CharGenChangeAppearancePage([NotNull] this GameCommandQueue gameCommandQueue, CharGenAppearancePageType pageType)
	{
		gameCommandQueue.CharGenCommand(new CharGenChangeAppearancePageGameCommand(pageType));
	}

	public static void CharGenSwitchPortraitTab([NotNull] this GameCommandQueue gameCommandQueue, CharGenPortraitTab tab)
	{
		gameCommandQueue.CharGenCommand(new CharGenSwitchPortraitTabGameCommand(tab));
	}

	public static void CharGenSetPortrait([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintPortrait blueprintPortrait)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetPortraitGameCommand(blueprintPortrait));
	}

	public static void CharGenSetGender([NotNull] this GameCommandQueue gameCommandQueue, Gender gender, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetGenderGameCommand(gender, index));
	}

	public static void CharGenSetHead([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink head, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetHeadGameCommand(head, index));
	}

	public static void CharGenSetRace([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintRaceVisualPreset racePreset, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetRaceGameCommand(racePreset, index));
	}

	public static void CharGenSetSkinColor([NotNull] this GameCommandQueue gameCommandQueue, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetSkinColorGameCommand(index));
	}

	public static void CharGenSetHair([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink hair, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetHairGameCommand(hair, index));
	}

	public static void CharGenSetHairColor([NotNull] this GameCommandQueue gameCommandQueue, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetHairColorGameCommand(index));
	}

	public static void CharGenSetEyebrows([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink eyebrows, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetEyebrowsGameCommand(eyebrows, index));
	}

	public static void CharGenSetEyebrowsColor([NotNull] this GameCommandQueue gameCommandQueue, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetEyebrowsColorGameCommand(index));
	}

	public static void CharGenSetBeard([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink beard, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetBeardGameCommand(beard, index));
	}

	public static void CharGenSetBeardColor([NotNull] this GameCommandQueue gameCommandQueue, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetBeardColorGameCommand(index));
	}

	public static void CharGenSetScar([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink scar, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetScarGameCommand(scar, index));
	}

	public static void CharGenSetTattoo([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink tattoo, int index, int tattooTabIndex)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetTattooGameCommand(tattoo, index, tattooTabIndex));
	}

	public static void CharGenSetTattooColor([NotNull] this GameCommandQueue gameCommandQueue, int rampIndex, int index)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetTattooColorGameCommand(rampIndex, index));
	}

	public static void CharGenSetPort([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] EquipmentEntityLink port, int index, int portNumber)
	{
		gameCommandQueue.CharGenCommand(new CharGenSetPortGameCommand(port, index, portNumber));
	}

	public static void CharGenChangeVoice([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] BlueprintUnitAsksList blueprint)
	{
		gameCommandQueue.CharGenCommand(new CharGenChangeVoiceGameCommand(blueprint));
	}

	private static void CharGenCommand([NotNull] this GameCommandQueue gameCommandQueue, [NotNull] GameCommand gameCommand, bool force = false)
	{
		if (IsMainMenu)
		{
			DelayedInvoker.InvokeInFrames(delegate
			{
				using (ContextData<GameCommandPlayer>.Request().Setup(gameCommand, NetPlayer.Empty))
				{
					gameCommand.Execute();
				}
			}, 1);
			return;
		}
		if ((bool)ContextData<GameCommandContext>.Current || (bool)ContextData<GameCommandContext>.Current)
		{
			if (GameCommandPlayer.IsCommandExecution(gameCommand.GetType()))
			{
				return;
			}
			if (!force)
			{
				Game.Instance.CoroutinesController.InvokeInTicks(gameCommand.Execute, 1);
				return;
			}
		}
		if (!UINetUtility.IsControlMainCharacter())
		{
			PFLog.GameCommands.Log("[" + gameCommandQueue.GetType().Name + "] Only main character can do this!");
		}
		else
		{
			gameCommandQueue.AddCommand(gameCommand);
		}
	}
}
