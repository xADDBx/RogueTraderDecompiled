using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.GuidUtility;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.SpaceCombat.MeteorStream;

[Serializable]
[TypeId("0ff6a26b0d9044b9b7b5b8ebe4780e4b")]
public class MeteorStream : EntityFactComponentDelegate, IHashable
{
	[ValidateNotNull]
	[SerializeField]
	private BlueprintMeteorStreamReference m_MeteorStream;

	[ValidateNotNull]
	[SerializeField]
	private List<BlueprintMeteorReference> m_Meteor;

	private MeteorStreamEntity m_StreamEntity;

	[SerializeField]
	public BlueprintMeteorStream MeteorStreamBlueprint => m_MeteorStream.Get();

	public List<BlueprintMeteor> MeteorBlueprint => m_Meteor.Select((BlueprintMeteorReference x) => x.Get()).ToList();

	protected override void OnInitialize()
	{
		m_StreamEntity = Entity.Initialize(new MeteorStreamEntity(Uuid.Instance.CreateString(), isInGame: true, MeteorStreamBlueprint));
	}

	protected override void OnActivate()
	{
		m_StreamEntity.CreateAndAttachView();
		m_StreamEntity.InitMeteors(MeteorBlueprint);
		Game.Instance.EntitySpawner.SpawnEntityImmediately(m_StreamEntity, Game.Instance.LoadedAreaState.MainState);
		m_StreamEntity.SpawnImmediately();
		m_StreamEntity.CombatState.JoinCombat();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
