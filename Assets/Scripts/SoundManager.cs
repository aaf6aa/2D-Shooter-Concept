using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [System.Serializable]
    public struct NamedClip
    {
        public string Name;
        public AudioClip Clip;
    }
    public NamedClip[] clips; // dictionaries don't show up in editor for some reason

    private AudioSource audioSource;

    public void PlayEffect(string effectName)
    {
        AudioSource.PlayClipAtPoint(clips.FirstOrDefault(x => x.Name == effectName).Clip, new Vector2());
    }

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }
}
