using Obi;
using UnityEngine;
using UnityEngine.UI;

public class CameraWetEffect : MonoBehaviour
{
	public GameObject mainCamera;

	public Collider mainCameraCol;

	public WetCameraCol SioCameraCol;

	public Image wetImage;

	public AQUAS_Parameters.WetLens wetLens = new AQUAS_Parameters.WetLens();

	private int sprayFrameIndex;

	private Material waterPlaneMaterial;

	[HideInInspector]
	public bool rundown;

	[HideInInspector]
	public float t;

	[SerializeField]
	private ObiSolver solver;

	public bool HitWaterMap { get; set; }

	public bool HitWaterSiru { get; set; }

	public bool HitWaterObi { get; private set; }

	private void Start()
	{
		if (wetImage != null)
		{
			wetImage.enabled = false;
			waterPlaneMaterial = wetImage.material;
			t = wetLens.wetTime + wetLens.dryingTime;
		}
		if (SioCameraCol != null)
		{
			SioCameraCol.CameraWetEffect = this;
		}
		solver.OnCollision += CheckHittingWater;
		HitWaterMap = false;
		HitWaterObi = false;
		HitWaterSiru = false;
	}

	private void CheckHittingWater(object sender, ObiSolver.ObiCollisionEventArgs e)
	{
		if (mainCameraCol == null)
		{
			return;
		}
		for (int i = 0; i < e.contacts.Count; i++)
		{
			if (e.contacts.Data[i].distance < 0.001f && ObiColliderBase.idToCollider.TryGetValue(e.contacts.Data[i].other, out var value) && value == mainCameraCol)
			{
				HitWaterObi = true;
				return;
			}
		}
		HitWaterObi = false;
	}

	private void Update()
	{
		if (!Singleton<CameraWetMapCol>.IsInstance())
		{
			HitWaterMap = false;
		}
		if (HitWaterObi || HitWaterSiru)
		{
			t = 0f;
			sprayFrameIndex = 0;
			rundown = true;
			wetImage.enabled = true;
			waterPlaneMaterial.SetFloat("_Refraction", 1f);
			waterPlaneMaterial.SetFloat("_Transparency", 0.01f);
			return;
		}
		t += Time.deltaTime;
		if (HitWaterMap)
		{
			t = 0f;
			rundown = true;
			wetImage.enabled = true;
			HitWaterMap = false;
		}
		if (rundown)
		{
			sprayFrameIndex = 0;
			NextFrame();
			InvokeRepeating("NextFrame", 1f / wetLens.rundownSpeed, 1f / wetLens.rundownSpeed);
			rundown = false;
		}
		if (t <= wetLens.wetTime)
		{
			waterPlaneMaterial.SetFloat("_Refraction", 1f);
			waterPlaneMaterial.SetFloat("_Transparency", 0.01f);
			return;
		}
		waterPlaneMaterial.SetFloat("_Refraction", Mathf.Lerp(1f, 0f, (t - wetLens.wetTime) / wetLens.dryingTime));
		waterPlaneMaterial.SetFloat("_Transparency", Mathf.Lerp(0.01f, 0f, (t - wetLens.wetTime) / wetLens.dryingTime));
		if ((t - wetLens.wetTime) / wetLens.dryingTime >= 1f)
		{
			wetImage.enabled = false;
		}
	}

	private void NextFrame()
	{
		if (sprayFrameIndex >= wetLens.sprayFrames.Length - 1)
		{
			sprayFrameIndex = 0;
			CancelInvoke("NextFrame");
		}
		waterPlaneMaterial.SetTexture("_CutoutReferenceTexture", wetLens.sprayFramesCutout[sprayFrameIndex]);
		waterPlaneMaterial.SetTexture("_Normal", wetLens.sprayFrames[sprayFrameIndex]);
		sprayFrameIndex++;
	}

	private void ResetFrame()
	{
		sprayFrameIndex = 0;
		CancelInvoke("NextFrame");
		waterPlaneMaterial.SetTexture("_CutoutReferenceTexture", wetLens.sprayFramesCutout[sprayFrameIndex]);
		waterPlaneMaterial.SetTexture("_Normal", wetLens.sprayFrames[sprayFrameIndex]);
	}
}
