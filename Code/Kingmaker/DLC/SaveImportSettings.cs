using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.DLC;

[Serializable]
public class SaveImportSettings
{
	public ConditionsChecker Condition;

	[SerializeField]
	private BlueprintCampaignReference m_Campaign;

	[SerializeField]
	private bool m_NewGameOnly;

	[SerializeField]
	private BlueprintUnitReference m_PlayerBlueprint;

	public bool ImportAsCompanion;

	[SerializeField]
	[FormerlySerializedAs("Flags")]
	private BlueprintUnlockableFlagReference[] m_Flags;

	[SerializeField]
	private BlueprintSummonPoolReference m_PlayerSummonPool;

	public ActionList OnImport;

	[SerializeField]
	private BlueprintDlcRewardReference m_DlcReward;

	public bool CanImport;

	public BlueprintCampaign Campaign => m_Campaign;

	public bool NewGameOnly => m_NewGameOnly;

	public BlueprintUnit PlayerBlueprint => m_PlayerBlueprint?.Get();

	public ReferenceArrayProxy<BlueprintUnlockableFlag> Flags
	{
		get
		{
			BlueprintReference<BlueprintUnlockableFlag>[] flags = m_Flags;
			return flags;
		}
	}

	public BlueprintSummonPool PlayerSummonPool => m_PlayerSummonPool?.Get();

	public BlueprintDlcReward DlcReward => m_DlcReward;

	public void DoImport(SaveInfo save)
	{
		Game.Instance.Player.UsedDlcRewards.Add(DlcReward);
		BaseUnitEntity baseUnitEntity = DlcSaveImporter.ImportPlayerState(save, PlayerBlueprint, ImportAsCompanion, Flags.ToArray());
		if (baseUnitEntity != null)
		{
			if (PlayerSummonPool != null)
			{
				Game.Instance.SummonPools.Register(PlayerSummonPool, baseUnitEntity);
			}
		}
		else
		{
			PFLog.Default.Error($"Unable to import player from save {save}.");
		}
		if (baseUnitEntity != null && OnImport.HasActions)
		{
			SceneEntitiesState state = (ImportAsCompanion ? Game.Instance.Player.CrossSceneState : Game.Instance.State.LoadedAreaState.MainState);
			using (ContextData<SpawnedUnitData>.Request().Setup(baseUnitEntity, state))
			{
				OnImport.Run();
			}
		}
	}

	public void CancelImport()
	{
		Game.Instance.Player.UsedDlcRewards.Add(DlcReward);
	}
}
