using UnityEngine;

namespace Config;

public class HSystem : BaseSystem
{
	public bool Visible = true;

	public bool Son = true;

	public bool Cloth = true;

	public bool Accessory = true;

	public bool Shoes = true;

	public bool SecondVisible = true;

	public bool SecondSon = true;

	public bool SecondCloth = true;

	public bool SecondAccessory = true;

	public bool SecondShoes = true;

	public int SiruDraw;

	public int UrineDraw;

	public bool _urine;

	public int SioDraw;

	public bool _sio;

	public bool FeelingGauge = true;

	public bool ActionGuide = true;

	public bool InitCamera = true;

	public bool EyeDir0;

	public bool NeckDir0;

	public bool EyeDir1;

	public bool NeckDir1;

	public bool HomeCallConciergeEventSkip;

	public bool SimpleBody;

	private Color colorDefault = new Color(0f, 0f, 1f, 0.5f);

	public Color SilhouetteColor = Color.blue;

	public bool _weakStop;

	private bool _escapeStop;

	public bool Urine
	{
		get
		{
			if (!SaveData.IsAchievementExchangeRelease(8))
			{
				return false;
			}
			return _urine;
		}
		set
		{
			_urine = value;
		}
	}

	public bool Sio
	{
		get
		{
			if (!SaveData.IsAchievementExchangeRelease(8))
			{
				return false;
			}
			return _sio;
		}
		set
		{
			_sio = value;
		}
	}

	public bool WeakStop
	{
		get
		{
			if (!SaveData.IsAchievementExchangeRelease(6))
			{
				return false;
			}
			return _weakStop;
		}
		set
		{
			_weakStop = value;
		}
	}

	public bool EscapeStop
	{
		get
		{
			if (!SaveData.IsAchievementExchangeRelease(7))
			{
				return false;
			}
			return _escapeStop;
		}
		set
		{
			_escapeStop = value;
		}
	}

	public HSystem(string elementName)
		: base(elementName)
	{
	}

	public override void Init()
	{
		Visible = true;
		Son = true;
		Cloth = true;
		Accessory = true;
		Shoes = true;
		SecondVisible = true;
		SecondSon = true;
		SecondCloth = true;
		SecondAccessory = true;
		SecondShoes = true;
		SiruDraw = 0;
		UrineDraw = 0;
		_urine = false;
		SioDraw = 0;
		_sio = false;
		FeelingGauge = true;
		ActionGuide = true;
		InitCamera = true;
		EyeDir0 = false;
		NeckDir0 = false;
		EyeDir1 = false;
		NeckDir1 = false;
		HomeCallConciergeEventSkip = false;
		SimpleBody = false;
		SilhouetteColor = colorDefault;
		_weakStop = false;
		_escapeStop = false;
	}
}
