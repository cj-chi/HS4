using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Obi;
using UnityEngine;

public class ObiEmitterCtrl : MonoBehaviour
{
	private class Info
	{
		public float Rate { get; private set; }

		public float Wait { get; private set; }

		public float Speed { get; private set; }

		public Info(float _rate, float _wait, float _speed)
		{
			Rate = _rate;
			Wait = _wait;
			Speed = _speed;
		}
	}

	private class SplitInfo
	{
		internal class Param
		{
			internal float Rate { get; private set; }

			internal float Wait { get; private set; }

			internal float[] Speeds { get; private set; }

			internal Param(float _rate, float _wait, float[] _speed)
			{
				Rate = _rate;
				Wait = _wait;
				Speeds = _speed;
			}

			internal float Speed(float _height)
			{
				_height = Mathf.Clamp01(_height);
				float num = 100f;
				int num2 = Speeds.Count();
				if (num2 == 3)
				{
					return MathfEx.RangeEqualOn(0f, _height, 0.5f) ? Mathf.Lerp(Speeds[0], Speeds[1], Mathf.InverseLerp(0f, 0.5f, _height)) : Mathf.Lerp(Speeds[1], Speeds[2], Mathf.InverseLerp(0.5f, 1f, _height));
				}
				return Speeds.SafeGet(0);
			}
		}

		internal Param[] Params { get; private set; }

		public SplitInfo(List<string> _lst)
		{
			int num = 0;
			List<Param> list = new List<Param>();
			while (true)
			{
				string text = _lst.SafeGet(num++);
				if (text.IsNullOrEmpty())
				{
					break;
				}
				float result = 0f;
				if (!float.TryParse(text, out result))
				{
					break;
				}
				text = _lst.SafeGet(num++);
				if (text.IsNullOrEmpty())
				{
					break;
				}
				float result2 = 0f;
				if (!float.TryParse(text, out result2))
				{
					break;
				}
				text = _lst.SafeGet(num++);
				if (text.IsNullOrEmpty())
				{
					break;
				}
				float speed = 0f;
				IEnumerable<(bool, float)> source = from s in text.Split('/')
					select (check: float.TryParse(s, out speed), value: speed) into v
					where v.check
					select v;
				if (source.Count() == 0)
				{
					break;
				}
				list.Add(new Param(result, result2, source.Select<(bool, float), float>(((bool check, float value) v) => v.value).ToArray()));
			}
			Params = list.ToArray();
		}
	}

	[Serializable]
	public class SetupInfo
	{
		public string emitterBundle = "";

		public string emitterFile = "";

		public string emitterManifest = "";

		public string collisionBundle = "";

		public string collisionFile = "";

		public string collisionManifest = "";

		public float speed = -1f;

		public float lifespan = -1f;

		public float randomVelocity = -1f;

		public int numParticles = -1;

		public float shapeRadius = -1f;

		public float particleRadius = -1f;

		public SetupInfo()
		{
		}

		public SetupInfo(string _emitterBundle = "", string _emitterFile = "", string _emitterManifest = "", string _collisionBundle = "", string _collisionFile = "", string _collisionManifest = "", float _speed = -1f, float _lifespan = -1f, float _randomVelocity = -1f, int _numParticles = -1, float _shapeRadius = -1f, float _particleRadius = -1f)
		{
			emitterBundle = _emitterBundle;
			emitterFile = _emitterFile;
			emitterManifest = _emitterManifest;
			collisionBundle = _collisionBundle;
			collisionFile = _collisionFile;
			collisionManifest = _collisionManifest;
			speed = _speed;
			lifespan = _lifespan;
			randomVelocity = _randomVelocity;
			numParticles = _numParticles;
			shapeRadius = _shapeRadius;
			particleRadius = _particleRadius;
		}

		public SetupInfo(IEnumerable<string> enumerable)
		{
			int num = 0;
			emitterBundle = enumerable.ElementAtOrDefault(num++);
			emitterFile = enumerable.ElementAtOrDefault(num++);
			emitterManifest = enumerable.ElementAtOrDefault(num++);
			collisionBundle = enumerable.ElementAtOrDefault(num++);
			collisionFile = enumerable.ElementAtOrDefault(num++);
			collisionManifest = enumerable.ElementAtOrDefault(num++);
			if (!float.TryParse(enumerable.ElementAtOrDefault(num++), out speed))
			{
				speed = -1f;
			}
			if (!float.TryParse(enumerable.ElementAtOrDefault(num++), out lifespan))
			{
				lifespan = -1f;
			}
			if (!float.TryParse(enumerable.ElementAtOrDefault(num++), out randomVelocity))
			{
				randomVelocity = -1f;
			}
			if (!int.TryParse(enumerable.ElementAtOrDefault(num++), out numParticles))
			{
				numParticles = -1;
			}
			if (!float.TryParse(enumerable.ElementAtOrDefault(num++), out shapeRadius))
			{
				shapeRadius = -1f;
			}
			if (!float.TryParse(enumerable.ElementAtOrDefault(num++), out particleRadius))
			{
				particleRadius = -1f;
			}
		}
	}

	[SerializeField]
	private ObiEmitter obiEmitter;

	[SerializeField]
	private ObiParticleRenderer obiParticleRenderer;

	[SerializeField]
	private ObiEmitterShape obiEmitterShape;

	private List<SplitInfo> splitInfos = new List<SplitInfo>();

	private Coroutine coroutineNow;

	public ObiEmitter ObiEmitter => obiEmitter;

	public ObiParticleRenderer ObiParticleRenderer => obiParticleRenderer;

	public float Speed
	{
		set
		{
			obiEmitter.speed = value;
		}
	}

	public float Lifespan
	{
		set
		{
			obiEmitter.lifespan = value;
		}
	}

	public float RandomVelocity
	{
		set
		{
			obiEmitter.randomVelocity = Mathf.Clamp(value, 0f, 1f);
		}
	}

	public int NumParticles
	{
		set
		{
			obiEmitter.NumParticles = value;
		}
	}

	public float ShapeRadius
	{
		set
		{
			if (obiEmitterShape == null)
			{
				obiEmitterShape = GetComponent<ObiEmitterShape>();
			}
			ObiEmitterShapeDisk obiEmitterShapeDisk = obiEmitterShape as ObiEmitterShapeDisk;
			if (obiEmitterShapeDisk != null)
			{
				obiEmitterShapeDisk.radius = value;
			}
		}
	}

	public float ParticleRadius
	{
		set
		{
			obiParticleRenderer.radiusScale = value;
		}
	}

	public void LoadFile(string _assetBundle, string _assetFile, string _manifest = "")
	{
		splitInfos.Clear();
		ExcelData excelData = LoadExcelData(_assetBundle, _assetFile, "");
		if (excelData == null)
		{
			return;
		}
		foreach (List<string> item in from v in excelData.list.Skip(2)
			select v.list)
		{
			float result = 0f;
			if (float.TryParse(item.SafeGet(0), out result))
			{
				splitInfos.Add(new SplitInfo(item));
			}
		}
	}

	public void Setup(string _emitterBundle, string _emitterFile, string _emitterManifest, string _collisionBundle, string _collisionFile, string _collisionManifest, float _speed, float _lifespan, float _randomVelocity, int _numParticles = -1, float _shapeRadius = -1f, float _particleRadius = -1f)
	{
		if (!_emitterBundle.IsNullOrEmpty() && !_emitterFile.IsNullOrEmpty())
		{
			LoadEmitterMaterial(_emitterBundle, _emitterFile, _emitterManifest);
		}
		if (!_collisionBundle.IsNullOrEmpty() && !_collisionFile.IsNullOrEmpty())
		{
			LoadCollisionMaterial(_collisionBundle, _collisionFile, _collisionManifest);
		}
		if (_speed > 0f)
		{
			Speed = _speed;
		}
		if (_lifespan > 0f)
		{
			Lifespan = _lifespan;
		}
		if (_randomVelocity > 0f)
		{
			RandomVelocity = _randomVelocity;
		}
		if (_numParticles > 0)
		{
			NumParticles = _numParticles;
		}
		if (_shapeRadius > 0f)
		{
			ShapeRadius = _shapeRadius;
		}
		if (_particleRadius > 0f)
		{
			ParticleRadius = _particleRadius;
		}
	}

	public void Setup(SetupInfo _setupInfo)
	{
		if (_setupInfo != null)
		{
			Setup(_setupInfo.emitterBundle, _setupInfo.emitterFile, _setupInfo.emitterManifest, _setupInfo.collisionBundle, _setupInfo.collisionFile, _setupInfo.collisionManifest, _setupInfo.speed, _setupInfo.lifespan, _setupInfo.randomVelocity, _setupInfo.numParticles, _setupInfo.shapeRadius, _setupInfo.particleRadius);
		}
	}

	public void LoadEmitterMaterial(string _assetBundle, string _assetFile, string _manifest = "")
	{
		ObiEmitterMaterial obiEmitterMaterial = CommonLib.LoadAsset<ObiEmitterMaterial>(_assetBundle, _assetFile, clone: true, _manifest);
		if (!(obiEmitterMaterial == null))
		{
			obiEmitter.EmitterMaterial = obiEmitterMaterial;
		}
	}

	public void LoadCollisionMaterial(string _assetBundle, string _assetFile, string _manifest = "")
	{
		ObiCollisionMaterial obiCollisionMaterial = CommonLib.LoadAsset<ObiCollisionMaterial>(_assetBundle, _assetFile, clone: true, _manifest);
		if (!(obiCollisionMaterial == null))
		{
			obiEmitter.CollisionMaterial = obiCollisionMaterial;
		}
	}

	public void Play(int _idx = -1, float _height = 0.5f)
	{
		if (coroutineNow != null)
		{
			StopCoroutine(coroutineNow);
		}
		if (splitInfos.IsNullOrEmpty())
		{
			coroutineNow = StartCoroutine(PlayCoroutine());
			return;
		}
		int index = (MathfEx.RangeEqualOn(0, _idx, splitInfos.Count - 1) ? _idx : UnityEngine.Random.Range(0, splitInfos.Count));
		Info[] infos = splitInfos[index].Params.Select((SplitInfo.Param _param) => new Info(_param.Rate, _param.Wait, _param.Speed(_height))).ToArray();
		coroutineNow = StartCoroutine(PlayCoroutine(infos));
	}

	public void Stop()
	{
		if (coroutineNow != null)
		{
			StopCoroutine(coroutineNow);
		}
		coroutineNow = null;
		obiEmitter.playMode = ObiEmitter.PlayMode.Stop;
		obiEmitter.KillAll();
	}

	private IEnumerator PlayCoroutine()
	{
		obiEmitter.KillAll();
		yield return null;
		obiEmitter.playMode = ObiEmitter.PlayMode.Play;
		yield return new WaitWhile(() => obiEmitter.ActiveParticles < obiEmitter.NumParticles);
		obiEmitter.playMode = ObiEmitter.PlayMode.Stop;
	}

	private IEnumerator PlayCoroutine(Info[] infos)
	{
		obiEmitter.KillAll();
		yield return null;
		int index = 0;
		float rate = 0f;
		while (true)
		{
			obiEmitter.playMode = ObiEmitter.PlayMode.Play;
			rate += infos[index].Rate;
			int num = Mathf.FloorToInt(Mathf.Lerp(0f, obiEmitter.NumParticles, rate * 0.1f));
			obiEmitter.speed = infos[index].Speed;
			yield return new WaitWhile(() => obiEmitter.ActiveParticles < obiEmitter.NumParticles && obiEmitter.ActiveParticles < num);
			obiEmitter.playMode = ObiEmitter.PlayMode.Stop;
			if (index + 1 < infos.Length)
			{
				yield return new WaitForSeconds(infos[index].Wait);
				index++;
				continue;
			}
			break;
		}
	}

	private ExcelData LoadExcelData(string _bundlePath, string _fileName, string _manifest)
	{
		return CommonLib.LoadAsset<ExcelData>(_bundlePath, _fileName, clone: false, _manifest);
	}
}
