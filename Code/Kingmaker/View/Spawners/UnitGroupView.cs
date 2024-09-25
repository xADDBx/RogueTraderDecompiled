using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("b94416fff5c65f44e9ec143a7ceb887e")]
public class UnitGroupView : EntityViewBase
{
	public class UnitGroupData : SimpleEntity, IHashable
	{
		public UnitGroupData(EntityViewBase view)
			: base(view)
		{
		}

		protected UnitGroupData(JsonConstructorMark _)
			: base(_)
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	public Color GizmosColor = Color.red;

	public bool DisableOnSimplified;

	public bool IgnoreInEncoutnerStatistic;

	public SquadSettings[] Squads = new SquadSettings[0];

	public override bool CreatesDataOnLoad => true;

	protected override void OnEnable()
	{
		base.OnEnable();
		foreach (Transform item in base.ViewTransform)
		{
			UnitSpawner component = item.GetComponent<UnitSpawner>();
			if ((bool)component)
			{
				component.SetGroupView(this);
			}
		}
		for (int i = 0; i < Squads.Length; i++)
		{
			SquadSettings squadSettings = Squads[i];
			if (squadSettings != null)
			{
				UnitSpawner[] spawners = squadSettings.Spawners;
				for (int j = 0; j < spawners.Length; j++)
				{
					spawners[j].SetSquadId($"{UniqueId}_squad{i}");
				}
				if (squadSettings.Leader != null)
				{
					squadSettings.Leader.SetSquadId($"{UniqueId}_squad{i}");
					squadSettings.Leader.MarkAsSquadLeader();
				}
			}
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new UnitGroupData(this));
	}
}
