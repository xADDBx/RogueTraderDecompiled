using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.QA.Validation;

public class BlueprintValidationHelper
{
	public class Disable : IDisposable
	{
		public Disable()
		{
			s_ValidationDisableCounter++;
		}

		public void Dispose()
		{
			s_ValidationDisableCounter--;
		}
	}

	private static int s_ValidationDisableCounter;

	public static Action RevalidateEvent;

	public static Action<UnityEngine.Object> ForceValidateEvent;

	public static Action<UnityEngine.Object, ValidationContext> ShowValidationWindow;

	public static Action<ScriptableObject, ValidationContext, string> ValidateLocalizationEvent;

	public static bool AllowOnValidate { get; private set; }

	public static bool ValidationEnabled => s_ValidationDisableCounter == 0;

	public static void ValidateLocalization(ScriptableObject asset, ValidationContext context, string path = "")
	{
	}

	public static void Revalidate([CanBeNull] UnityEngine.Object target = null)
	{
	}
}
