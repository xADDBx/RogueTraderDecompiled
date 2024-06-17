using Kingmaker.View;
using Kingmaker.Visual.Particles;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPersonalEnemyFx : MonoBehaviour
{
	private GameObject m_Fx;

	public UnitPartPersonalEnemy Data { get; set; }

	private void Update()
	{
		if ((bool)m_Fx != !Data.IsCurrentlyTargetable)
		{
			if ((bool)m_Fx)
			{
				FxHelper.Destroy(m_Fx);
			}
			else
			{
				m_Fx = FxHelper.SpawnFxOnEntity(Game.Instance.BlueprintRoot.Prefabs.PersonalEnemyFxPrefab, GetComponent<UnitEntityView>());
			}
		}
	}

	private void OnDestroy()
	{
		if ((bool)m_Fx)
		{
			FxHelper.Destroy(m_Fx);
		}
	}
}
