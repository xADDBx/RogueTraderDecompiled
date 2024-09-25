using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using UnityEngine;

namespace Kingmaker.QA;

public class MegatronPoint : MonoBehaviour
{
	[SerializeField]
	private string m_UID;

	[SerializeField]
	private BlueprintAreaReference m_Area;

	public string UID
	{
		get
		{
			return m_UID;
		}
		set
		{
			m_UID = value;
		}
	}

	public BlueprintArea Area
	{
		get
		{
			return m_Area.Get();
		}
		set
		{
			m_Area = value.ToReference<BlueprintAreaReference>();
		}
	}
}
