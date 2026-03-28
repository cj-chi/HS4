using PlayfulSystems.LoadingScreen;
using UnityEngine;
using UnityEngine.UI;

public class LoadingScreenPro : LoadingScreenProBase
{
	[Header("Scene Info")]
	public Text sceneInfoHeader;

	public Text sceneInfoDescription;

	public Image sceneInfoImage;

	private const string scenePreviewPath = "ScenePreviews/";

	[Header("Game Tips")]
	public Text tipHeader;

	public Text tipDescription;

	[Header("Fade Settings")]
	public bool doFade = true;

	public float fadeInDuration = 1f;

	public float fadeOutDuration = 1f;

	public Color fadeFromColor = Color.black;

	public Color fadeToColor = Color.black;

	private CameraFade fade;

	[Header("Loading Visuals")]
	[Tooltip("A canvas group and parent for all graphics to show during loading. Leave empty if you want no fading.")]
	public CanvasGroup loadingCanvasGroup;

	[Tooltip("Progress Bar Pro that will filled as the target scene loads.")]
	public ProgressBarPro progressBar;

	[Tooltip("Fillable image that will be filled as the target scene loads.")]
	public Image loadingBar;

	public Text loadingText;

	[Tooltip("#progress# will be replaced with the loading progress from 0 to 100.")]
	public string loadingString = "#progress#%";

	[Header("Confirmation Visuals")]
	[Tooltip("A canvas group and parent for all graphics to show once loading is done. Leave empty if you want no fading.")]
	public CanvasGroup confirmationCanvasGroup;

	protected override void Init()
	{
		if (doFade)
		{
			fade = base.gameObject.AddComponent<CameraFade>();
			fade.Init();
		}
	}

	protected override void DisplaySceneInfo(SceneInfo info)
	{
		SetTextIfStringIsNotNull(sceneInfoHeader, info?.header);
		SetTextIfStringIsNotNull(sceneInfoDescription, info?.description);
		if (sceneInfoImage != null && info != null && !string.IsNullOrEmpty(info.imageName))
		{
			sceneInfoImage.sprite = Resources.Load<Sprite>("ScenePreviews/" + info.imageName);
			AspectRatioFitter component = sceneInfoImage.GetComponent<AspectRatioFitter>();
			if (component != null && sceneInfoImage.sprite != null)
			{
				component.aspectRatio = (float)sceneInfoImage.sprite.texture.width / (float)sceneInfoImage.sprite.texture.height;
			}
		}
	}

	protected override void DisplayGameTip(LoadingTip info)
	{
		SetTextIfStringIsNotNull(tipHeader, info?.header);
		SetTextIfStringIsNotNull(tipDescription, info?.description);
	}

	protected override void ShowStartingVisuals()
	{
		if (doFade)
		{
			fade.StartFadeFrom(fadeFromColor, fadeInDuration);
		}
		SetLoadingVisuals(0f);
		ShowGroup(loadingCanvasGroup, show: true, 0f);
		ShowGroup(confirmationCanvasGroup, show: false, 0f);
	}

	protected override void ShowProgressVisuals(float progress)
	{
		SetLoadingVisuals(progress);
	}

	protected override void ShowLoadingDoneVisuals()
	{
		ShowGroup(loadingCanvasGroup, show: false, 0.25f);
		ShowGroup(confirmationCanvasGroup, show: true, 0.25f);
	}

	protected override void ShowEndingVisuals()
	{
		if (doFade)
		{
			fade.StartFadeTo(fadeToColor, fadeOutDuration);
		}
	}

	protected override bool CanShowConfirmation()
	{
		if (progressBar != null)
		{
			return !progressBar.IsAnimating();
		}
		return true;
	}

	protected override bool CanActivateTargetScene()
	{
		if (doFade && fade != null)
		{
			return !fade.IsFading();
		}
		return true;
	}

	private void SetTextIfStringIsNotNull(Text text, string s)
	{
		if (!(text == null))
		{
			if (string.IsNullOrEmpty(s))
			{
				text.gameObject.SetActive(value: false);
				return;
			}
			text.gameObject.SetActive(value: true);
			text.text = s;
		}
	}

	private void ShowGroup(CanvasGroup group, bool show, float fadeDuration)
	{
		if (!(group == null))
		{
			CanvasGroupFade canvasGroupFade = group.GetComponent<CanvasGroupFade>();
			if (canvasGroupFade == null)
			{
				canvasGroupFade = group.gameObject.AddComponent<CanvasGroupFade>();
			}
			if (canvasGroupFade != null)
			{
				canvasGroupFade.FadeAlpha(show ? 0f : 1f, show ? 1f : 0f, fadeDuration);
			}
		}
	}

	private void SetLoadingVisuals(float progress)
	{
		if (progressBar != null)
		{
			progressBar.Value = progress;
		}
		if (loadingBar != null)
		{
			loadingBar.fillAmount = progress;
		}
		if (loadingText != null)
		{
			loadingText.text = loadingString.Replace("#progress#", Mathf.RoundToInt(progress * 100f).ToString());
		}
	}
}
