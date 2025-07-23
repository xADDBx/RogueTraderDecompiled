using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.Evaluators;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.UnityExtensions;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("fd702e0de4d04c6e83cfe0d645446780")]
public class PartyPetSpawner : UnitSpawnerBase, IAddInspectorGUI, IEtudesUpdateHandler, ISubscriber
{
	public new class MyData : UnitSpawnerBase.MyData
	{
		internal bool IsControl { get; set; }

		public MyData(EntityViewBase view)
			: base(view)
		{
		}

		protected MyData(JsonConstructorMark _)
			: base(_)
		{
		}

		protected override void OnDispose()
		{
			UnitPartPetOwner unitPartPetOwner = base.SpawnedUnit.Get<BaseUnitEntity>().Master?.GetOptional<UnitPartPetOwner>();
			if (unitPartPetOwner != null)
			{
				if (IsControl)
				{
					unitPartPetOwner.StartFollowing();
				}
				base.OnDispose();
			}
		}
	}

	[SerializeField]
	private BlueprintPetOwnerPriorityConfigReference m_PetOwnerPriorityConfig;

	[SerializeField]
	private ConditionsReference m_ControlCondition;

	private bool IsInControl
	{
		get
		{
			return ((MyData)base.Data).IsControl;
		}
		set
		{
			((MyData)base.Data).IsControl = value;
		}
	}

	protected override AbstractUnitEntity SpawnUnit(Vector3 position, Quaternion rotation)
	{
		return CheckControl();
	}

	private AbstractUnitEntity CheckControl()
	{
		BaseUnitEntity existingPet = GetExistingPet();
		if (existingPet == null)
		{
			return null;
		}
		UnitPartPetOwner unitPartPetOwner = existingPet.Master?.GetOptional<UnitPartPetOwner>();
		if (unitPartPetOwner == null)
		{
			return null;
		}
		bool isPetFollowing = unitPartPetOwner.IsPetFollowing;
		bool flag = (m_ControlCondition?.Get())?.Check() ?? true;
		bool flag2 = existingPet.IsInCombat || existingPet.Master.IsInCombat;
		if (flag && isPetFollowing && !IsInControl && !flag2)
		{
			unitPartPetOwner.StopFollowing();
			IsInControl = true;
			base.Data.SpawnedUnit = existingPet;
			existingPet.Position = base.ViewTransform.position;
			existingPet.SetOrientation(base.ViewTransform.rotation.eulerAngles.y);
			existingPet.PetCanDialogInteract = true;
			return existingPet;
		}
		if (!flag && IsInControl)
		{
			unitPartPetOwner.StartFollowing();
			IsInControl = false;
			existingPet.PetCanDialogInteract = true;
			base.Data.SpawnedUnit = default(EntityRef<AbstractUnitEntity>);
			base.HasSpawned = false;
		}
		return null;
	}

	private BaseUnitEntity GetExistingPet()
	{
		List<BaseUnitEntity> list = Game.Instance.Player.PartyAndPets.Where((BaseUnitEntity u) => u.Blueprint.CheckEqualsWithPrototype(base.Blueprint)).ToList();
		if (list.Count == 0)
		{
			return null;
		}
		PetOwnerPriorityConfig petOwnerPriorityConfig = m_PetOwnerPriorityConfig?.Get();
		if (petOwnerPriorityConfig == null)
		{
			return list.Random(PFStatefulRandom.Designers);
		}
		List<UnitPartPetOwner> units = (from p in list
			where p.Master != null
			select p.Master.GetOptional<UnitPartPetOwner>()).ToList();
		for (int i = 0; i < petOwnerPriorityConfig.PriorityOrder.Count; i++)
		{
			UnitPartPetOwner byPriority = petOwnerPriorityConfig.PriorityOrder[i].GetByPriority(units);
			if (byPriority != null)
			{
				return byPriority.PetUnit;
			}
		}
		return null;
	}

	public void OnEtudesUpdate()
	{
		CheckControl();
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new MyData(this));
	}
}
