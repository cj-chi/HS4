namespace ReMotion;

public interface ITween
{
	bool MoveNext(ref float deltaTime, ref float unscaledDeltaTime);
}
