using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.ResourceLinks;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.View.Spawners;

public class AssetsPreload : MonoBehaviour, IResourcePreloadHandler, ISubscriber
{
	[SerializeField]
	[FormerlySerializedAs("Units")]
	private BlueprintUnitReference[] m_Units = new BlueprintUnitReference[0];

	public PrefabLink[] Prefabs = new PrefabLink[0];

	public ReferenceArrayProxy<BlueprintUnit> Units
	{
		get
		{
			BlueprintReference<BlueprintUnit>[] units = m_Units;
			return units;
		}
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void OnPreloadResources()
	{
		foreach (BlueprintUnit unit in Units)
		{
			unit.Prefab.Preload();
			unit.PreloadResources();
		}
		PrefabLink[] prefabs = Prefabs;
		for (int i = 0; i < prefabs.Length; i++)
		{
			prefabs[i].Preload();
		}
	}
}
