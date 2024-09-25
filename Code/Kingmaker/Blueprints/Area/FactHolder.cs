using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints.Area;

[DisallowMultipleComponent]
public class FactHolder : MonoBehaviour
{
	public class Fact : MechanicEntityFact, IHashable
	{
		public Fact(BlueprintLogicConnector blueprint)
			: base(blueprint)
		{
		}

		[JsonConstructor]
		private Fact()
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}

	[SerializeField]
	private BlueprintLogicConnectorReference m_Blueprint;

	private MapObjectView m_MapObjectView;

	public BlueprintLogicConnector Blueprint
	{
		get
		{
			return m_Blueprint?.Get();
		}
		set
		{
			m_Blueprint = value.ToReference<BlueprintLogicConnectorReference>();
		}
	}

	public MapObjectView MapObjectView => ObjectExtensions.Or(m_MapObjectView, null) ?? (m_MapObjectView = GetComponent<MapObjectView>());

	public EntityFact GetFact()
	{
		return ObjectExtensions.Or(MapObjectView, null)?.Data.Facts.Get(Blueprint);
	}
}
