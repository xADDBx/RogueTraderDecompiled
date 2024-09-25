using Owlcat.Runtime.Visual.Highlighting;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

public class AddSelectionHighlighter : MonoBehaviour, IFxSpawner
{
	private Highlighter m_Highlighter;

	public FxSpawnerPriority Priority => FxSpawnerPriority.Initialize;

	public void SpawnFxOnGameObject(GameObject target)
	{
		m_Highlighter = target.GetComponent<Highlighter>();
		if ((bool)m_Highlighter)
		{
			m_Highlighter.AddExtraRenderer(GetComponent<Renderer>());
		}
	}

	public void SpawnFxOnPoint(Vector3 point, Quaternion rotation)
	{
	}

	private void OnDisable()
	{
		if ((bool)m_Highlighter)
		{
			m_Highlighter.RemoveExtraRenderer(GetComponent<Renderer>());
		}
	}
}
