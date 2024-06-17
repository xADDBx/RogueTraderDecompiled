using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.RenderPipeline;

public class CameraChain
{
	private Stack<List<Camera>> m_CameraQueuePool = new Stack<List<Camera>>();

	private Dictionary<CameraGroupDescriptor, List<Camera>> m_Chain = new Dictionary<CameraGroupDescriptor, List<Camera>>();

	private List<CameraGroupDescriptor> m_SortedGroups = new List<CameraGroupDescriptor>();

	private CameraChainDescriptor m_CameraChainDesc = new CameraChainDescriptor();

	public IEnumerable<CameraChainDescriptor> EnumerateCameras()
	{
		foreach (CameraGroupDescriptor sortedGroup in m_SortedGroups)
		{
			List<Camera> cameraQueue = m_Chain[sortedGroup];
			for (int i = 0; i < cameraQueue.Count; i++)
			{
				m_CameraChainDesc.IsFirst = i == 0;
				m_CameraChainDesc.IsLast = i == cameraQueue.Count - 1;
				m_CameraChainDesc.Camera = cameraQueue[i];
				yield return m_CameraChainDesc;
			}
		}
	}

	public void Update(Camera[] cameras)
	{
		Reset();
		foreach (Camera camera in cameras)
		{
			OwlcatAdditionalCameraData component = camera.GetComponent<OwlcatAdditionalCameraData>();
			CameraGroupDescriptor key = new CameraGroupDescriptor(camera.targetTexture, component?.DepthTexture, camera.pixelRect);
			if (!m_Chain.TryGetValue(key, out var value))
			{
				value = ClaimQueue();
				m_Chain.Add(key, value);
			}
			value.Add(camera);
		}
		foreach (KeyValuePair<CameraGroupDescriptor, List<Camera>> item in m_Chain)
		{
			item.Value.Sort((Camera lhs, Camera rhs) => (int)(lhs.depth - rhs.depth));
		}
		m_SortedGroups.AddRange(m_Chain.Keys);
		m_SortedGroups.Sort((CameraGroupDescriptor lhs, CameraGroupDescriptor rhs) => (int)(rhs.Viewport.width * rhs.Viewport.height - lhs.Viewport.width * lhs.Viewport.height));
	}

	private List<Camera> ClaimQueue()
	{
		if (m_CameraQueuePool.Count > 0)
		{
			return m_CameraQueuePool.Pop();
		}
		return new List<Camera>();
	}

	private void Reset()
	{
		foreach (KeyValuePair<CameraGroupDescriptor, List<Camera>> item in m_Chain)
		{
			item.Value.Clear();
			m_CameraQueuePool.Push(item.Value);
		}
		m_Chain.Clear();
		m_SortedGroups.Clear();
	}
}
