using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("8aedcbb167a34180968f45e6201fd29f")]
public class AddEnergyDamageImmunity : BlueprintComponent
{
}
