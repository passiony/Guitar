using System;
using UnityEngine;
using System.Collections;

public class SongPlayer : MonoBehaviour
{
    public SongData Song;
    private AudioSource m_AudioSource;
    protected float SmoothAudioTime = 0f;
    protected bool AudioStopEventFired = false;
    protected bool WasPlaying = false;
    protected bool IsSongPlaying = false;

    AudioSource Audio
    {
        get
        {
            if (!m_AudioSource) m_AudioSource = GetComponent<AudioSource>();
            return m_AudioSource;
        }
    }

    void Update()
    {
        if (IsPlaying())
        {
            AudioStopEventFired = false;
            WasPlaying = true;
            UpdateSmoothAudioTime();
        }
    }

    protected void OnSongStopped()
    {
        if (!m_AudioSource.clip)
        {
            return;
        }

        //I want to check if the song has finished playing automatically.
        //Sometimes this is triggered when the song is at the end, 
        //and sometimes it has already been reset to the beginning of the song.
        if (Audio.time == Audio.clip.length || (WasPlaying && Audio.time == 0))
        {
            IsSongPlaying = false;
            GetComponent<GuitarGameplay>().OnSongFinished();
        }
    }

    protected void UpdateSmoothAudioTime()
    {
        //Smooth audio time is used because the audio.time has smaller discreet steps and therefore the notes wont move
        //as smoothly. This uses Time.deltaTime to progress the audio time
        SmoothAudioTime += Time.deltaTime;

        if (SmoothAudioTime >= Audio.clip.length)
        {
            SmoothAudioTime = Audio.clip.length;
            OnSongStopped();
        }

        //Sometimes the audio jumps or lags, this checks if the smooth audio time is off and corrects it
        //making the notes jump or lag along with the audio track
        if (IsSmoothAudioTimeOff())
        {
            CorrectSmoothAudioTime();
        }
    }

    protected bool IsSmoothAudioTimeOff()
    {
        //Negative SmoothAudioTime means the songs playback is delayed
        if (SmoothAudioTime < 0)
        {
            return false;
        }

        //Check if my smooth time and the actual audio time are of by 0.1
        return Mathf.Abs(SmoothAudioTime - Audio.time) > 0.1f;
    }

    protected void CorrectSmoothAudioTime()
    {
        SmoothAudioTime = Audio.time;
    }

    public void Play()
    {
        IsSongPlaying = true;

        if (SmoothAudioTime < 0)
        {
            StartCoroutine(PlayDelayed(Mathf.Abs(SmoothAudioTime)));
        }
        else
        {
            Audio.Play();
            SmoothAudioTime = Audio.time;
        }
    }

    protected IEnumerator PlayDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);

        Audio.Play();
    }

    public void Pause()
    {
        IsSongPlaying = false;
        Audio.Pause();
    }

    public void Stop()
    {
        Audio.Stop();
        WasPlaying = false;
        IsSongPlaying = false;
    }

    public bool IsPlaying()
    {
        return IsSongPlaying;
    }

    public void SetSong(SongData song)
    {
        Song = song;
        Audio.time = 0;
        Audio.clip = Song.BackgroundTrack;
        Audio.pitch = 1;

        SmoothAudioTime = MyMath.BeatsToSeconds(-Song.AudioStartBeatOffset, Song.BeatsPerMinute);
    }

    public float GetCurrentBeat(bool songDataEditor = false)
    {
        if (songDataEditor)
        {
            SmoothAudioTime = Audio.time;
        }

        return MyMath.SecondsToBeats(SmoothAudioTime, Song.BeatsPerMinute) - Song.AudioStartBeatOffset;
    }
}