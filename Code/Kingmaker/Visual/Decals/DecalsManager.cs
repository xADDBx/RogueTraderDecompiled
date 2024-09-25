using System.Collections.Generic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

[ExecuteInEditMode]
public class DecalsManager : MonoBehaviour
{
	private static Dictionary<Decal, DecalsManager> s_Managers = new Dictionary<Decal, DecalsManager>();

	[SerializeField]
	[InspectorReadOnly]
	private List<DecalGeometryHolder> m_Holders = new List<DecalGeometryHolder>();

	private static DecalsManager GetManager(Decal decal, bool noCache = false)
	{
		bool flag = false;
		if (!s_Managers.TryGetValue(decal, out var value))
		{
			flag = true;
		}
		else if (value == null)
		{
			flag = true;
		}
		if (flag && !noCache)
		{
			value = Object.FindObjectsOfType<DecalsManager>().FirstOrDefault((DecalsManager m) => m.gameObject.scene.Equals(decal.gameObject.scene));
			if (value == null)
			{
				GameObject obj = new GameObject("[DECALS MANAGER]");
				value = obj.AddComponent<DecalsManager>();
				s_Managers[decal] = value;
				obj.transform.parent = decal.transform;
				obj.transform.parent = null;
				obj.transform.position = default(Vector3);
				obj.transform.rotation = Quaternion.identity;
				obj.transform.localScale = Vector3.one;
			}
		}
		return value;
	}

	private void Update()
	{
		ProcessHolders();
	}

	private void ProcessHolders()
	{
		for (int i = 0; i < m_Holders.Count; i++)
		{
			DecalGeometryHolder decalGeometryHolder = m_Holders[i];
			if (decalGeometryHolder == null)
			{
				m_Holders.RemoveAt(i);
				i--;
			}
			else if (decalGeometryHolder.gameObject == null)
			{
				m_Holders.RemoveAt(i);
				i--;
			}
			else if (decalGeometryHolder.Decal == null)
			{
				Object.DestroyImmediate(decalGeometryHolder.gameObject);
				m_Holders.RemoveAt(i);
				i--;
			}
			else if (decalGeometryHolder.Decal.gameObject.activeInHierarchy != decalGeometryHolder.gameObject.activeInHierarchy)
			{
				decalGeometryHolder.gameObject.SetActive(decalGeometryHolder.Decal.gameObject.activeInHierarchy);
			}
		}
	}
}
