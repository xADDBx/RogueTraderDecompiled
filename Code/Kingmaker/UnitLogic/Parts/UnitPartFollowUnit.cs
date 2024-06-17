using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Visual.Critters;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartFollowUnit : EntityPart<AbstractUnitEntity>, IHashable
{
	private bool m_Initialized;

	private EntityRef<BaseUnitEntity> m_LeaderRef;

	public bool AlwaysRun { get; private set; }

	public bool CanBeSlowerThanLeader { get; private set; }

	public bool FollowWhileCutscene { get; private set; }

	public bool FollowInCombat { get; private set; }

	public BaseUnitEntity Leader => m_LeaderRef;

	public void Init(BaseUnitEntity leader, FollowerSettings settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene, settings.FollowInCombat);
	}

	public void Init(BaseUnitEntity leader, EtudeBracketFollowUnit settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene);
	}

	public void Init(BaseUnitEntity leader, MakeUnitFollowUnit settings)
	{
		Init(leader, settings.AlwaysRun, settings.CanBeSlowerThanLeader, settings.FollowWhileCutscene);
	}

	private void Init(BaseUnitEntity leader, bool alwaysRun, bool canBeSlowerThanLeader, bool followWhileCutscene, bool followInCombat = true)
	{
		m_LeaderRef = leader;
		AlwaysRun = alwaysRun;
		CanBeSlowerThanLeader = canBeSlowerThanLeader;
		FollowWhileCutscene = followWhileCutscene;
		FollowInCombat = followInCombat;
		if (Leader == null)
		{
			PFLog.Default.Error("UnitPartFollowUnit.Init: Leader is null");
		}
		else
		{
			OnAttachOrPostLoad();
		}
	}

	protected override void OnAttachOrPostLoad()
	{
		if (Leader != null && !m_Initialized)
		{
			base.Owner.Sleepless.Retain();
			Leader.GetOrCreate<UnitPartFollowedByUnits>().AddFollower(base.Owner);
			m_Initialized = true;
		}
	}

	protected override void OnDetach()
	{
		if (m_Initialized)
		{
			base.Owner.Sleepless.Release();
			Leader.GetOrCreate<UnitPartFollowedByUnits>().RemoveFollower(base.Owner);
			m_Initialized = false;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
