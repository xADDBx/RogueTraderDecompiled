using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

[RequireComponent(typeof(CombatHUDRenderer))]
public class ThreateningAreaVisualizer : MonoBehaviour
{
	[SerializeField]
	private GameObject m_Decal;

	private readonly List<GameObject> m_CellsShown = new List<GameObject>();

	private CombatHUDRenderer m_CombatHUDRenderer;

	public static ThreateningAreaVisualizer Instance { get; private set; }

	public static void ShowThreateningAreaForTarget(BaseUnitEntity target)
	{
		Instance.m_CellsShown.ForEach(delegate(GameObject c)
		{
			c.SetActive(value: false);
		});
		HashSet<GraphNode> hashSet = new HashSet<GraphNode>();
		foreach (BaseUnitEntity engagedByUnit in target.GetEngagedByUnits())
		{
			if (engagedByUnit.State.CanAct)
			{
				hashSet.UnionWith(engagedByUnit.GetThreateningArea());
			}
		}
		while (Instance.m_CellsShown.Count < hashSet.Count)
		{
			GameObject gameObject = Object.Instantiate(Instance.m_Decal, Vector3.zero, Quaternion.identity);
			gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
			Instance.m_CellsShown.Add(gameObject);
		}
		List<GameObject>.Enumerator enumerator2 = Instance.m_CellsShown.GetEnumerator();
		foreach (GraphNode item in hashSet)
		{
			enumerator2.MoveNext();
			ShowDecalAt(enumerator2.Current, item);
		}
	}

	public static void HideThreateningArea()
	{
		Instance.m_CellsShown.ForEach(delegate(GameObject c)
		{
			c.SetActive(value: false);
		});
	}

	private static void ShowDecalAt(GameObject decal, GraphNode cell)
	{
		decal.transform.position = (Vector3)cell.position;
		decal.SetActive(value: true);
	}

	private void OnEnable()
	{
		Instance = this;
		m_CombatHUDRenderer = GetComponent<CombatHUDRenderer>();
	}

	private void OnDisable()
	{
		Instance = null;
	}
}
