using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Camera;

[KnowledgeDatabaseID("891de2d8dd1a6f043a664f0c2adc2586")]
public class CutsceneArtController : EntityViewBase, IUpdatable
{
	[SerializeField]
	private Transform m_CameraPivot;

	[SerializeField]
	private GameObject _cutsceneObject;

	[SerializeField]
	private float _lifeTime;

	private float _timeLeft;

	private Action m_OnComplete;

	protected override void Awake()
	{
		base.Awake();
		_cutsceneObject.SetActive(value: false);
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		SetVisible(visible: true, force: true);
	}

	public void OnRun(Action onComplete)
	{
		_timeLeft = 0f;
		CameraRig instance = CameraRig.Instance;
		instance.Camera.transform.SetParent(m_CameraPivot);
		instance.Camera.transform.ResetAll();
		instance.DontForceLookAtTarget = true;
		instance.ChangeListenerParent(m_CameraPivot);
		m_OnComplete = onComplete;
		_cutsceneObject.SetActive(value: true);
		Game.Instance.CustomUpdateController.Add(this);
	}

	public void OnStop()
	{
		CameraRig.Instance.ResetCurrentModeSettings();
		CameraRig.Instance.DontForceLookAtTarget = false;
		_cutsceneObject.SetActive(value: false);
		Game.Instance.CustomUpdateController.Remove(this);
	}

	public void Interrupt()
	{
		if (_cutsceneObject.activeSelf)
		{
			Animator component = _cutsceneObject.GetComponent<Animator>();
			if ((bool)component)
			{
				component.Play(component.GetCurrentAnimatorStateInfo(0).shortNameHash, 0, 1f);
			}
		}
		OnStop();
	}

	void IUpdatable.Tick(float delta)
	{
		_timeLeft += delta;
		if (_timeLeft >= _lifeTime)
		{
			OnStop();
			Action onComplete = m_OnComplete;
			m_OnComplete = null;
			onComplete?.Invoke();
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new CutsceneArtControllerEntity(this));
	}
}
