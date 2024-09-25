using System.Collections.Generic;
using Code.GameCore.AreaLogic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

[TypeId("51c5dfefb75a19a49a0dc708f3a2b76e")]
public class Gate : BlueprintScriptableObject, IEvaluationErrorHandlingPolicyHolder
{
	public enum ActivationModeType
	{
		AllTracks,
		FirstTrack,
		RandomTrack
	}

	public enum SkipTracksModeType
	{
		SignalGate,
		DoNotSignalGate
	}

	[SerializeField]
	private EvaluationErrorHandlingPolicy m_EvaluationErrorHandlingPolicy;

	public Color Color = Color.white;

	[SerializeField]
	private List<Track> m_Tracks = new List<Track>();

	[SerializeField]
	private Operation m_Op;

	[SerializeField]
	private ActivationModeType m_ActivationMode;

	[ShowIf("CanSkipTracks")]
	public SkipTracksModeType WhenTrackIsSkipped = SkipTracksModeType.DoNotSignalGate;

	[Tooltip("Стартовать трэки гейта с задержкой в 1 кадр, как раньше (см. https://confluence.owlcat.local/pages/viewpage.action?pageId=24971382)")]
	public bool PauseForOneFrame;

	public EvaluationErrorHandlingPolicy EvaluationErrorHandlingPolicy => m_EvaluationErrorHandlingPolicy;

	public IReadOnlyList<Track> Tracks => m_Tracks;

	private bool CanSkipTracks => m_ActivationMode != ActivationModeType.AllTracks;

	public List<Track> StartedTracks
	{
		get
		{
			return m_Tracks;
		}
		set
		{
			m_Tracks = value;
		}
	}

	public Operation Op
	{
		get
		{
			return m_Op;
		}
		set
		{
			m_Op = value;
		}
	}

	public ActivationModeType ActivationMode
	{
		get
		{
			return m_ActivationMode;
		}
		set
		{
			m_ActivationMode = value;
		}
	}

	public void SetDirtyUpdateTracks()
	{
		GateExtension.SetGateDirty(new List<ISerializationCallbackReceiver>(m_Tracks), AssetGuid);
	}
}
