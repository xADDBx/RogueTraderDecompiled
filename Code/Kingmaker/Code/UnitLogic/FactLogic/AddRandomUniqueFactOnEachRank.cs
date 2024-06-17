using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.Code.UnitLogic.FactLogic;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[TypeId("e40a52710a2d44ddab5083812e6d458d")]
public class AddRandomUniqueFactOnEachRank : MechanicEntityFactComponentDelegate, IHashable
{
	public class ComponentData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public readonly List<EntityFactRef<MechanicEntityFact>> AddedFacts = new List<EntityFactRef<MechanicEntityFact>>();

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			List<EntityFactRef<MechanicEntityFact>> addedFacts = AddedFacts;
			if (addedFacts != null)
			{
				for (int i = 0; i < addedFacts.Count; i++)
				{
					EntityFactRef<MechanicEntityFact> obj = addedFacts[i];
					Hash128 val2 = StructHasher<EntityFactRef<MechanicEntityFact>>.GetHash128(ref obj);
					result.Append(ref val2);
				}
			}
			return result;
		}
	}

	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_Facts;

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> Facts
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] facts = m_Facts;
			return facts;
		}
	}

	private int Rank => base.Fact.GetRank();

	protected override void OnActivateOrPostLoad()
	{
		Update();
	}

	protected override void OnFactDetached()
	{
		RequestSavableData<ComponentData>().AddedFacts.RemoveAll(delegate(EntityFactRef<MechanicEntityFact> i)
		{
			base.Owner.Facts.Remove(i.Fact);
			return true;
		});
	}

	private void Update()
	{
		MechanicEntity concreteOwner = base.Owner;
		ComponentData componentData = RequestSavableData<ComponentData>();
		componentData.AddedFacts.RemoveAll(delegate(EntityFactRef<MechanicEntityFact> i)
		{
			int num;
			if (i.Fact != null)
			{
				num = ((!Facts.HasReference(i.Fact.Blueprint)) ? 1 : 0);
				if (num == 0)
				{
					goto IL_0049;
				}
			}
			else
			{
				num = 1;
			}
			concreteOwner.Facts.Remove(i.Fact);
			goto IL_0049;
			IL_0049:
			return (byte)num != 0;
		});
		while (componentData.AddedFacts.Count > 0 && componentData.AddedFacts.Count > Rank)
		{
			List<EntityFactRef<MechanicEntityFact>> addedFacts = componentData.AddedFacts;
			EntityFactRef<MechanicEntityFact> entityFactRef = addedFacts[addedFacts.Count - 1];
			concreteOwner.Facts.Remove((MechanicEntityFact)entityFactRef);
			componentData.AddedFacts.Remove(entityFactRef);
		}
		while (componentData.AddedFacts.Count < Rank)
		{
			BlueprintMechanicEntityFact blueprintMechanicEntityFact = Facts.Where((BlueprintMechanicEntityFact i) => !concreteOwner.Facts.Contains(i)).Random(PFStatefulRandom.Mechanics);
			if (blueprintMechanicEntityFact != null)
			{
				MechanicEntityFact mechanicEntityFact = concreteOwner.Facts.Add(blueprintMechanicEntityFact.CreateFact(base.Context, concreteOwner, default(BuffDuration)));
				if (mechanicEntityFact != null)
				{
					mechanicEntityFact.AddSource(base.Fact, this);
					componentData.AddedFacts.Add(mechanicEntityFact);
					continue;
				}
				break;
			}
			break;
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
