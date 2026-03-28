using Config;
using Manager;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace ILSetUtility.TimeUtility;

public class TimeUtility : MonoBehaviour
{
	private float fps;

	[Range(0f, 50f)]
	public float time_scale = 1f;

	private float deltaTime;

	private uint frame_cnt;

	private float time_cnt;

	public bool mode_mem;

	private float memTime;

	private GUIStyle style;

	private GUIStyleState styleState;

	public bool ForceDrawFPS;

	private void Awake()
	{
		fps = 0f;
		time_scale = 1f;
		deltaTime = 0f;
		frame_cnt = 0u;
		time_cnt = 0f;
		mode_mem = false;
		memTime = 0f;
	}

	private void Start()
	{
		style = new GUIStyle();
		style.fontSize = 20;
		styleState = new GUIStyleState();
		styleState.textColor = new Color(1f, 1f, 1f);
		style.normal = styleState;
		DebugSystem debug = Manager.Config.DebugStatus;
		this.UpdateAsObservable().Subscribe(delegate
		{
			if (Input.GetKey(KeyCode.RightShift) && Input.GetKeyDown(KeyCode.Delete))
			{
				debug.FPS = !debug.FPS;
				base.enabled = debug.FPS;
			}
			deltaTime = Time.deltaTime * time_scale;
			if (mode_mem)
			{
				memTime += Time.deltaTime;
			}
			time_cnt += Time.deltaTime;
			frame_cnt++;
			if (1f <= time_cnt)
			{
				fps = (float)frame_cnt / time_cnt;
				frame_cnt = 0u;
				time_cnt = 0f;
			}
		});
		base.enabled = debug.FPS;
	}

	private void OnGUI()
	{
		DebugSystem debugStatus = Manager.Config.DebugStatus;
		if (debugStatus != null && (ForceDrawFPS || debugStatus.FPS))
		{
			GUILayout.BeginVertical("box");
			GUILayout.Label("FPS:" + fps.ToString("000.0"), style);
			GUILayout.EndVertical();
		}
	}

	public void SetTimeScale(float value)
	{
		time_scale = value;
	}

	public float GetTimeScale()
	{
		return time_scale;
	}

	public float GetFps()
	{
		return fps;
	}

	public float GetTime()
	{
		return deltaTime;
	}

	public void ChangeMemoryFlags(bool flags)
	{
		mode_mem = flags;
	}

	public float GetMemoryTime()
	{
		return memTime;
	}
}
