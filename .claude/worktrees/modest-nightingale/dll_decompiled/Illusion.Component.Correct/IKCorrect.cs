namespace Illusion.Component.Correct;

public class IKCorrect : BaseCorrect
{
	public static string[] FrameNames { get; } = new string[13]
	{
		"f_t_hips", "f_t_thigh_L", "f_t_thigh_R", "f_t_shoulder_L", "f_t_shoulder_R", "f_t_arm_L", "f_t_arm_R", "f_t_elbo_L", "f_t_elbo_R", "f_t_knee_L",
		"f_t_knee_R", "f_t_leg_L", "f_t_leg_R"
	};

	public override string[] GetFrameNames => FrameNames;
}
