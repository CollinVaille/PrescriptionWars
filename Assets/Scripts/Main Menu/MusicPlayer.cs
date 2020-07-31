using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource musicSource;

    public List<AudioClip> songs;

    static List<int> songsQueued = new List<int>();
    static List<int> songsAvailable = new List<int>();

    static int previousSong = -1;

    // Start is called before the first frame update
    void Start()
    {

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
            previousSong = songsQueued[0];
            songsQueued.RemoveAt(0);
        }
    }

    public void ShuffleSongs()
    {
        songsQueued.Clear();
        songsAvailable.Clear();

        List<int> previousSongPlayed = new List<int>();
        for (int x = 0; x < songs.Count; x++)
        {
            if (x != previousSong)
                songsAvailable.Add(x);
            else
                previousSongPlayed.Add(x);
        }
        foreach(int previousSongIndex in previousSongPlayed)
            songsAvailable.Add(previousSongIndex);

        bool firstSong = true;
        while (songsAvailable.Count != 0)
        {
            int random;
            if (firstSong)
                random = Random.Range(0, songsAvailable.Count - previousSongPlayed.Count);
            else
                random = Random.Range(0, songsAvailable.Count);

            songsQueued.Add(songsAvailable[random]);
            songsAvailable.RemoveAt(random);
            firstSong = false;
        }
    }
}
