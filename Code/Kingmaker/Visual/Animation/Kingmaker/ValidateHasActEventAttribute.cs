using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker.Visual.Animation.Events;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker;

public class ValidateHasActEventAttribute : ValidatingFieldAttribute
{
	public override void ValidateField(object obj, FieldInfo field, ValidationContext context, int parentIndex)
	{
		if (Application.isPlaying)
		{
			return;
		}
		if (field.FieldType.IsUnityCollection())
		{
			ValidateList(obj, field, parentIndex, context);
			return;
		}
		object value = field.GetValue(obj);
		if (value is AnimationClipWrapper)
		{
			ValidateClipWrapper(context, (AnimationClipWrapper)value);
		}
		else if (value is AnimationClip)
		{
			ValidateClip(context, (AnimationClip)value);
		}
	}

	private static void ValidateClipWrapper(ValidationContext context, AnimationClipWrapper clipWrapper)
	{
		if ((bool)clipWrapper && !HasEvent(clipWrapper))
		{
			context.AddError("({0}) does not have PostCommandActEvent", clipWrapper.name);
		}
	}

	public static bool HasEvent(AnimationClipWrapper clipWrapper)
	{
		return clipWrapper.Events.Any((AnimationClipEvent e) => e.GetType() == typeof(AnimationClipEventAct));
	}

	private static void ValidateClip(ValidationContext context, AnimationClip clip)
	{
		if ((bool)clip && !clip.events.Any((AnimationEvent e) => e.functionName == "PostCommandActEvent"))
		{
			context.AddError("({0}) does not have PostCommandActEvent", clip.name);
		}
	}

	private void ValidateList(object obj, FieldInfo field, int parentIndex, ValidationContext context)
	{
		if (!(field.GetValue(obj) is ICollection collection))
		{
			return;
		}
		int num = 0;
		foreach (object item in collection)
		{
			context.CreateChild($"{field.Name}[{num}]", ValidationNodeType.Field, parentIndex);
			if (item is AnimationClipWrapper)
			{
				ValidateClipWrapper(context, (AnimationClipWrapper)item);
			}
			else if (item is AnimationClip)
			{
				ValidateClip(context, (AnimationClip)item);
			}
			num++;
		}
	}
}
