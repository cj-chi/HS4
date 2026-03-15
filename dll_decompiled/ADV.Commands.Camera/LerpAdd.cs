namespace ADV.Commands.Camera;

public class LerpAdd : LerpSet
{
	public override string[] ArgsLabel => new string[3] { "Time", "X,Y,Z", "Pitch,Yaw,Roll" };

	public override string[] ArgsDefault => null;

	public override void Calc()
	{
		for (int i = 0; i < 3; i++)
		{
			if (!float.IsNaN(calcPos[i]))
			{
				endPos[i] += calcPos[i];
			}
		}
		for (int j = 0; j < 3; j++)
		{
			if (!float.IsNaN(calcAng[j]))
			{
				endAng[j] += calcAng[j];
			}
		}
	}
}
