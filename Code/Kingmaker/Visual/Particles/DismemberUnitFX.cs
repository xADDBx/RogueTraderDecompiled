using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.Mechanics.Entities;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class DismemberUnitFX : MonoBehaviour
{
	public float Delay = 0.1f;

	public void Init(AbstractUnitEntityView unit)
	{
		if (unit.EntityData is BaseUnitEntity baseUnitEntity)
		{
			baseUnitEntity.GetOrCreate<PartDropLootAndDestroyAfterDelay>().Setup(Delay);
		}
		else
		{
			unit.EntityData.GetOrCreate<PartDestroyAfterDelay>().Setup(Delay);
		}
	}
}
