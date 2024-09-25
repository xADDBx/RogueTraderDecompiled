using System.Collections.Generic;
using Owlcat.Runtime.UI.ConsoleTools.ClickHandlers;
using Owlcat.Runtime.UI.ConsoleTools.NavigationTool;
using UnityEngine;

namespace Owlcat.Runtime.UI.ConsoleTools;

public static class ConsoleEntityExtensions
{
	public static void SetFocused(this IConsoleEntity entity, bool value)
	{
		if (entity.TryGetInterface<IConsoleNavigationEntity>(out var implementation))
		{
			implementation.SetFocus(value);
		}
	}

	public static bool IsValid(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IConsoleNavigationEntity>(out var implementation))
		{
			return implementation.IsValid();
		}
		return false;
	}

	public static bool IsSelected(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IConsoleNavigationEntity>(out var implementation))
		{
			return implementation.IsSelected();
		}
		return false;
	}

	public static Vector2 GetPosition(this IConsoleEntity entity)
	{
		if (!entity.TryGetInterface<IFloatConsoleNavigationEntity>(out var implementation))
		{
			return Vector2.zero;
		}
		return implementation.GetPosition();
	}

	public static List<IFloatConsoleNavigationEntity> GetNeighbours(this IConsoleEntity entity)
	{
		if (!entity.TryGetInterface<IFloatConsoleNavigationEntity>(out var implementation))
		{
			return null;
		}
		return implementation.GetNeighbours();
	}

	public static bool HandleUp(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<INavigationUpDirectionHandler>(out var implementation))
		{
			return implementation.HandleUp();
		}
		return false;
	}

	public static bool HandleDown(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<INavigationDownDirectionHandler>(out var implementation))
		{
			return implementation.HandleDown();
		}
		return false;
	}

	public static bool HandleLeft(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<INavigationLeftDirectionHandler>(out var implementation))
		{
			return implementation.HandleLeft();
		}
		return false;
	}

	public static bool HandleRight(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<INavigationRightDirectionHandler>(out var implementation))
		{
			return implementation.HandleRight();
		}
		return false;
	}

	public static bool HandleVector(this IConsoleEntity entity, Vector2 vector)
	{
		if (entity.TryGetInterface<INavigationVectorDirectionHandler>(out var implementation))
		{
			return implementation.HandleVector(vector);
		}
		return false;
	}

	public static bool CanConfirmClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IConfirmClickHandler>(out var implementation))
		{
			return implementation.CanConfirmClick();
		}
		return false;
	}

	public static string GetConfirmClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IConfirmClickHandler>(out var implementation))
		{
			return implementation.GetConfirmClickHint();
		}
		return string.Empty;
	}

	public static void OnConfirmClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IConfirmClickHandler>(out var implementation))
		{
			implementation.OnConfirmClick();
		}
	}

	public static bool CanLongConfirmClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongConfirmClickHandler>(out var implementation))
		{
			return implementation.CanLongConfirmClick();
		}
		return false;
	}

	public static string GetLongConfirmClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongConfirmClickHandler>(out var implementation))
		{
			return implementation.GetLongConfirmClickHint();
		}
		return string.Empty;
	}

	public static void OnLongConfirmClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongConfirmClickHandler>(out var implementation))
		{
			implementation.OnLongConfirmClick();
		}
	}

	public static bool CanDeclineClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IDeclineClickHandler>(out var implementation))
		{
			return implementation.CanDeclineClick();
		}
		return false;
	}

	public static string GetDeclineClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IDeclineClickHandler>(out var implementation))
		{
			return implementation.GetDeclineClickHint();
		}
		return string.Empty;
	}

	public static void OnDeclineClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IDeclineClickHandler>(out var implementation))
		{
			implementation.OnDeclineClick();
		}
	}

	public static bool CanLongDeclineClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongDeclineClickHandler>(out var implementation))
		{
			return implementation.CanLongDeclineClick();
		}
		return false;
	}

	public static string GetLongDeclineClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongDeclineClickHandler>(out var implementation))
		{
			return implementation.GetLongDeclineClickHint();
		}
		return string.Empty;
	}

	public static void OnLongDeclineClick(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongDeclineClickHandler>(out var implementation))
		{
			implementation.OnLongDeclineClick();
		}
	}

	public static bool CanFunc01Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc01ClickHandler>(out var implementation))
		{
			return implementation.CanFunc01Click();
		}
		return false;
	}

	public static string GetFunc01ClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc01ClickHandler>(out var implementation))
		{
			return implementation.GetFunc01ClickHint();
		}
		return string.Empty;
	}

	public static void OnFunc01Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc01ClickHandler>(out var implementation))
		{
			implementation.OnFunc01Click();
		}
	}

	public static bool CanLongFunc01Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc01ClickHandler>(out var implementation))
		{
			return implementation.CanLongFunc01Click();
		}
		return false;
	}

	public static string GetLongFunc01ClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc01ClickHandler>(out var implementation))
		{
			return implementation.GetLongFunc01ClickHint();
		}
		return string.Empty;
	}

	public static void OnLongFunc01Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc01ClickHandler>(out var implementation))
		{
			implementation.OnLongFunc01Click();
		}
	}

	public static bool CanFunc02Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc02ClickHandler>(out var implementation))
		{
			return implementation.CanFunc02Click();
		}
		return false;
	}

	public static string GetFunc02ClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc02ClickHandler>(out var implementation))
		{
			return implementation.GetFunc02ClickHint();
		}
		return string.Empty;
	}

	public static void OnFunc02Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<IFunc02ClickHandler>(out var implementation))
		{
			implementation.OnFunc02Click();
		}
	}

	public static bool CanLongFunc02Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc02ClickHandler>(out var implementation))
		{
			return implementation.CanLongFunc02Click();
		}
		return false;
	}

	public static string GetLongFunc02ClickHint(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc02ClickHandler>(out var implementation))
		{
			return implementation.GetLongFunc02ClickHint();
		}
		return string.Empty;
	}

	public static void OnLongFunc02Click(this IConsoleEntity entity)
	{
		if (entity.TryGetInterface<ILongFunc02ClickHandler>(out var implementation))
		{
			implementation.OnLongFunc02Click();
		}
	}

	private static bool TryGetInterface<TInterface>(this IConsoleEntity entity, out TInterface implementation) where TInterface : IConsoleEntity
	{
		if (entity is TInterface val)
		{
			implementation = val;
			return true;
		}
		while (entity is IConsoleEntityProxy consoleEntityProxy)
		{
			entity = consoleEntityProxy.ConsoleEntityProxy;
			if (entity is TInterface val2)
			{
				implementation = val2;
				return true;
			}
		}
		implementation = default(TInterface);
		return false;
	}
}
