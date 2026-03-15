using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class MoviePlayerGUI : MonoBehaviour
{
	public VideoPlayer player;

	public AudioSource audioSource;

	public RawImage imgVideo;

	private void Start()
	{
		if (!(player == null) && !(imgVideo == null))
		{
			player.EnableAudioTrack(0, enabled: true);
			player.SetTargetAudioSource(0, audioSource);
			imgVideo.texture = player.texture;
			player.Play();
			audioSource.Play();
		}
	}
}
