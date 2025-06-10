using JetBrains.Annotations;
using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartUnitDescription : BaseUnitPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartUnitDescription>, IEntityPartOwner
	{
		PartUnitDescription Description { get; }
	}

	[JsonProperty]
	[CanBeNull]
	public string CustomPetName;

	[JsonProperty]
	[CanBeNull]
	public Gender? CustomGender { get; private set; }

	[JsonProperty]
	[CanBeNull]
	public string CustomName { get; private set; }

	[JsonProperty]
	public bool ForceUseClassEquipment { get; private set; }

	public Gender Gender => CustomGender ?? base.Owner.Blueprint.Gender;

	[NotNull]
	public string Name => base.Owner.GetOptional<PartPolymorphed>()?.ReplaceBlueprintForInspection?.CharacterName ?? CustomName ?? base.Owner.Blueprint.CharacterName ?? "";

	public bool UseClassEquipment
	{
		get
		{
			if (base.Owner.CopyOf.Entity == null)
			{
				if (!ForceUseClassEquipment && !base.Owner.IsMainCharacter)
				{
					return base.Owner.IsCustomCompanion();
				}
				return true;
			}
			return base.Owner.CopyOf.Entity.Description.UseClassEquipment;
		}
	}

	protected override void OnAttach()
	{
		SpawningData current = ContextData<SpawningData>.Current;
		if (current != null)
		{
			CustomGender = current.Gender;
		}
	}

	public void SetGender(Gender gender)
	{
		CustomGender = gender;
	}

	public void SetName([CanBeNull] string name)
	{
		CustomName = (name.IsNullOrEmpty() ? null : name);
	}

	public void SetUseClassEquipment(bool use)
	{
		ForceUseClassEquipment = use;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		if (CustomGender.HasValue)
		{
			Gender val2 = CustomGender.Value;
			result.Append(ref val2);
		}
		result.Append(CustomName);
		result.Append(CustomPetName);
		bool val3 = ForceUseClassEquipment;
		result.Append(ref val3);
		return result;
	}
}
