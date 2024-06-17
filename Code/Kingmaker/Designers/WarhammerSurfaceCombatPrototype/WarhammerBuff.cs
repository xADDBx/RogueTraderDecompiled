using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.WarhammerSurfaceCombatPrototype;

[Deprecated]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("3f160d3cce3b4805bc093ae1fc1dcb8e")]
public class WarhammerBuff : UnitBuffComponentDelegate, IHashable
{
	[InfoBox("Этот компонент больше не используется и в будущем будет удалён.\nИспользуйте ContextActionApplyBuff или другие подходящие компоненты для указания длительности баффа")]
	public int Duration;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
