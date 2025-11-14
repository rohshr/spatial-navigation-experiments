using UnityEngine;

namespace LandmarkPlacementTest
{
    public class PlacementAudioManager : MonoBehaviour
    {
        public static PlacementAudioManager Instance;
    
        [Header("Global Placement Sounds")]
        public AudioClip defaultPlacementSound;
        public AudioClip defaultPickupSound;
    
        [Header("Specialized Sounds (Optional)")]
        public AudioClip correctPlacementSound;  // Different sound for correct placements
        public AudioClip incorrectPlacementSound; // Different sound for incorrect placements
    
        [Header("Audio Settings")]
        [Range(0f, 1f)]
        public float masterVolume = 0.5f;
        [Range(0f, 1f)]
        public float placementVolume = 1f;
        [Range(0f, 1f)]
        public float pickupVolume = 0.7f;
        [Range(0f, 1f)]
        public float feedbackVolume = 0.8f;
    
        private AudioSource audioSource;
    
        void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        
            SetupAudioSource();
        }
    
        void SetupAudioSource()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        
            audioSource.playOnAwake = false;
            audioSource.volume = masterVolume;
        }
    
        // Play placement sound (called by DraggableObject)
        public void PlayPlacementSound()
        {
            if (defaultPlacementSound != null)
            {
                audioSource.PlayOneShot(defaultPlacementSound, masterVolume * placementVolume);
            }
        }
    
        // Play pickup sound (called by DraggableObject)
        public void PlayPickupSound()
        {
            if (defaultPickupSound != null)
            {
                audioSource.PlayOneShot(defaultPickupSound, masterVolume * pickupVolume);
            }
        }
    
        // Play feedback sounds (called by PlacementChecker)
        public void PlayCorrectSound()
        {
            AudioClip soundToPlay = correctPlacementSound != null ? correctPlacementSound : defaultPlacementSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay, masterVolume * feedbackVolume);
            }
        }
    
        public void PlayIncorrectSound()
        {
            AudioClip soundToPlay = incorrectPlacementSound != null ? incorrectPlacementSound : defaultPlacementSound;
            if (soundToPlay != null)
            {
                audioSource.PlayOneShot(soundToPlay, masterVolume * feedbackVolume * 0.8f); // Slightly quieter
            }
        }
    
        // Custom sound playback
        public void PlayCustomSound(AudioClip clip, float volumeMultiplier = 1f)
        {
            if (clip != null)
            {
                audioSource.PlayOneShot(clip, masterVolume * volumeMultiplier);
            }
        }
    
        // Volume control methods
        public void SetMasterVolume(float volume)
        {
            masterVolume = Mathf.Clamp01(volume);
            audioSource.volume = masterVolume;
        }
    
        public void ToggleMute()
        {
            audioSource.mute = !audioSource.mute;
        }
    
        // Alternative method for objects to use centralized sounds
        public static void PlayPlacement()
        {
            Instance?.PlayPlacementSound();
        }
    
        public static void PlayPickup()
        {
            Instance?.PlayPickupSound();
        }
    }
}