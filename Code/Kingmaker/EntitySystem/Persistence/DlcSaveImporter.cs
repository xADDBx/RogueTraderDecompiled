using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kingmaker.AreaLogic.QuestSystem;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.DLC;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Stores.DlcInterfaces;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.Serialization;

namespace Kingmaker.EntitySystem.Persistence;

public static class DlcSaveImporter
{
	public static List<SaveInfo> GetBestSaveForDLC(IBlueprintDlcReward dlcReward)
	{
		BlueprintDlcRewardCampaign dlcCampaign = dlcReward as BlueprintDlcRewardCampaign;
		if (dlcCampaign == null)
		{
			return null;
		}
		return Game.Instance.SaveManager.Where((SaveInfo saveInfo) => saveInfo.Campaign == dlcCampaign.Campaign && saveInfo.Type == SaveInfo.SaveType.ForImport).ToList();
	}

	public static BaseUnitEntity ImportPlayerState(SaveInfo save, BlueprintUnit playerUnitBlueprint, bool asExCompanion, BlueprintUnlockableFlag[] flags)
	{
		using (CodeTimer.New("ImportPlayerState"))
		{
			using (save)
			{
				using (save.GetReadScope())
				{
					Task<Player> task = UpgradePlayer(save);
					Task<SceneEntitiesState> task2 = UpgradeParty(save);
					Player state = task.Result;
					SceneEntitiesState result = task2.Result;
					BaseUnitEntity baseUnitEntity2;
					using (CodeTimer.New("Clone player unit"))
					{
						BaseUnitEntity baseUnitEntity = result.AllEntityData.OfType<BaseUnitEntity>().Single((BaseUnitEntity u) => u.UniqueId == state.MainCharacter.Id);
						baseUnitEntity.PostLoad();
						baseUnitEntity.Buffs.RawFacts.ForEach(delegate(Buff i)
						{
							i.Unsubscribe();
						});
						baseUnitEntity.State.Size = baseUnitEntity.OriginalSize;
						SceneEntitiesState state2 = (asExCompanion ? Game.Instance.Player.CrossSceneState : Game.Instance.State.LoadedAreaState.MainState);
						baseUnitEntity2 = SummonUnitCopy.CreateCopy(baseUnitEntity, playerUnitBlueprint, state2);
						baseUnitEntity2.Description.SetUseClassEquipment(use: true);
						baseUnitEntity2.View.UpdateClassEquipment();
						baseUnitEntity.Dispose();
						if (asExCompanion)
						{
							baseUnitEntity2.GetOrCreate<UnitPartCompanion>().SetState(CompanionState.ExCompanion);
							baseUnitEntity2.IsInGame = false;
						}
					}
					UnlockableFlagsManager unlockableFlags = state.UnlockableFlags;
					foreach (BlueprintUnlockableFlag flag in flags)
					{
						if (unlockableFlags.IsUnlocked(flag))
						{
							Game.Instance.Player.UnlockableFlags.SetFlagValue(flag, unlockableFlags.GetFlagValue(flag));
						}
						else
						{
							Game.Instance.Player.UnlockableFlags.Lock(flag);
						}
					}
					return baseUnitEntity2;
				}
			}
		}
		static async Task<SceneEntitiesState> UpgradeParty(SaveInfo save)
		{
			await EditorSafeThreading.Awaitable;
			string text;
			using (CodeTimer.New("Read party json"))
			{
				using ISaver saver = save.Saver.Clone();
				save.Saver = saver;
				using (save.GetReadScope())
				{
					text = saver.ReadJson("party");
					text = JsonUpgradeSystem.Upgrade(save, "party", text);
				}
			}
			using (CodeTimer.New("Restore party state"))
			{
				return SaveSystemJsonSerializer.Serializer.DeserializeObject<SceneEntitiesState>(text);
			}
		}
		static async Task<Player> UpgradePlayer(SaveInfo save)
		{
			await EditorSafeThreading.Awaitable;
			string text2;
			using (CodeTimer.New("Read player json"))
			{
				using ISaver saver2 = save.Saver.Clone();
				save.Saver = saver2;
				using (save.GetReadScope())
				{
					text2 = saver2.ReadJson("player");
					text2 = JsonUpgradeSystem.Upgrade(save, "player", text2);
				}
			}
			using (CodeTimer.New("Restore player state"))
			{
				return SaveSystemJsonSerializer.Serializer.DeserializeObject<Player>(text2);
			}
		}
	}

	public static List<SaveInfo> GetSavesForImport(BlueprintCampaign campaign)
	{
		if (campaign.DlcReward != null && !campaign.DlcReward.IsAvailable)
		{
			return null;
		}
		return Game.Instance.SaveManager.Where((SaveInfo saveInfo) => saveInfo.Campaign == campaign && saveInfo.Type == SaveInfo.SaveType.ForImport).ToList();
	}
}
