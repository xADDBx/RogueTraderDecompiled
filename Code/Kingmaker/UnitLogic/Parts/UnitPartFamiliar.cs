using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.Visual.Critters;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFamiliar : EntityPart<AbstractUnitEntity>, IHashable
{
	[JsonProperty]
	private EntityRef<BaseUnitEntity> m_Leader;

	private FamiliarSettings FamiliarSettings => base.Owner.Blueprint.GetComponent<FamiliarSettingsOverride>()?.FamiliarSettings ?? Game.Instance.BlueprintRoot.FamiliarsRoot.DefaultFamiliarSettings;

	public BaseUnitEntity Leader => m_Leader.Entity;

	public bool IsLeaderVisible
	{
		get
		{
			if (Leader != null && Leader.View.IsVisible && Leader.IsViewActive)
			{
				return !Leader.IsInvisible;
			}
			return false;
		}
	}

	public bool IsVisible
	{
		get
		{
			if (IsLeaderVisible)
			{
				return !NeedHide();
			}
			return false;
		}
	}

	private bool NeedHide()
	{
		if (Leader == null)
		{
			return true;
		}
		HideFamiliarSettings hideFamiliarSettings = FamiliarSettings.HideFamiliarSettings;
		if (hideFamiliarSettings.HideInCapitalMode && Game.Instance.LoadedAreaState.Settings.CapitalPartyMode)
		{
			return true;
		}
		if (hideFamiliarSettings.HideInCombat && Leader.IsInCombat)
		{
			return true;
		}
		if (hideFamiliarSettings.HideInStealth && Leader.Stealth.Active)
		{
			return true;
		}
		if (hideFamiliarSettings.HideIfLeaderUnconscious)
		{
			return !Leader.LifeState.IsConscious;
		}
		return false;
	}

	public void Init(BaseUnitEntity leader)
	{
		m_Leader = leader;
		OnAttachOrPostLoad();
		OnViewDidAttach();
	}

	protected override void OnAttachOrPostLoad()
	{
		base.OnAttachOrPostLoad();
		if (Leader != null)
		{
			base.Owner.Features.IsUntargetable.Retain();
			base.Owner.Features.IsIgnoredByCombat.Retain();
			base.Owner.SetViewHandlingOnDisposePolicy(FamiliarSettings.ViewHandlingOnDisposePolicyType);
			base.Owner.GetOrCreate<UnitPartFollowUnit>().Init(Leader, FamiliarSettings.FollowerSettings);
		}
	}

	protected override void OnViewDidAttach()
	{
		base.OnViewDidAttach();
		if (Leader != null)
		{
			base.Owner.View.EnsureComponent<FamiliarUnit>().TeleportToLeader();
			UpdateViewVisibility();
		}
	}

	public void UpdateViewVisibility()
	{
		base.Owner.View?.SetVisible(IsVisible);
	}

	public void UpdateIsInGameState(bool isInGame)
	{
		base.Owner.IsInGame = isInGame;
	}

	protected override void OnApplyPostLoadFixes()
	{
		UnitPartFamiliarLeader unitPartFamiliarLeader = Leader?.GetFamiliarLeaderOptional();
		if (unitPartFamiliarLeader == null || !unitPartFamiliarLeader.HasEquippedFamiliar(base.Owner))
		{
			Game.Instance.EntityDestroyer.Destroy(base.Owner);
			PFLog.Default.Warning($"Invalid familiar removed: {base.Owner}");
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		EntityRef<BaseUnitEntity> obj = m_Leader;
		Hash128 val2 = StructHasher<EntityRef<BaseUnitEntity>>.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
