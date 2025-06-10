using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Kingmaker.Visual;

public class CameraStackManager
{
	public enum CameraStackState
	{
		Full,
		UiOnly,
		AllExceptUi
	}

	[Flags]
	public enum CameraStackType
	{
		Background = 1,
		Main = 2,
		Ui = 4
	}

	public readonly struct CameraInfo
	{
		public readonly Camera camera;

		public readonly WaaaghAdditionalCameraData additionalCameraData;

		public readonly CameraStackType cameraStackType;

		public CameraInfo(Camera camera, WaaaghAdditionalCameraData additionalCameraData, CameraStackType cameraStackType)
		{
			this.camera = camera;
			this.additionalCameraData = additionalCameraData;
			this.cameraStackType = cameraStackType;
		}
	}

	public class CameraStackStateChangeScope : IDisposable
	{
		public readonly CameraStackState State;

		private CameraStackManager m_Manager;

		public CameraStackStateChangeScope(CameraStackManager manager, CameraStackState tempState)
		{
			State = tempState;
			m_Manager = manager;
			manager.StateAdd(this);
		}

		public void Dispose()
		{
			m_Manager?.StateRemove(this);
			m_Manager = null;
		}
	}

	private sealed class CameraInstance : IComparable<CameraInstance>
	{
		[CanBeNull]
		public readonly Camera camera;

		[CanBeNull]
		public readonly WaaaghAdditionalCameraData additionalCameraData;

		public readonly float depth;

		public int refCount;

		public bool inStack;

		public CameraInstance(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
		{
			this.camera = camera;
			this.additionalCameraData = additionalCameraData;
			depth = camera.depth;
			refCount = 0;
			inStack = false;
		}

		public bool IsValid()
		{
			if (camera != null)
			{
				return additionalCameraData != null;
			}
			return false;
		}

		public int CompareTo(CameraInstance other)
		{
			float num = depth;
			return num.CompareTo(other.depth);
		}
	}

	private sealed class CameraRecord : IComparable<CameraRecord>
	{
		[NotNull]
		public readonly CameraInstance cameraInstance;

		public readonly CameraStackType cameraStackType;

		public int count;

		public CameraRecord(CameraInstance cameraInstance, CameraStackType cameraStackType)
		{
			this.cameraInstance = cameraInstance;
			this.cameraStackType = cameraStackType;
			count = 0;
		}

		public int CompareTo(CameraRecord other)
		{
			int num = cameraStackType.CompareTo(other.cameraStackType);
			if (num != 0)
			{
				return num;
			}
			return cameraInstance.CompareTo(other.cameraInstance);
		}
	}

	private static readonly LogChannel s_LogChannel = LogChannelFactory.GetOrCreate("Camera");

	public static readonly CameraStackManager Instance = new CameraStackManager();

	private readonly List<CameraInstance> m_CameraInstances = new List<CameraInstance>();

	private readonly List<CameraRecord> m_CameraRecords = new List<CameraRecord>();

	private readonly List<CameraRecord> m_StackCameraRecords = new List<CameraRecord>();

	private readonly List<CameraStackStateChangeScope> m_StateStack = new List<CameraStackStateChangeScope>();

	public CameraStackState State
	{
		get
		{
			if (m_StateStack.Count <= 0)
			{
				return CameraStackState.Full;
			}
			List<CameraStackStateChangeScope> stateStack = m_StateStack;
			return stateStack[stateStack.Count - 1].State;
		}
	}

	[CanBeNull]
	public Camera ActiveMainCamera => GetCamera(CameraStackType.Main);

	public event EventHandler StackChanged;

	public CameraStackStateChangeScope SetTempState(CameraStackState state)
	{
		return new CameraStackStateChangeScope(this, state);
	}

	private void StateAdd(CameraStackStateChangeScope scope)
	{
		CameraStackState state = State;
		m_StateStack.Add(scope);
		if (state != scope.State)
		{
			LogCameraStackStateChanged(scope.State);
			UpdateStack();
		}
	}

	private void StateRemove(CameraStackStateChangeScope scope)
	{
		CameraStackState state = State;
		if (m_StateStack.Remove(scope))
		{
			CameraStackState state2 = State;
			if (state != state2)
			{
				LogCameraStackStateChanged(state2);
				UpdateStack();
			}
		}
	}

	[CanBeNull]
	public Camera GetMain()
	{
		return GetCamera(CameraStackType.Main);
	}

	[CanBeNull]
	public Camera GetFirstBase()
	{
		foreach (CameraRecord stackCameraRecord in m_StackCameraRecords)
		{
			if (stackCameraRecord.cameraInstance.IsValid())
			{
				return stackCameraRecord.cameraInstance.camera;
			}
		}
		return null;
	}

	[CanBeNull]
	public Camera GetCamera(CameraStackType cameraStackType)
	{
		foreach (CameraRecord stackCameraRecord in m_StackCameraRecords)
		{
			if (stackCameraRecord.cameraStackType == cameraStackType && stackCameraRecord.cameraInstance.IsValid())
			{
				return stackCameraRecord.cameraInstance.camera;
			}
		}
		return null;
	}

	public void GetStack(ICollection<CameraInfo> results)
	{
		foreach (CameraRecord stackCameraRecord in m_StackCameraRecords)
		{
			results.Add(new CameraInfo(stackCameraRecord.cameraInstance.camera, stackCameraRecord.cameraInstance.additionalCameraData, stackCameraRecord.cameraStackType));
		}
	}

	public void AddCamera(Camera camera, CameraStackType cameraStackType)
	{
		if (!camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			LogMissingAdditionalCameraDataError(camera, cameraStackType);
			return;
		}
		ValidateForDuplicates(camera, cameraStackType);
		CameraInstance orCreateCameraInstance = GetOrCreateCameraInstance(camera, component);
		CameraRecord orCreateCameraRecord = GetOrCreateCameraRecord(orCreateCameraInstance, cameraStackType);
		orCreateCameraRecord.count++;
		orCreateCameraRecord.cameraInstance.refCount++;
		LogCameraAddedToRegistry(orCreateCameraRecord);
		UpdateStack();
	}

	public void RemoveCamera(Camera camera, CameraStackType cameraStackType)
	{
		int i = 0;
		for (int count = m_CameraRecords.Count; i < count; i++)
		{
			CameraRecord cameraRecord = m_CameraRecords[i];
			if (cameraRecord.cameraStackType == cameraStackType && cameraRecord.cameraInstance.camera == camera)
			{
				cameraRecord.cameraInstance.refCount--;
				if (cameraRecord.cameraInstance.refCount == 0)
				{
					m_CameraInstances.Remove(cameraRecord.cameraInstance);
				}
				cameraRecord.count--;
				if (cameraRecord.count == 0)
				{
					m_CameraRecords.Remove(cameraRecord);
				}
				LogCameraRemovedFromRegistry(cameraRecord);
				UpdateStack();
				break;
			}
		}
	}

	private void UpdateStack()
	{
		RebuildCameraStack();
		UpdateCameraComponents();
		NotifyStackChanged();
		LogCameraStackUpdated();
	}

	private void RebuildCameraStack()
	{
		m_StackCameraRecords.Clear();
		foreach (CameraInstance cameraInstance in m_CameraInstances)
		{
			cameraInstance.inStack = false;
		}
		CameraStackType cameraStackType = GetCameraStackTypeFilter();
		foreach (CameraRecord cameraRecord in m_CameraRecords)
		{
			if (!cameraRecord.cameraInstance.inStack && (cameraRecord.cameraStackType & cameraStackType) != 0 && cameraRecord.cameraInstance.IsValid())
			{
				m_StackCameraRecords.Add(cameraRecord);
				cameraRecord.cameraInstance.inStack = true;
				cameraStackType &= ~cameraRecord.cameraStackType;
				if (cameraStackType == (CameraStackType)0)
				{
					break;
				}
			}
		}
		foreach (CameraInstance cameraInstance2 in m_CameraInstances)
		{
			if (cameraInstance2.camera != null)
			{
				cameraInstance2.camera.enabled = cameraInstance2.inStack;
			}
		}
	}

	private void UpdateCameraComponents()
	{
		if (m_StackCameraRecords.Count != 0)
		{
			CameraInstance cameraInstance = m_StackCameraRecords[0].cameraInstance;
			cameraInstance.additionalCameraData.RenderType = CameraRenderType.Base;
			cameraInstance.additionalCameraData.CameraStack.Clear();
			for (int i = 1; i < m_StackCameraRecords.Count; i++)
			{
				CameraInstance cameraInstance2 = m_StackCameraRecords[i].cameraInstance;
				cameraInstance2.additionalCameraData.RenderType = CameraRenderType.Overlay;
				cameraInstance.additionalCameraData.CameraStack.Add(cameraInstance2.camera);
			}
		}
	}

	private CameraStackType GetCameraStackTypeFilter()
	{
		return State switch
		{
			CameraStackState.Full => CameraStackType.Background | CameraStackType.Main | CameraStackType.Ui, 
			CameraStackState.UiOnly => CameraStackType.Ui, 
			CameraStackState.AllExceptUi => CameraStackType.Background | CameraStackType.Main, 
			_ => (CameraStackType)0, 
		};
	}

	private CameraInstance GetOrCreateCameraInstance(Camera camera, WaaaghAdditionalCameraData additionalCameraData)
	{
		foreach (CameraInstance cameraInstance2 in m_CameraInstances)
		{
			if (cameraInstance2.camera == camera)
			{
				return cameraInstance2;
			}
		}
		CameraInstance cameraInstance = new CameraInstance(camera, additionalCameraData);
		m_CameraInstances.Add(cameraInstance);
		return cameraInstance;
	}

	private CameraRecord GetOrCreateCameraRecord(CameraInstance cameraInstance, CameraStackType cameraStackType)
	{
		int i = 0;
		for (int count = m_CameraRecords.Count; i < count; i++)
		{
			CameraRecord cameraRecord = m_CameraRecords[i];
			if (cameraRecord.cameraStackType == cameraStackType && cameraRecord.cameraInstance == cameraInstance)
			{
				return cameraRecord;
			}
		}
		CameraRecord record = new CameraRecord(cameraInstance, cameraStackType);
		int insertIndexForNewRecord = GetInsertIndexForNewRecord(in record);
		m_CameraRecords.Insert(insertIndexForNewRecord, record);
		return record;
	}

	private int GetInsertIndexForNewRecord(in CameraRecord record)
	{
		for (int num = m_CameraRecords.Count - 1; num >= 0; num--)
		{
			if (record.CompareTo(m_CameraRecords[num]) >= 0)
			{
				return num + 1;
			}
		}
		return 0;
	}

	private void ValidateForDuplicates(Camera camera, CameraStackType cameraStackType)
	{
		foreach (CameraInstance cameraInstance in m_CameraInstances)
		{
			if (cameraInstance.camera == camera)
			{
				LogCameraInstanceIsAlreadyRegisteredError(camera);
				break;
			}
		}
		foreach (CameraRecord cameraRecord in m_CameraRecords)
		{
			if (cameraRecord.cameraStackType == cameraStackType)
			{
				LogCameraTypeIsAlreadyRegisteredError(camera, cameraStackType);
				break;
			}
		}
	}

	private void NotifyStackChanged()
	{
		this.StackChanged?.Invoke(this, EventArgs.Empty);
	}

	private static void LogCameraAddedToRegistry(CameraRecord record)
	{
		s_LogChannel.Log(record.cameraInstance.camera, "Camera {0}({1}) added to camera stack registry", record.cameraInstance.camera, record.cameraStackType);
	}

	private static void LogCameraRemovedFromRegistry(CameraRecord record)
	{
		s_LogChannel.Log(record.cameraInstance.camera, "Camera {0}({1}) removed from camera stack registry", record.cameraInstance.camera, record.cameraStackType);
	}

	private static void LogCameraStackUpdated()
	{
		s_LogChannel.Log("Camera stack updated");
	}

	private static void LogCameraStackStateChanged(CameraStackState cameraStackState)
	{
		s_LogChannel.Log("Camera stack state changed to {0}", cameraStackState);
	}

	private static void LogMissingAdditionalCameraDataError(Camera camera, CameraStackType cameraStackType)
	{
		s_LogChannel.Error(camera, "In order to be added to the stack, camera {0}({1}) must have a WaaaghAdditionalCameraData component", camera, cameraStackType);
	}

	private static void LogCameraInstanceIsAlreadyRegisteredError(Camera camera)
	{
		s_LogChannel.Error(camera, "Camera {0} is already registered", camera);
	}

	private static void LogCameraTypeIsAlreadyRegisteredError(Camera camera, CameraStackType cameraStackType)
	{
		s_LogChannel.Error(camera, "Camera with type {0} is already registered", cameraStackType);
	}
}
