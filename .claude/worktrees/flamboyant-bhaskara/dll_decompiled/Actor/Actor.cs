using ADV;
using AIChara;

namespace Actor;

public class Actor : IActor
{
	public ChaFileControl chaFile => _charaData.chaFile;

	private CharaData _charaData { get; }

	public Actor(CharaData charaData)
	{
		_charaData = charaData;
	}
}
