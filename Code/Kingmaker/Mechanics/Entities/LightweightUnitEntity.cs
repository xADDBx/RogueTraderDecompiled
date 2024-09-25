using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Mechanics.Entities;

public sealed class LightweightUnitEntity : AbstractUnitEntity, IHashable
{
	private List<FakeBuff> m_FakeBuffs = new List<FakeBuff>();

	public override PartUnitAsks Asks => GetRequired<PartUnitAsks>();

	public override PartUnitViewSettings ViewSettings => GetRequired<PartUnitViewSettings>();

	public override PartUnitState State => GetRequired<PartUnitState>();

	public override PartSavedRagdollState SavedRagdoll => null;

	public override SavedDismembermentState SavedDismemberment => null;

	public override PartHealth Health => GetRequired<PartHealth>();

	public override string Name => base.Blueprint.CharacterName;

	public override bool IsAffectedByFogOfWar => true;

	public override bool AlwaysRevealedInFogOfWar => false;

	public override bool IsExtra => true;

	public LightweightUnitEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public LightweightUnitEntity(string uniqueId, bool isInGame, BlueprintUnit blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<PartStatsContainer>();
		GetOrCreate<PartUnitCommands>();
		GetOrCreate<PartMovable>();
		GetOrCreate<PartUnitAsks>();
		GetOrCreate<PartUnitViewSettings>();
		GetOrCreate<PartLifeState>();
		GetOrCreate<PartHealth>();
		GetOrCreate<EntityBoundsPart>();
		GetOrCreate<PartUnitState>();
	}

	public void CreateView(UnitEntityView prefab, Vector3 position, Quaternion rotation)
	{
		UnitEntityView unitEntityView = UnityEngine.Object.Instantiate(prefab, position, rotation);
		GameObject gameObject = unitEntityView.gameObject;
		if (gameObject.TryGetComponent(typeof(AbstractUnitEntityView), out var component))
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		LightweightUnitEntityView lightweightUnitEntityView = gameObject.AddComponent<LightweightUnitEntityView>();
		lightweightUnitEntityView.SoftColliderPlaceholder = unitEntityView.SoftColliderPlaceholder;
		lightweightUnitEntityView.RigidbodyController = unitEntityView.RigidbodyController;
		lightweightUnitEntityView.Footprints = unitEntityView.Footprints ?? Array.Empty<GameObject>();
		UnityEngine.Object.DestroyImmediate(unitEntityView);
		AttachView(lightweightUnitEntityView);
		if (string.IsNullOrEmpty(lightweightUnitEntityView.UniqueId))
		{
			lightweightUnitEntityView.UniqueId = base.UniqueId;
		}
	}

	protected override IEntityViewBase CreateViewForData()
	{
		UnitEntityView unitEntityView = ViewSettings.Instantiate();
		if (unitEntityView == null)
		{
			return null;
		}
		GameObject gameObject = unitEntityView.gameObject;
		if (gameObject.TryGetComponent(typeof(AbstractUnitEntityView), out var component))
		{
			UnityEngine.Object.DestroyImmediate(component);
		}
		LightweightUnitEntityView lightweightUnitEntityView = gameObject.AddComponent<LightweightUnitEntityView>();
		lightweightUnitEntityView.SoftColliderPlaceholder = unitEntityView.SoftColliderPlaceholder;
		lightweightUnitEntityView.RigidbodyController = unitEntityView.RigidbodyController;
		lightweightUnitEntityView.Footprints = unitEntityView.Footprints ?? Array.Empty<GameObject>();
		UnityEngine.Object.DestroyImmediate(unitEntityView);
		return lightweightUnitEntityView;
	}

	public override void MarkExtra()
	{
	}

	public void PlayBuffFx(BlueprintBuff blueprint)
	{
		FakeBuff fakeBuff = FakeBuff.Create(this, blueprint);
		m_FakeBuffs.Add(fakeBuff);
		fakeBuff.PlayFx();
	}

	public void RemoveBuffFx(BlueprintBuff blueprint)
	{
		FakeBuff fakeBuff = m_FakeBuffs.FirstOrDefault((FakeBuff buff) => buff.Blueprint == blueprint);
		if (fakeBuff != null)
		{
			m_FakeBuffs.Remove(fakeBuff);
			fakeBuff.Clear();
		}
	}

	protected override void OnDestroy()
	{
		ClearFakeBuffs();
		base.OnDestroy();
	}

	protected override void OnDispose()
	{
		ClearFakeBuffs();
		base.OnDispose();
	}

	private void ClearFakeBuffs()
	{
		for (int i = 0; i < m_FakeBuffs.Count; i++)
		{
			m_FakeBuffs[i].Clear();
		}
		m_FakeBuffs.Clear();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
