using System;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("154b566cab8b4735bd573f4bea26019f")]
public class DOTLogicVisual : BlueprintComponent
{
	public DOT Type;
}
