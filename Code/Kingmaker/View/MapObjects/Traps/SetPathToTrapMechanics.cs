using System.Collections.Generic;
using System.Linq;
using Dreamteck.Splines;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Kingmaker.View.MapObjects.Traps;

[ExecuteInEditMode]
[RequireComponent(typeof(Seeker), typeof(SplineComputer))]
public class SetPathToTrapMechanics : MonoBehaviour, SetPathToTrapMechanics.IEditor
{
	public interface IEditor
	{
		bool IsGeneratingPath { get; }

		void GenerateComplexPath();

		void GenerateSimplePath();

		void GenerateStraightLine();

		void GenerateComplexFlatPath();

		void Refresh();

		void SetEnd(Transform end);
	}

	[SerializeField]
	private float m_LineHeight = 0.5f;

	[FormerlySerializedAs("start")]
	[SerializeField]
	private Transform _start;

	[FormerlySerializedAs("end")]
	[SerializeField]
	private Transform _end;

	[SerializeField]
	private LayerMask _raycastLayers = 2097408;

	[SerializeField]
	private float _projectionHeightOffset = 20f;

	[SerializeField]
	private float _projectedLineHeight = 0.05f;

	private SplineComputer m_SplineComputer;

	private Seeker m_Seeker;

	private MonoModifier[] _modifiers;

	private bool _isGeneratingPath;

	bool IEditor.IsGeneratingPath => _isGeneratingPath;

	public void OnEnable()
	{
		Initialize();
	}

	void IEditor.GenerateStraightLine()
	{
		if (HasTransforms())
		{
			_isGeneratingPath = true;
			SavePositions(_start, _end);
			Initialize();
			ResetSpline();
			m_SplineComputer.SetPoints(new SplinePoint[2]
			{
				new SplinePoint(_start.position),
				new SplinePoint(_end.position)
			});
			SaveStats();
			_isGeneratingPath = false;
		}
	}

	void IEditor.GenerateComplexPath()
	{
		if (HasTransforms())
		{
			GenerateComplexPath(_start, _end);
		}
	}

	void IEditor.GenerateComplexFlatPath()
	{
		if (HasTransforms())
		{
			GenerateComplexFlatPath(_start, _end);
		}
	}

	void IEditor.GenerateSimplePath()
	{
		if (HasTransforms())
		{
			GenerateSimplePath(_start, _end);
		}
	}

	void IEditor.SetEnd(Transform end)
	{
		_end = end;
	}

	private bool HasTransforms()
	{
		if ((bool)_start && (bool)_end && (bool)m_SplineComputer)
		{
			return true;
		}
		PFLog.Default.Log(base.gameObject.name + " SetPathToTrapMechanics: transforms or computer are not setup");
		return false;
	}

	public void GenerateComplexPath(Transform start, Transform end)
	{
		_isGeneratingPath = true;
		Initialize();
		GeneratePath(start, end, delegate(Path path)
		{
			OnComplexPathComplete(path);
			_isGeneratingPath = false;
		});
	}

	public void GenerateComplexFlatPath(Transform start, Transform end)
	{
		_isGeneratingPath = true;
		Initialize();
		GeneratePath(start, end, delegate(Path path)
		{
			OnComplexFlatPathComplete(path, start, end);
			_isGeneratingPath = false;
		});
	}

	public void GenerateSimplePath(Transform startTransform, Transform endTransform)
	{
		_isGeneratingPath = true;
		Initialize();
		GeneratePath(startTransform, endTransform, delegate(Path path)
		{
			OnSimplePathComplete(path);
			_isGeneratingPath = false;
		});
	}

	private void GeneratePath(Transform startTransform, Transform endTransform, OnPathDelegate OnPathCompleteCallback)
	{
		SavePositions(startTransform, endTransform);
		AstarPath.FindAstarPath();
		if (AstarPath.active == null)
		{
			PFLog.Default.Error(base.gameObject.name + " Could not find NavMesh for A* pathfinding in loaded scenes! Scenes:" + string.Join(",", from scene in SceneManager.GetAllScenes()
				select scene.name));
			Debug.LogError(base.gameObject.name + " Could not find NavMesh for A* pathfinding in loaded scenes! Scenes: " + string.Join(",", from scene in SceneManager.GetAllScenes()
				select scene.name));
			OnPathCompleteCallback(null);
			return;
		}
		AstarPath.active.Scan();
		WarhammerABPath abPath = WarhammerABPath.Construct(startTransform.position, endTransform.position);
		abPath.Claim(this);
		m_Seeker.StartPath(abPath, delegate(Path path)
		{
			if (path.vectorPath.Count < 1)
			{
				abPath.Release(this);
				PFLog.Default.Warning("Could not create Path " + base.gameObject.name);
			}
			else
			{
				OnPathCompleteCallback(path);
				abPath.Release(this);
			}
		});
	}

	private void SavePositions(Transform startTransform, Transform endTransform)
	{
		_start = startTransform;
		_end = endTransform;
	}

	private void OnComplexPathComplete(Path path)
	{
		if (path == null)
		{
			Debug.LogError("OnComplexPathComplete no path received, aborting.");
			return;
		}
		ResetSpline();
		List<Vector3> vectorPath = path.vectorPath;
		for (int i = 0; i < vectorPath.Count; i++)
		{
			vectorPath[i] = new Vector3(vectorPath[i].x, vectorPath[i].y + m_LineHeight, vectorPath[i].z);
		}
		vectorPath[vectorPath.Count - 1] = _end.position;
		if ((_start.position - vectorPath[0]).To2D().magnitude > 0.1f)
		{
			vectorPath.Insert(0, _start.position);
		}
		for (int j = 0; j < vectorPath.Count; j++)
		{
			m_SplineComputer.SetPoint(j, new SplinePoint(vectorPath[j]));
		}
		m_SplineComputer.RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
		SaveStats();
	}

	private void OnComplexFlatPathComplete(Path path, Transform start, Transform end)
	{
		if (path == null)
		{
			Debug.LogError("OnComplexFlatPathComplete no path received, aborting.");
			return;
		}
		ResetSpline();
		List<Vector3> vectorPath = path.vectorPath;
		for (int i = 0; i < vectorPath.Count; i++)
		{
			Vector3 point = vectorPath[i];
			point = SurfaceRaycastUtils.RaycastPointToSurface(point, _projectionHeightOffset, _raycastLayers);
			point += Vector3.up * _projectedLineHeight;
			vectorPath[i] = point;
		}
		if ((start.position - vectorPath[0]).magnitude > 0.1f)
		{
			vectorPath.Insert(0, start.position);
		}
		if ((end.position - vectorPath[vectorPath.Count - 1]).magnitude > 0.1f)
		{
			vectorPath.Add(end.position);
		}
		for (int j = 0; j < vectorPath.Count; j++)
		{
			m_SplineComputer.SetPoint(j, new SplinePoint(vectorPath[j]));
		}
		m_SplineComputer.RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
		SaveStats();
	}

	private void OnSimplePathComplete(Path path)
	{
		if (path == null)
		{
			Debug.LogError("OnSimplePathComplete no path received, aborting.");
			return;
		}
		ResetSpline();
		List<Vector3> vectorPath = path.vectorPath;
		for (int i = 0; i < vectorPath.Count; i++)
		{
			vectorPath[i] = new Vector3(vectorPath[i].x, vectorPath[i].y + m_LineHeight, vectorPath[i].z);
		}
		List<Vector3> list = vectorPath;
		list[list.Count - 1] = _end.position;
		List<Vector3> obj = new List<Vector3> { vectorPath[0] };
		List<Vector3> list2 = vectorPath;
		obj.Add(list2[list2.Count - 1]);
		vectorPath = obj;
		if (Vector3.Distance(_start.position, vectorPath[0]) > 0.1f)
		{
			vectorPath.Insert(0, _start.position);
		}
		for (int j = 0; j < vectorPath.Count; j++)
		{
			m_SplineComputer.SetPoint(j, new SplinePoint(vectorPath[j]));
		}
		m_SplineComputer.RebuildImmediate(calculateSamples: true, forceUpdateAll: true);
		SaveStats();
	}

	private void ResetSpline()
	{
		if (!m_SplineComputer)
		{
			Initialize();
		}
		m_SplineComputer.SetPoints(new SplinePoint[2]
		{
			new SplinePoint(Vector3.zero),
			new SplinePoint(Vector3.one)
		});
		m_SplineComputer.RebuildImmediate();
	}

	public void SaveStats()
	{
	}

	private static bool CompareLists(List<Vector3> a, List<Vector3> b)
	{
		if (a.Count == b.Count)
		{
			for (int i = 0; i < a.Count; i++)
			{
				if (a[i] != b[i])
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	void IEditor.Refresh()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (!m_Seeker)
		{
			m_Seeker = GetComponent<Seeker>();
		}
		if (!m_SplineComputer)
		{
			m_SplineComputer = GetComponent<SplineComputer>();
		}
		_modifiers = GetComponents<MonoModifier>();
		m_Seeker.ClearModifiers();
		m_Seeker.RegisterModifier(m_Seeker.startEndModifier);
		MonoModifier[] modifiers = _modifiers;
		foreach (MonoModifier monoModifier in modifiers)
		{
			if (monoModifier.enabled)
			{
				m_Seeker.RegisterModifier(monoModifier);
			}
		}
	}
}
