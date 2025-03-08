using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource musicSource; // Référence à l'AudioSource

    void Start()
    {
        // Joue la musique au démarrage
        musicSource.Play();
    }

    // Méthode pour changer le volume
    public void SetVolume(float volume)
    {
        musicSource.volume = volume;
    }

    // Méthode pour arrêter la musique
    public void StopMusic()
    {
        musicSource.Stop();
    }

    // Méthode pour démarrer la musique
    public void PlayMusic()
    {
        musicSource.Play();
    }
}
