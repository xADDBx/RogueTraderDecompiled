using Kingmaker;
using UnityEngine;

public class MainMenuAnimationsController : MonoBehaviour
{
	public Animator FadeOutAnimator;

	public void Awake()
	{
		PFLog.MainMenuLight.Log("MainMenuAnimationsController.Awake()");
	}

	public void Start()
	{
		PFLog.MainMenuLight.Log("MainMenuAnimationsController.Start()");
	}

	public void OnEnable()
	{
		PFLog.MainMenuLight.Log("MainMenuAnimationsController.OnEnable()");
	}

	public void OnDisable()
	{
		PFLog.MainMenuLight.Log("MainMenuAnimationsController.OnDisable()");
	}

	public void OnDestroy()
	{
		PFLog.MainMenuLight.Log("MainMenuAnimationsController.OnDestroy()");
	}

	private void ApplyPpSettings()
	{
	}

	private void Update()
	{
	}
}
