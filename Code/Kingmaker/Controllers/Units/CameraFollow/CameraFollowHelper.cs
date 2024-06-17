using Kingmaker.Blueprints.Camera;

namespace Kingmaker.Controllers.Units.CameraFollow;

public static class CameraFollowHelper
{
	public static CameraFollowTaskParams GetDefaultTaskParams()
	{
		return new CameraFollowTaskParams
		{
			DefaultParams = new CameraFollowTaskParamsEntry
			{
				CameraFlyParams = new CameraFlyAnimationParams()
			},
			HasMeleeParams = true,
			MeleeParams = new CameraFollowTaskParamsEntry
			{
				CameraFlyParams = new CameraFlyAnimationParams()
			}
		};
	}

	public static CameraFollowTaskParamsEntry GetTaskParamsEntry(CameraFollowTaskParams taskParams, bool isMelee)
	{
		if (isMelee && taskParams.HasMeleeParams)
		{
			return taskParams.MeleeParams;
		}
		return taskParams.DefaultParams;
	}
}
