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
[TypeId("483fb23d5cb1dcb43817b1b835a30ceb")]
public class AddEnergyImmunity : BlueprintComponent
{
}
