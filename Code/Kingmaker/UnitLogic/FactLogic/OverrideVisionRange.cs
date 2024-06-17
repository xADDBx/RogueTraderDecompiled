using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Passive/Override vision range")]
[AllowedOn(typeof(BlueprintUnit))]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67915f4130ad1714a9ba6b2fd210e28f")]
public class OverrideVisionRange : BlueprintComponent, IRuntimeEntityFactComponentProvider
{
	public class Runtime : EntityFactComponent<BaseUnitEntity, OverrideVisionRange>, IHashable
	{
		protected override void OnActivate()
		{
			base.Owner.Vision.VisionRangeMetersOverride = base.SourceBlueprintComponent.VisionRangeInMeters;
			if (base.SourceBlueprintComponent.AlsoInCombat)
			{
				base.Owner.Vision.CombatVisionRangeMetersOverride = base.SourceBlueprintComponent.VisionRangeInMeters;
			}
		}

		protected override void OnDeactivate()
		{
			PartVision vision = base.Owner.Vision;
			float? combatVisionRangeMetersOverride = (base.Owner.Vision.VisionRangeMetersOverride = null);
			vision.CombatVisionRangeMetersOverride = combatVisionRangeMetersOverride;
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[Range(0f, 22f)]
	public int VisionRangeInMeters = 8;

	[InfoBox(Text = "Очень опасная галка, может ломать вход/выход из боя")]
	public bool AlsoInCombat;

	public EntityFactComponent CreateRuntimeFactComponent()
	{
		return new Runtime();
	}
}
