using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager instance;

    public AudioSource SFXObject;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void PlayClip(AudioClip audioclip, Transform spawntransform, float volume)
    {
        AudioSource audioSource = Instantiate(SFXObject, spawntransform.position, Quaternion.identity);

        audioSource.clip = audioclip;

        audioSource.volume = volume;

        audioSource.Play();

        float cliplength = audioSource.clip.length;

        Destroy(audioSource.gameObject);

    }

    void PlayRandomClip(AudioClip[] audioclip, Transform spawntransform, float volume)
    {
        int rand = Random.Range(0, audioclip.Length);

        AudioSource audioSource = Instantiate(SFXObject, spawntransform.position, Quaternion.identity);

        audioSource.clip = audioclip[rand];

        audioSource.volume = volume;

        audioSource.Play();

        float cliplength = audioSource.clip.length;

        Destroy(audioSource.gameObject);
    }
}
