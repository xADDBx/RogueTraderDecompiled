using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.AreaLogic.SceneControllables;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.Visual.FX;

[ExecuteAlways]
[HelpURL("https://confluence.owlcat.local/display/WH40K/NecronDissolveGroup")]
public class NecronDissolveGroup : ControllableComponent, IAreaActivationHandler, ISubscriber
{
	[Serializable]
	public class MaterialOverride
	{
		public Renderer Renderer;

		public int SubmeshIndex;

		public Material Original;

		public Material Dissolve;
	}

	public const float MinDissolveValue = -30f;

	public const float MaxDissolveValue = 30f;

	private const float DefaultVisibleValue = -10f;

	private const float DefaultDissolvedValue = 10f;

	[Tooltip("DissolveMoveValue applied on first OnEnable (load). -10 = visible, +10 = fully dissolved.")]
	[SerializeField]
	[Range(-30f, 30f)]
	private float _initialValue = 10f;

	[Tooltip("DissolveMoveValue for Show action (object visible). Typically -10.")]
	[SerializeField]
	[Range(-30f, 30f)]
	private float _visibleValue = -10f;

	[Tooltip("DissolveMoveValue for Hide action (object dissolved). Typically +10.")]
	[SerializeField]
	[Range(-30f, 30f)]
	private float _dissolvedValue = 10f;

	[Tooltip("Template material with NecronDissolve shader; Bake copies DissolveTex/Noise/AlphaBorder/Glow from here")]
	[SerializeField]
	private Material _dissolveTemplate;

	[Tooltip("Only materials with this shader will be replaced by Bake. Default: owlcat/lit")]
	[SerializeField]
	private Shader _sourceShader;

	[Tooltip("Renderers under this transform, populated by Bake / Refresh Renderer Cache in Editor")]
	[SerializeField]
	private Renderer[] _cachedRenderers;

	[Tooltip("Mapping original->dissolve materials, written by Bake for Revert support")]
	[SerializeField]
	private List<MaterialOverride> _overrides = new List<MaterialOverride>();

	[Tooltip("Default tween duration if catscene action does not specify one (Duration <= 0). Also used by polling fallback.")]
	[SerializeField]
	private float _defaultTweenDuration = 1.5f;

	[Tooltip("Animated by tween. Reflects current dissolve state. -10 = visible, +10 = fully dissolved.")]
	[Range(-30f, 30f)]
	public float DissolveMoveValue = -10f;

	private static readonly int _DissolveMoveId = Shader.PropertyToID("_DissolveMove");

	private MaterialPropertyBlock _block;

	private float _lastApplied = float.NaN;

	private bool _restoreAttempted;

	private int? _lastObservedSavedState;

	private Coroutine _activeTween;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			DissolveMoveValue = _initialValue;
			_restoreAttempted = false;
			_lastObservedSavedState = null;
			EventBus.Subscribe(this);
		}
		Apply();
	}

	protected override void OnDisable()
	{
		if (Application.isPlaying)
		{
			EventBus.Unsubscribe(this);
		}
		base.OnDisable();
	}

	public void OnAreaActivated()
	{
		_restoreAttempted = false;
		_lastObservedSavedState = null;
		DissolveMoveValue = _initialValue;
		if (_activeTween != null)
		{
			StopCoroutine(_activeTween);
			_activeTween = null;
		}
		Apply();
	}

	public override void SetState(ControllableState state)
	{
		base.SetState(state);
	}

	public void AnimateTo(NecronDissolveAction action, float duration)
	{
		_lastObservedSavedState = (int)action;
		if (Game.Instance != null && Game.Instance.SceneControllables != null && !string.IsNullOrEmpty(UniqueId))
		{
			Game.Instance.SceneControllables.SetState(UniqueId, new ControllableState
			{
				State = (int)action
			});
		}
		float duration2 = ((duration > 0f) ? duration : _defaultTweenDuration);
		ApplyAction(action, duration2);
	}

	private void Update()
	{
		if (Application.isPlaying && IsAreaReady())
		{
			if (!_restoreAttempted)
			{
				_restoreAttempted = true;
				TryRestoreFromSavedState();
			}
			else
			{
				PollSavedStateChange();
			}
		}
		if (_lastApplied != DissolveMoveValue)
		{
			Apply();
		}
	}

	private void PollSavedStateChange()
	{
		if (string.IsNullOrEmpty(UniqueId) || Game.Instance == null || Game.Instance.SceneControllables == null || !Game.Instance.SceneControllables.TryGetState(UniqueId, out var state))
		{
			return;
		}
		int? num = state?.State;
		if (num != _lastObservedSavedState)
		{
			_lastObservedSavedState = num;
			if (num.HasValue)
			{
				NecronDissolveAction value = (NecronDissolveAction)num.Value;
				ApplyAction(value, _defaultTweenDuration);
			}
		}
	}

	private void TryRestoreFromSavedState()
	{
		if (!string.IsNullOrEmpty(UniqueId) && Game.Instance != null && Game.Instance.SceneControllables != null && Game.Instance.SceneControllables.TryGetState(UniqueId, out var state))
		{
			_lastObservedSavedState = state?.State;
			if (state != null && state.State.HasValue)
			{
				NecronDissolveAction value = (NecronDissolveAction)state.State.Value;
				ApplyAction(value, 0f);
			}
		}
	}

	private void ApplyAction(NecronDissolveAction action, float duration)
	{
		float targetValue = GetTargetValue(action);
		if (_activeTween != null)
		{
			StopCoroutine(_activeTween);
			_activeTween = null;
		}
		if (duration <= 0f)
		{
			DissolveMoveValue = targetValue;
		}
		else
		{
			_activeTween = StartCoroutine(TweenDissolveValue(DissolveMoveValue, targetValue, duration));
		}
	}

	private float GetTargetValue(NecronDissolveAction action)
	{
		if (action != NecronDissolveAction.Hide)
		{
			return _visibleValue;
		}
		return _dissolvedValue;
	}

	private IEnumerator TweenDissolveValue(float from, float to, float duration)
	{
		float t = 0f;
		while (t < duration)
		{
			t += Time.deltaTime;
			DissolveMoveValue = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
			yield return null;
		}
		DissolveMoveValue = to;
		_activeTween = null;
	}

	private static bool IsAreaReady()
	{
		Game instance = Game.Instance;
		if (instance == null)
		{
			return false;
		}
		if (instance.LoadedAreaState == null || instance.LoadedAreaState.MainState == null)
		{
			return false;
		}
		return true;
	}

	private void Apply()
	{
		if (_cachedRenderers == null || _cachedRenderers.Length == 0)
		{
			return;
		}
		if (_block == null)
		{
			_block = new MaterialPropertyBlock();
		}
		_block.SetFloat(_DissolveMoveId, DissolveMoveValue);
		Renderer[] cachedRenderers = _cachedRenderers;
		foreach (Renderer renderer in cachedRenderers)
		{
			if (renderer != null)
			{
				renderer.SetPropertyBlock(_block);
			}
		}
		_lastApplied = DissolveMoveValue;
	}
}
