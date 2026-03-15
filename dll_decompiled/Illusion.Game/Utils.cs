using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Illusion.Extensions;
using Manager;
using RootMotion.FinalIK;
using Sound;
using UniRx;
using UniRx.Async;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Illusion.Game;

public static class Utils
{
	public static class Bundle
	{
		public static void LoadSprite(string assetBundleName, string assetName, Image image, bool isTexSize, string spAnimeName = null, string manifest = null, string spAnimeManifest = null)
		{
			AssetBundleLoadAssetOperation assetBundleLoadAssetOperation = AssetBundleManager.LoadAsset(assetBundleName, assetName, typeof(Sprite), manifest);
			Sprite sprite = assetBundleLoadAssetOperation.GetAsset<Sprite>();
			if (sprite == null)
			{
				Texture2D asset = assetBundleLoadAssetOperation.GetAsset<Texture2D>();
				sprite = Sprite.Create(asset, new Rect(0f, 0f, asset.width, asset.height), Vector2.zero);
			}
			image.sprite = sprite;
			RectTransform rectTransform = image.rectTransform;
			Vector2 vector = (isTexSize ? new Vector2(sprite.rect.width, sprite.rect.height) : rectTransform.sizeDelta);
			if (!spAnimeName.IsNullOrEmpty())
			{
				Animator component = image.GetComponent<Animator>();
				component.enabled = true;
				component.runtimeAnimatorController = AssetBundleManager.LoadAsset(assetBundleName, spAnimeName, typeof(RuntimeAnimatorController)).GetAsset<RuntimeAnimatorController>();
				Func<float, float, float> obj = (float x, float y) => x / y;
				Func<float, float, bool> func = (float a, float b) => a > b && Mathf.FloorToInt(a - 1f) > 0;
				float num = obj(vector.x, vector.y);
				float num2 = obj(vector.y, vector.x);
				if (func(num, num2))
				{
					rectTransform.sizeDelta = new Vector2(vector.y, vector.y);
				}
				else if (func(num2, num))
				{
					rectTransform.sizeDelta = new Vector2(vector.x, vector.x);
				}
				else
				{
					rectTransform.sizeDelta = new Vector2(vector.x, vector.y);
				}
				AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: false, spAnimeManifest);
			}
			else
			{
				rectTransform.sizeDelta = new Vector2(vector.x, vector.y);
			}
			AssetBundleManager.UnloadAssetBundle(assetBundleName, isUnloadForceRefCount: false, manifest);
		}
	}

	public static class Layout
	{
		public class EnabledScope : GUI.Scope
		{
			private readonly bool enabled;

			public EnabledScope()
			{
				enabled = GUI.enabled;
			}

			public EnabledScope(bool enabled)
			{
				this.enabled = GUI.enabled;
				GUI.enabled = enabled;
			}

			protected override void CloseScope()
			{
				GUI.enabled = enabled;
			}
		}

		public class ColorScope : GUI.Scope
		{
			private readonly Color[] colors;

			public ColorScope()
			{
				colors = new Color[3]
				{
					GUI.color,
					GUI.backgroundColor,
					GUI.contentColor
				};
			}

			public ColorScope(params Color[] colors)
			{
				this.colors = new Color[3]
				{
					GUI.color,
					GUI.backgroundColor,
					GUI.contentColor
				};
				foreach (var item in colors.Take(this.colors.Length).Select((Color color, int index) => new { color, index }))
				{
					switch (item.index)
					{
					case 0:
						GUI.color = item.color;
						break;
					case 1:
						GUI.backgroundColor = item.color;
						break;
					case 2:
						GUI.contentColor = item.color;
						break;
					}
				}
			}

			public ColorScope(Colors colors)
			{
				this.colors = new Color[3]
				{
					GUI.color,
					GUI.backgroundColor,
					GUI.contentColor
				};
				if (colors.color.HasValue)
				{
					GUI.color = colors.color.Value;
				}
				if (colors.backgroundColor.HasValue)
				{
					GUI.backgroundColor = colors.backgroundColor.Value;
				}
				if (colors.contentColor.HasValue)
				{
					GUI.contentColor = colors.contentColor.Value;
				}
			}

			protected override void CloseScope()
			{
				int num = 0;
				GUI.color = colors[num++];
				GUI.backgroundColor = colors[num++];
				GUI.contentColor = colors[num++];
			}
		}
	}

	public static class ScreenShot
	{
		public static string Path
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder(256);
				stringBuilder.Append(UserData.Create("cap"));
				DateTime now = DateTime.Now;
				stringBuilder.Append(now.Year.ToString("0000"));
				stringBuilder.Append(now.Month.ToString("00"));
				stringBuilder.Append(now.Day.ToString("00"));
				stringBuilder.Append(now.Hour.ToString("00"));
				stringBuilder.Append(now.Minute.ToString("00"));
				stringBuilder.Append(now.Second.ToString("00"));
				stringBuilder.Append(now.Millisecond.ToString("000"));
				stringBuilder.Append(".png");
				return stringBuilder.ToString();
			}
		}

		public static IEnumerator CaptureGSS(List<ScreenShotCamera> ssCamList, string path, Texture capMark, int capRate = 1)
		{
			if (ssCamList.IsNullOrEmpty() || path.IsNullOrEmpty())
			{
				yield break;
			}
			yield return new WaitForEndOfFrame();
			Capture(delegate(RenderTexture rt)
			{
				Graphics.Blit(ssCamList[0].renderTexture, rt);
				foreach (ScreenShotCamera item in ssCamList.Skip(1))
				{
					Graphics.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), item.renderTexture);
				}
				if (capMark != null)
				{
					DrawCapMark(capMark, null);
				}
			}, path, capRate);
			yield return null;
		}

		public static IEnumerator CaptureCameras(List<Camera> cameraList, string path, Texture capMark, int capRate = 1)
		{
			yield return new WaitForEndOfFrame();
			Capture(delegate(RenderTexture rt)
			{
				Graphics.SetRenderTarget(rt);
				GL.Clear(clearDepth: true, clearColor: true, Color.black);
				Graphics.SetRenderTarget(null);
				foreach (Camera item in cameraList.Where((Camera p) => p != null))
				{
					bool enabled = item.enabled;
					RenderTexture targetTexture = item.targetTexture;
					Rect rect = item.rect;
					item.enabled = true;
					item.targetTexture = rt;
					item.Render();
					item.targetTexture = targetTexture;
					item.rect = rect;
					item.enabled = enabled;
				}
				if (capMark != null)
				{
					Graphics.SetRenderTarget(rt);
					DrawCapMark(capMark, null);
					Graphics.SetRenderTarget(null);
				}
			}, path, capRate);
			yield return null;
		}

		public static void Capture(Action<RenderTexture> proc, string path, int capRate = 1)
		{
			int num = ((capRate == 0) ? 1 : capRate);
			Texture2D texture2D = new Texture2D(Screen.width * num, Screen.height * num, TextureFormat.RGB24, mipChain: false);
			RenderTexture temporary = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 24, RenderTextureFormat.Default, RenderTextureReadWrite.Default, (QualitySettings.antiAliasing == 0) ? 1 : QualitySettings.antiAliasing);
			proc(temporary);
			RenderTexture.active = temporary;
			texture2D.ReadPixels(new Rect(0f, 0f, texture2D.width, texture2D.height), 0, 0);
			texture2D.Apply();
			RenderTexture.active = null;
			byte[] bytes = texture2D.EncodeToPNG();
			RenderTexture.ReleaseTemporary(temporary);
			UnityEngine.Object.Destroy(texture2D);
			texture2D = null;
			File.WriteAllBytes(path, bytes);
		}

		public static void DrawCapMark(Texture tex, Vector2? pos)
		{
			float num = (float)Screen.width / 1280f;
			if (!pos.HasValue)
			{
				pos = new Vector2(1152f, 688f);
			}
			Graphics.DrawTexture(new Rect(pos.Value.x * num, pos.Value.y * num, (float)tex.width * num, (float)tex.height * num), tex);
		}
	}

	public static class Sound
	{
		public class SettingBGM : Setting
		{
			public override string bundle
			{
				get
				{
					return base.bundle;
				}
				set
				{
					base.bundle = value;
					base.asset = Path.GetFileNameWithoutExtension(value);
				}
			}

			public SettingBGM()
			{
				Initialize();
			}

			public SettingBGM(int bgmNo)
			{
				Setting(ConvertBundle(bgmNo), ConvertAsset(bgmNo));
			}

			public SettingBGM(BGM bgm)
			{
				Setting(ConvertBundle((int)bgm), ConvertAsset((int)bgm));
			}

			public SettingBGM(string bundle)
			{
				Setting(bundle, bundle);
			}

			private void Setting(string bundle, string asset)
			{
				this.bundle = bundle;
				this.asset = asset;
				Initialize();
			}

			private string ConvertBundle(int bgmNo)
			{
				return SystemBGMCast[(BGM)bgmNo][0];
			}

			private string ConvertAsset(int bgmNo)
			{
				return SystemBGMCast[(BGM)bgmNo][1];
			}

			private void Initialize()
			{
				base.loader.type = Manager.Sound.Type.BGM;
				base.loader.fadeTime = 0.8f;
				base.loader.isAssetEqualPlay = false;
			}
		}

		public class Setting
		{
			public Manager.Sound.Loader loader => _loader;

			private Manager.Sound.Loader _loader { get; } = new Manager.Sound.Loader();

			public virtual string bundle
			{
				get
				{
					return _loader.bundle;
				}
				set
				{
					_loader.bundle = value;
				}
			}

			public virtual string asset
			{
				get
				{
					return _loader.asset;
				}
				set
				{
					_loader.asset = value;
				}
			}

			public void Cast(Manager.Sound.Type type)
			{
				_loader.type = type;
				bundle = SoundBasePath[type];
			}

			public Setting()
			{
			}

			public Setting(SystemSE se)
			{
				Cast(Manager.Sound.Type.SystemSE);
				asset = SystemSECast[se];
			}

			public Setting(Manager.Sound.Type type)
			{
				Cast(type);
			}
		}

		private class SystemSEComparer : IEqualityComparer<SystemSE>
		{
			public bool Equals(SystemSE x, SystemSE y)
			{
				return x == y;
			}

			public int GetHashCode(SystemSE obj)
			{
				return (int)obj;
			}
		}

		private class SoundTypeComparer : IEqualityComparer<Manager.Sound.Type>
		{
			public bool Equals(Manager.Sound.Type x, Manager.Sound.Type y)
			{
				return x == y;
			}

			public int GetHashCode(Manager.Sound.Type obj)
			{
				return (int)obj;
			}
		}

		private class SystemBGMComparer : IEqualityComparer<BGM>
		{
			public bool Equals(BGM x, BGM y)
			{
				return x == y;
			}

			public int GetHashCode(BGM obj)
			{
				return (int)obj;
			}
		}

		public static IReadOnlyDictionary<SystemSE, string> SystemSECast { get; } = new Dictionary<SystemSE, string>(new SystemSEComparer())
		{
			{
				SystemSE.sel,
				"HS2_se_00_03"
			},
			{
				SystemSE.ok_s,
				"HS2_se_00_00"
			},
			{
				SystemSE.ok_l,
				"HS2_se_00_01"
			},
			{
				SystemSE.cancel,
				"HS2_se_00_02"
			},
			{
				SystemSE.photo,
				"HS2_se_00_05"
			},
			{
				SystemSE.save,
				"HS2_se_00_01"
			},
			{
				SystemSE.load,
				"HS2_se_00_load"
			},
			{
				SystemSE.achievement,
				"HS2_se_00_04"
			},
			{
				SystemSE.page,
				"se_07_button_A"
			}
		};

		public static IReadOnlyDictionary<Manager.Sound.Type, string> SoundBasePath { get; } = new Dictionary<Manager.Sound.Type, string>(new SoundTypeComparer())
		{
			{
				Manager.Sound.Type.BGM,
				"sound/data/bgm/bgm_30.unity3d"
			},
			{
				Manager.Sound.Type.ENV,
				"sound/data/env/30.unity3d"
			},
			{
				Manager.Sound.Type.GameSE2D,
				"sound/data/se/30.unity3d"
			},
			{
				Manager.Sound.Type.GameSE3D,
				"sound/data/se/30.unity3d"
			},
			{
				Manager.Sound.Type.SystemSE,
				"sound/data/systemse/30.unity3d"
			}
		};

		public static IReadOnlyDictionary<BGM, string[]> SystemBGMCast { get; } = new Dictionary<BGM, string[]>(new SystemBGMComparer())
		{
			{
				BGM.title,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_00" }
			},
			{
				BGM.myroom,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_01" }
			},
			{
				BGM.lobby,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_13" }
			},
			{
				BGM.custom,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_02" }
			},
			{
				BGM.network,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_04" }
			},
			{
				BGM.state_normal,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_05" }
			},
			{
				BGM.state_favor,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_06" }
			},
			{
				BGM.state_enjoyment,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_07" }
			},
			{
				BGM.state_aversion,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_09" }
			},
			{
				BGM.state_slavery,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_08" }
			},
			{
				BGM.state_broken,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_10" }
			},
			{
				BGM.state_dependence,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_11" }
			},
			{
				BGM.fur,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_03" }
			},
			{
				BGM.japanesemap,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_12" }
			},
			{
				BGM.fashionablemap,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_13" }
			},
			{
				BGM.darkmap,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "hs2_bgm_14" }
			},
			{
				BGM.ai_custom,
				new string[2] { "sound/data/bgm/bgm_30.unity3d", "ai_bgm_10" }
			},
			{
				BGM.title2,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_00" }
			},
			{
				BGM.sitri,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_01" }
			},
			{
				BGM.apeend_favor,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_02" }
			},
			{
				BGM.apeend_enjoyment,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_03" }
			},
			{
				BGM.apeend_aversion,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_05" }
			},
			{
				BGM.apeend_slavery,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_04" }
			},
			{
				BGM.apeend_broken,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_07" }
			},
			{
				BGM.apeend_dependence,
				new string[2] { "sound/data/bgm/bgm_50.unity3d", "hs2a_bgm_06" }
			}
		};

		public static Sound_Component GetBGM()
		{
			if (Manager.Sound.currentBGM != null)
			{
				return Manager.Sound.currentBGM.GetComponent<Sound_Component>();
			}
			return null;
		}

		public static AudioSource Get(Manager.Sound.Type type, AssetBundleData data)
		{
			return Manager.Sound.CreateCache(type, data);
		}

		public static AudioSource Get(Manager.Sound.Type type, AssetBundleManifestData data)
		{
			return Manager.Sound.CreateCache(type, data);
		}

		public static AudioSource Get(Manager.Sound.Type type, string bundle, string asset, string manifest = null)
		{
			return Manager.Sound.CreateCache(type, bundle, asset, manifest);
		}

		public static void Remove(Manager.Sound.Type type, string bundle, string asset, string manifest = null)
		{
			Manager.Sound.ReleaseCache(type, bundle, asset, manifest);
		}

		public static AudioSource Get(SystemSE se)
		{
			return Get(Manager.Sound.Type.SystemSE, SoundBasePath[Manager.Sound.Type.SystemSE], SystemSECast[se]);
		}

		public static void Remove(SystemSE se)
		{
			Remove(Manager.Sound.Type.SystemSE, SoundBasePath[Manager.Sound.Type.SystemSE], SystemSECast[se]);
		}

		public static void Play(SystemSE se)
		{
			AudioSource audioSource = Get(se);
			if (!(audioSource == null))
			{
				audioSource.Play();
			}
		}

		public static AudioSource Play(Manager.Sound.Type type, AudioClip clip, float fadeTime = 0f)
		{
			AudioSource audio = Manager.Sound.Play(type, clip, fadeTime);
			(from __ in audio.UpdateAsObservable()
				where !audio.isPlaying
				select __).Take(1).Subscribe(delegate
			{
				UnityEngine.Object.Destroy(audio.gameObject);
			});
			return audio;
		}

		public static async UniTask GetBGMandVolume(Action<(string, float)> bgmAndVolume)
		{
			(string, float) obj = (string.Empty, 1f);
			Sound_Component now = GetBGM();
			if (now != null)
			{
				AudioSource audioSource = now.GetComponent<AudioSource>();
				FadePlayer fadePlayer = audioSource.GetComponent<FadePlayer>();
				await UniTask.WaitWhile(() => fadePlayer != null && !(fadePlayer.nowState is FadePlayer.Playing));
				obj.Item1 = now.bundle;
				obj.Item2 = audioSource.volume;
			}
			bgmAndVolume?.Invoke(obj);
		}

		public static async UniTask GetFadePlayerWhileNull(string bgm, float volume)
		{
			SettingBGM s = new SettingBGM(bgm);
			AudioSource source = Play(s);
			FadePlayer player = null;
			if (source != null)
			{
				await UniTask.WaitWhile(() => (player = source.GetComponent<FadePlayer>()) == null);
			}
			if (player != null)
			{
				player.currentVolume = volume;
			}
			else
			{
				if (!(Manager.Sound.currentBGM != null))
				{
					return;
				}
				player = Manager.Sound.currentBGM.GetComponent<FadePlayer>();
				if (player != null)
				{
					player.currentVolume = volume;
					return;
				}
				AudioSource component = Manager.Sound.currentBGM.GetComponent<AudioSource>();
				if (component != null)
				{
					component.volume = volume;
				}
			}
		}

		public static bool isPlay(SystemSE se)
		{
			return Manager.Sound.IsPlay(Manager.Sound.Type.SystemSE, SystemSECast[se]);
		}

		public static AudioSource Play(Setting s)
		{
			return Manager.Sound.Play(s.loader);
		}

		public static void Play(Setting s, Action<AudioSource> action)
		{
			Manager.Sound.Play(s.loader, action);
		}
	}

	public static class IKLoader
	{
		public static void Execute(FullBodyBipedIK ik, List<List<string>> dataList)
		{
			Transform[] componentsInChildren = ik.GetComponentsInChildren<Transform>(includeInactive: true);
			int num = 0;
			List<string> list = dataList[num++];
			int num2 = 0;
			ik.solver.IKPositionWeight = float.Parse(list[num2++]);
			ik.solver.iterations = int.Parse(list[num2++]);
			num2 = 0;
			foreach (List<string> item in dataList.Skip(num))
			{
				if (ik.solver.effectors.Length <= num2)
				{
					break;
				}
				num2++;
				int num3 = 0;
				IKEffector eff = ik.solver.GetEffector(Illusion.Utils.Enum<FullBodyBipedEffector>.Cast(item[num3++]));
				if (eff == null)
				{
					continue;
				}
				eff.positionWeight = float.Parse(item[num3++]);
				eff.rotationWeight = float.Parse(item[num3++]);
				string findFrame = item[num3++];
				if (findFrame == "null")
				{
					eff.target = null;
				}
				else
				{
					componentsInChildren.FirstOrDefault((Transform p) => p.name == findFrame).SafeProc(delegate(Transform frame)
					{
						eff.target = frame;
					});
				}
				if (eff.target != null)
				{
					eff.target.localPosition = item[num3++].GetVector3();
					eff.target.localEulerAngles = item[num3++].GetVector3();
				}
			}
			num += num2;
			num2 = 0;
			foreach (List<string> item2 in dataList.Skip(num))
			{
				if (ik.solver.chain.Length <= num2)
				{
					break;
				}
				FBIKChain fBIKChain = ik.solver.chain[num2++];
				int num4 = 0;
				fBIKChain.pull = float.Parse(item2[num4++]);
				fBIKChain.reach = float.Parse(item2[num4++]);
				fBIKChain.push = float.Parse(item2[num4++]);
				fBIKChain.pushParent = float.Parse(item2[num4++]);
				fBIKChain.reachSmoothing = Illusion.Utils.Enum<FBIKChain.Smoothing>.Cast(item2[num4++]);
				fBIKChain.pushSmoothing = Illusion.Utils.Enum<FBIKChain.Smoothing>.Cast(item2[num4++]);
				fBIKChain.bendConstraint.weight = float.Parse(item2[num4++]);
				string findFrame2 = item2[num4++];
				if (findFrame2 == "null")
				{
					fBIKChain.bendConstraint.bendGoal = null;
				}
				else
				{
					fBIKChain.bendConstraint.bendGoal = componentsInChildren.FirstOrDefault((Transform p) => p.name == findFrame2);
				}
				if (fBIKChain.bendConstraint.bendGoal != null)
				{
					fBIKChain.bendConstraint.bendGoal.localPosition = item2[num4++].GetVector3();
					fBIKChain.bendConstraint.bendGoal.localEulerAngles = item2[num4++].GetVector3();
				}
			}
		}
	}

	public static class Scene
	{
		public static void GameEnd(bool isCheck)
		{
			ExitDialog.GameEnd(isCheck);
		}

		public static void ReturnTitle(Action action, bool skipCheck = false, bool isForce = false)
		{
			string sceneName = "Title";
			bool isBaseTitle = Manager.Scene.LoadSceneName == sceneName;
			if (isBaseTitle)
			{
				skipCheck = true;
			}
			if (!skipCheck || isForce)
			{
				ConfirmDialog.Status status = ConfirmDialog.status;
				status.Sentence = "タイトルへ戻りますか？";
				status.Yes = delegate
				{
					GotoTitle();
				};
				status.No = delegate
				{
					Sound.Play(SystemSE.cancel);
				};
				ConfirmDialog.Load();
			}
			else
			{
				GotoTitle();
			}
			void GotoTitle()
			{
				Sound.Play(SystemSE.ok_l);
				if (isBaseTitle)
				{
					Manager.Scene.UnloadAddScene();
					action?.Invoke();
				}
				else
				{
					Manager.Scene.isReturnTitle = true;
					Manager.Scene.UnloadByOrderSceneAsync(sceneName, async delegate(bool isFind)
					{
						if (!isFind)
						{
							await Manager.Scene.LoadReserveAsync(new Manager.Scene.Data
							{
								levelName = sceneName,
								fadeType = FadeCanvas.Fade.In,
								onLoad = delegate
								{
									action?.Invoke();
								}
							}, isLoadingImageDraw: false);
						}
						else
						{
							await Manager.Scene.sceneFadeCanvas.StartFadeAysnc(FadeCanvas.Fade.In);
							action?.Invoke();
						}
					}).Forget();
				}
			}
		}
	}

	public static class UniRx
	{
		public static class FPSCounter
		{
			private const int BufferSize = 5;

			public static IReadOnlyReactiveProperty<float> Current { get; private set; }

			static FPSCounter()
			{
				Current = (from x in (from _ in Observable.EveryUpdate()
						select Time.deltaTime).Buffer(5, 1)
					select 1f / x.Average()).ToReadOnlyReactiveProperty();
			}
		}

		public static void FixPerspectiveObject<T>(T o, Camera camera) where T : UnityEngine.Component
		{
			Transform transform = o.transform;
			Func<float> distance = () => (transform.position - camera.transform.position).magnitude;
			Vector3 baseScale = transform.localScale / distance();
			(from _ in o.UpdateAsObservable().TakeWhile((Unit _) => camera != null)
				select baseScale * distance()).Subscribe(delegate(Vector3 scale)
			{
				transform.localScale = scale;
			});
		}
	}
}
