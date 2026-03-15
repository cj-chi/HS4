using System.Collections;
using Manager;
using UnityEngine;
using UnityEngine.Audio;

namespace Map;

public class MapEnvSetting : MonoBehaviour
{
	[SerializeField]
	private AudioSource[] audioSources;

	private IEnumerator Start()
	{
		while (!SingletonInitializer<Manager.Sound>.initialized)
		{
			yield return null;
		}
		if (audioSources != null)
		{
			AudioMixerGroup outputAudioMixerGroup = Manager.Sound.Mixer.FindMatchingGroups(Manager.Sound.Type.ENV.ToString())[0];
			AudioSource[] array = audioSources;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].outputAudioMixerGroup = outputAudioMixerGroup;
			}
		}
	}
}
