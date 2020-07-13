using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicSource;

    public List<AudioClip> songs;

    static List<int> songsQueued;
    static List<int> songsAvailable;

    // Start is called before the first frame update
    void Start()
    {
        songsQueued = new List<int>();
        songsAvailable = new List<int>();
    }

    // Update is called once per frame
    void Update()
    {
        if(musicSource.isPlaying == false)
        {
            if(songsQueued.Count == 0)
            {
                ShuffleSongs();
            }

            musicSource.clip = songs[songsQueued[0]];
            musicSource.Play();
            songsQueued.RemoveAt(0);
        }
    }

    public void ShuffleSongs()
    {
        songsQueued.Clear();
        songsAvailable.Clear();

        for (int x = 0; x < songs.Count; x++)
        {
            songsAvailable.Add(x);
        }

        while (songsAvailable.Count != 0)
        {
            int random = Random.Range(0, songsAvailable.Count);
            songsQueued.Add(songsAvailable[random]);
            songsAvailable.RemoveAt(random);
        }
    }
}
