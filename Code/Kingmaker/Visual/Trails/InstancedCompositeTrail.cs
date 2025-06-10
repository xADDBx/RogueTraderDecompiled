using JetBrains.Annotations;
using Kingmaker.QA.Profiling;
using Kingmaker.Utility.Attributes;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Visual.Trails;

public class InstancedCompositeTrail : MonoBehaviour
{
	[ValidateNotNull]
	[AssetPicker("Assets/FX/Prefabs/")]
	public CompositeTrailRenderer Prefab;

	public int InstancesCount = 1;

	public int InstanceId = -1;

	[CanBeNull]
	private CompositeTrailRenderer m_TrailInstance;

	private void OnEnable()
	{
		m_TrailInstance = InstancedCompositeTrailsManager.RequestInstance(this);
	}

	private void LateUpdate()
	{
		using (Counters.Trails?.Measure())
		{
			if (Prefab == null)
			{
				return;
			}
			if (m_TrailInstance == null)
			{
				m_TrailInstance = InstancedCompositeTrailsManager.RequestInstance(this);
				if (m_TrailInstance == null)
				{
					PFLog.Default.Warning($"Could not request instance of composite trail: {this}");
					return;
				}
			}
			if (GameCameraCulling.IsCulled(base.transform.position))
			{
				return;
			}
			m_TrailInstance.InstancingUpdate();
			Vector3 position = base.transform.position;
			Quaternion q;
			switch (Prefab.Alignment)
			{
			case CompositeTrailRenderer.TrailAlignment.View:
			{
				Camera camera = Game.GetCamera();
				if (camera == null)
				{
					q = Quaternion.identity;
					break;
				}
				Vector3 forward = camera.transform.position - position;
				forward.y = 0f;
				q = Quaternion.LookRotation(forward);
				break;
			}
			case CompositeTrailRenderer.TrailAlignment.World:
				q = base.transform.rotation;
				break;
			default:
				q = Quaternion.identity;
				break;
			}
			Matrix4x4 matrix = Matrix4x4.TRS(position, q, Vector3.one);
			m_TrailInstance.DrawMesh(matrix);
		}
	}
}
