namespace Kingmaker.Controllers.Units.CameraFollow;

public static class CameraFollowTaskFactory
{
	public static ICameraFollowTask GetFollowTask(CameraFollowTaskContext context)
	{
		return new CameraFollowTask(CameraFollowHelper.GetTaskParamsEntry(context.Params, context.IsMelee), context.Owner, context.Position);
	}

	public static ICameraFollowTask GetScrollToTask(CameraFollowTaskContext context)
	{
		return new CameraScrollToTask(CameraFollowHelper.GetTaskParamsEntry(context.Params, context.IsMelee), context.Owner, context.Position, context.Priority.GetValueOrDefault());
	}

	public static ICameraFollowTask GetActionCameraTask(CameraFollowTaskContext context)
	{
		return new ActionCameraTask(context.Caster, context.Target, context.Priority.GetValueOrDefault());
	}

	public static ICameraFollowTask GetDoNothingWaitTask(ICameraFollowTask anotherTask)
	{
		return new DoNothingWait(anotherTask.TaskParams, anotherTask.Owner, anotherTask.Priority);
	}
}
