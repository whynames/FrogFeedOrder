using UnityEngine;
using UnityEngine.Audio;
using VContainer;
using System.Collections.Generic;
using System.Linq;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class SoundEffect
    {
        public string id;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [Range(0f, 1f)] public float randomPitchRange = 0f;
        [Range(0f, 1f)] public float randomVolumeRange = 0f;
    }

    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private SoundEffect[] soundEffects;
    private Dictionary<string, SoundEffect> soundEffectMap;

    private void Awake()
    {
        soundEffectMap = soundEffects.ToDictionary(s => s.id);


    }
    private void Start()
    {
        Subscribe();
    }

    public void Subscribe()
    {
        GameEvents.OnBerryCollected += () => PlaySound("berry_collected");
        GameEvents.OnCollectionFinished += () => PlaySound("collection_finished");
    }

    public void PlaySound(string id)
    {
        if (!soundEffectMap.TryGetValue(id, out var sound)) return;

        float randomPitch = Random.Range(-sound.randomPitchRange, sound.randomPitchRange);
        float randomVolume = Random.Range(-sound.randomVolumeRange, sound.randomVolumeRange);
        sfxSource.pitch = sound.pitch + randomPitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume + randomVolume);
    }
}