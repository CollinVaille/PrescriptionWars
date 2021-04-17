using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public List<AudioClip> songs;

    private static List<int> songsQueued = new List<int>();
    private static List<int> songsAvailable = new List<int>();

    private static int previousSong = -1;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(!AudioManager.IsMusicPlaying)
        {
            if(songsQueued.Count == 0)
            {
                ShuffleSongs();
            }

            AudioManager.PlaySong(songs[songsQueued[0]]);
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
