using UnityEngine;

namespace KriptoFX.BloodFX;

internal class UVTextureAnimator : MonoBehaviour
{
	public int Rows = 4;

	public int Columns = 4;

	public float Fps = 20f;

	public int OffsetMat;

	public float StartDelay;

	public bool IsInterpolateFrames = true;

	public BFX_TextureShaderProperties[] TextureNames = new BFX_TextureShaderProperties[1];

	public AnimationCurve FrameOverTime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	private Renderer currentRenderer;

	private Projector projector;

	private Material instanceMaterial;

	private float animationStartTime;

	private bool canUpdate;

	private int previousIndex;

	private int totalFrames;

	private float currentInterpolatedTime;

	private int currentIndex;

	private Vector2 size;

	private bool isInitialized;

	private bool startDelayIsBroken;

	private void OnEnable()
	{
		if (isInitialized)
		{
			InitDefaultVariables();
		}
	}

	private void Start()
	{
		InitDefaultVariables();
		isInitialized = true;
	}

	private void Update()
	{
		if (startDelayIsBroken)
		{
			ManualUpdate();
		}
	}

	private void ManualUpdate()
	{
		if (canUpdate)
		{
			UpdateMaterial();
			SetSpriteAnimation();
			if (IsInterpolateFrames)
			{
				SetSpriteAnimationIterpolated();
			}
		}
	}

	private void StartDelayFunc()
	{
		startDelayIsBroken = true;
		animationStartTime = Time.time;
	}

	private void InitDefaultVariables()
	{
		InitializeMaterial();
		totalFrames = Columns * Rows;
		previousIndex = 0;
		canUpdate = true;
		Vector3 zero = Vector3.zero;
		size = new Vector2(1f / (float)Columns, 1f / (float)Rows);
		animationStartTime = Time.time;
		if (StartDelay > 1E-05f)
		{
			startDelayIsBroken = false;
			Invoke("StartDelayFunc", StartDelay);
		}
		else
		{
			startDelayIsBroken = true;
		}
		if (instanceMaterial != null)
		{
			BFX_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				BFX_TextureShaderProperties bFX_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(bFX_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(bFX_TextureShaderProperties.ToString(), zero);
			}
		}
	}

	private void InitializeMaterial()
	{
		GetComponent<MeshRenderer>().enabled = true;
		currentRenderer = GetComponent<Renderer>();
		if (currentRenderer == null)
		{
			projector = GetComponent<Projector>();
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void UpdateMaterial()
	{
		if (currentRenderer == null)
		{
			if (projector != null)
			{
				if (!projector.material.name.EndsWith("(Instance)"))
				{
					projector.material = new Material(projector.material)
					{
						name = projector.material.name + " (Instance)"
					};
				}
				instanceMaterial = projector.material;
			}
		}
		else
		{
			instanceMaterial = currentRenderer.material;
		}
	}

	private void SetSpriteAnimation()
	{
		int num = (int)((Time.time - animationStartTime) * Fps);
		num %= totalFrames;
		if (num < previousIndex)
		{
			canUpdate = false;
			GetComponent<MeshRenderer>().enabled = false;
			return;
		}
		if (IsInterpolateFrames && num != previousIndex)
		{
			currentInterpolatedTime = 0f;
		}
		previousIndex = num;
		int num2 = num % Columns;
		int num3 = num / Columns;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 value = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			BFX_TextureShaderProperties[] textureNames = TextureNames;
			for (int i = 0; i < textureNames.Length; i++)
			{
				BFX_TextureShaderProperties bFX_TextureShaderProperties = textureNames[i];
				instanceMaterial.SetTextureScale(bFX_TextureShaderProperties.ToString(), size);
				instanceMaterial.SetTextureOffset(bFX_TextureShaderProperties.ToString(), value);
			}
		}
	}

	private void SetSpriteAnimationIterpolated()
	{
		currentInterpolatedTime += Time.deltaTime;
		int num = previousIndex + 1;
		if (num == totalFrames)
		{
			num = previousIndex;
		}
		int num2 = num % Columns;
		int num3 = num / Columns;
		float x = (float)num2 * size.x;
		float y = 1f - size.y - (float)num3 * size.y;
		Vector2 vector = new Vector2(x, y);
		if (instanceMaterial != null)
		{
			instanceMaterial.SetVector("_Tex_NextFrame", new Vector4(size.x, size.y, vector.x, vector.y));
			instanceMaterial.SetFloat("InterpolationValue", Mathf.Clamp01(currentInterpolatedTime * Fps));
		}
	}
}
