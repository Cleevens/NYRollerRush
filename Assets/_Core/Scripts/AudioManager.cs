// NY ROLLER RUSH - CORE SYSTEM
// Music + one-shot SFX. Clips are optional; assign in inspector or drop under Resources/Audio.

using System.Collections.Generic;
using UnityEngine;

namespace NYRollerRush.Core
{
    public enum SfxId
    {
        Jump,
        Land,
        LaneChange,
        Coin,
        PowerUp,
        NearMiss,
        Crash,
        Button,
        HighScore
    }

    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        public const string MusicResourceFolder = "Audio/Music/";
        public const string SfxResourceFolder = "Audio/SFX/";

        [Header("Music (or Resources/Audio/Music/{neighborhoodId})")]
        [SerializeField] AudioClip musicTimesSquare;
        [SerializeField] AudioClip musicMidtown;
        [SerializeField] AudioClip musicCentralPark;
        [SerializeField] AudioClip musicBrooklynBridge;
        [SerializeField] AudioClip musicSohoChinatown;
        [SerializeField] float musicVolume = 0.45f;

        [Header("SFX (or Resources/Audio/SFX/{name})")]
        [SerializeField] AudioClip sfxJump;
        [SerializeField] AudioClip sfxLand;
        [SerializeField] AudioClip sfxLaneChange;
        [SerializeField] AudioClip sfxCoin;
        [SerializeField] AudioClip sfxPowerUp;
        [SerializeField] AudioClip sfxNearMiss;
        [SerializeField] AudioClip sfxCrash;
        [SerializeField] AudioClip sfxButton;
        [SerializeField] AudioClip sfxHighScore;
        [SerializeField] float sfxVolume = 0.85f;
        [SerializeField] int sfxVoices = 8;

        AudioSource music;
        readonly List<AudioSource> voices = new List<AudioSource>();
        int voiceIndex;
        string currentMusicId;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            music = gameObject.AddComponent<AudioSource>();
            music.loop = true;
            music.playOnAwake = false;
            music.volume = musicVolume;
            int count = Mathf.Clamp(sfxVoices, 2, 16);
            for (int i = 0; i < count; i++)
            {
                var src = gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.loop = false;
                src.volume = sfxVolume;
                voices.Add(src);
            }
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Play(SfxId id)
        {
            var clip = ResolveSfx(id);
            if (clip == null || voices.Count == 0) return;
            var src = voices[voiceIndex];
            voiceIndex = (voiceIndex + 1) % voices.Count;
            src.pitch = Random.Range(0.96f, 1.04f);
            src.PlayOneShot(clip, sfxVolume);
        }

        public void PlayMusicForNeighborhood(string neighborhoodId)
        {
            if (string.IsNullOrEmpty(neighborhoodId) || neighborhoodId == currentMusicId)
                return;
            var clip = ResolveMusic(neighborhoodId);
            currentMusicId = neighborhoodId;
            if (clip == null)
            {
                music.Stop();
                return;
            }

            if (music.clip == clip && music.isPlaying)
                return;
            music.clip = clip;
            music.volume = musicVolume;
            music.Play();
        }

        public void SetMusicPaused(bool paused)
        {
            if (music == null) return;
            if (paused)
                music.Pause();
            else if (music.clip != null)
                music.UnPause();
        }

        public void StopMusic()
        {
            currentMusicId = null;
            if (music != null)
                music.Stop();
        }

        AudioClip ResolveSfx(SfxId id)
        {
            AudioClip assigned = null;
            string resource = null;
            switch (id)
            {
                case SfxId.Jump: assigned = sfxJump; resource = "jump"; break;
                case SfxId.Land: assigned = sfxLand; resource = "land"; break;
                case SfxId.LaneChange: assigned = sfxLaneChange; resource = "lane_change"; break;
                case SfxId.Coin: assigned = sfxCoin; resource = "coin"; break;
                case SfxId.PowerUp: assigned = sfxPowerUp; resource = "powerup"; break;
                case SfxId.NearMiss: assigned = sfxNearMiss; resource = "near_miss"; break;
                case SfxId.Crash: assigned = sfxCrash; resource = "crash"; break;
                case SfxId.Button: assigned = sfxButton; resource = "button"; break;
                case SfxId.HighScore: assigned = sfxHighScore; resource = "high_score"; break;
            }

            if (assigned != null) return assigned;
            return string.IsNullOrEmpty(resource) ? null : Resources.Load<AudioClip>(SfxResourceFolder + resource);
        }

        AudioClip ResolveMusic(string neighborhoodId)
        {
            AudioClip assigned = null;
            switch (neighborhoodId)
            {
                case "times_square": assigned = musicTimesSquare; break;
                case "midtown": assigned = musicMidtown; break;
                case "central_park": assigned = musicCentralPark; break;
                case "brooklyn_bridge": assigned = musicBrooklynBridge; break;
                case "soho_chinatown": assigned = musicSohoChinatown; break;
            }

            if (assigned != null) return assigned;
            return Resources.Load<AudioClip>(MusicResourceFolder + neighborhoodId);
        }
    }
}
