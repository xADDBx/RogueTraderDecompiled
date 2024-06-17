using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UI;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartPersonalEnemy : BaseUnitPart, IAreaHandler, ISubscriber, IHashable
{
	private UnitPersonalEnemyFx m_FxComponent;

	[JsonProperty]
	public UnitReference Enemy { get; private set; }

	public bool IsCurrentlyTargetable => UIAccess.SelectionManager.IsSelected(Enemy.ToAbstractUnitEntity());

	public void Init(BaseUnitEntity enemy)
	{
		Enemy = enemy.FromBaseUnitEntity();
		m_FxComponent = base.Owner.View.gameObject.AddComponent<UnitPersonalEnemyFx>();
		m_FxComponent.Data = this;
	}

	protected override void OnDetach()
	{
		if ((bool)m_FxComponent)
		{
			Object.Destroy(m_FxComponent);
		}
	}

	protected override void OnPostLoad()
	{
		EventBus.Subscribe(this);
	}

	public void OnAreaBeginUnloading()
	{
	}

	public void OnAreaDidLoad()
	{
		Init(Enemy.ToBaseUnitEntity());
		EventBus.Unsubscribe(this);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		UnitReference obj = Enemy;
		Hash128 val2 = UnitReferenceHasher.GetHash128(ref obj);
		result.Append(ref val2);
		return result;
	}
}
