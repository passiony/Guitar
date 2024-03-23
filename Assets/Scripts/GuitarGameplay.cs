using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GuitarGameplay : MonoBehaviour
{
    public static GuitarGameplay Instance;
    public GameObject NotePrefab;
    public SongData[] Playlist;
    public GamePanel gamePanel;

    //References to important objects or components
    protected GameObject GuitarNeckObject;
    protected KeyboardControl KeyboardControl;
    protected SongPlayer Player;

    protected List<GameObject> NoteObjects;
    protected Color[] Colors;

    //Game state variables
    protected float Score = 0f;
    protected float Multiplier = 1f;
    protected float Streak = 0;
    protected float MaxStreak = 0;
    protected float NumNotesHit = 0;
    protected float NumNotesMissed = 0;

    protected bool[] HasHitNoteOnStringIndexThisFrame;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        //Init references to external objects/components
        KeyboardControl = gameObject.GetComponent<KeyboardControl>();
        GuitarNeckObject = transform.Find("Guitar Neck").gameObject;
        Player = GetComponent<SongPlayer>();
        NoteObjects = new List<GameObject>();

        HasHitNoteOnStringIndexThisFrame = new bool[5];

        //Get string colors from buttons
        UpdateColorsArray();
    }

    void Update()
    {
        if (Player.IsPlaying())
        {
            //Check for ESC and possibly show in game menu
            ShowInGameMenuOnKeypress();

            //No note has been hit int this frame yet
            ResetHasHitNoteOnStringIndexArray();

            UpdateNeckTextureOffset();
            UpdateNotes();

            UpdateGuiScore();
            UpdateGuiMultiplier();
        }
    }

    public void UpdateColorsArray()
    {
        //If colors array is not initialized, do so
        if (Colors == null || Colors.Length != 5)
        {
            Colors = new Color[5];
        }

        //Get all five string buttons, get the colors from their StringButton component and apply it to the meshes and light
        for (int i = 0; i < 5; ++i)
        {
            GameObject stringButton = GameObject.Find("StringButton" + (i + 1));
            Color color = stringButton.GetComponent<StringButton>().Color;

            if (color != Colors[i])
            {
                Colors[i] = color;

                stringButton.transform.Find("Paddle").GetComponent<Renderer>().material.color = color;
                stringButton.transform.Find("Socket").GetComponent<Renderer>().material.color = color;
                stringButton.transform.Find("Light").GetComponent<Light>().color = color;
            }
        }
    }

    public void StartPlaying(int playlistIndex)
    {
        ResetGameStateValues();

        SetInGameUserInterfaceVisibility(true);

        Player.SetSong(Playlist[playlistIndex]);

        CreateNoteObjects();

        Player.Play();
        StartCoroutine("DisplayCountdown");
    }

    public void StopPlaying()
    {
        SetInGameUserInterfaceVisibility(false);

        DestroyNoteObjects();

        StopAllCoroutines();
    }

    protected void SetInGameUserInterfaceVisibility(bool show)
    {
        gamePanel.scoreText.enabled = show;
        gamePanel.multiplierText.enabled = show;
    }

    protected IEnumerator StartAudio(float delay)
    {
        yield return new WaitForSeconds(delay);

        Player.Play();
    }

    protected void CreateNoteObjects()
    {
        NoteObjects.Clear();

        for (int i = 0; i < Player.Song.Notes.Count; ++i)
        {
            //Create note and trail objects
            GameObject note = InstantiateNoteFromPrefab(Player.Song.Notes[i].StringIndex);
            CreateTrailObject(note, Player.Song.Notes[i]);

            //Hide object on start, they will be shown - when appropriate - in the UpdateNotes routine
            note.GetComponent<Renderer>().enabled = false;
            note.SetActive(false);

            NoteObjects.Add(note);
        }
    }

    protected void DestroyNoteObjects()
    {
        for (int i = 0; i < NoteObjects.Count; ++i)
        {
            Destroy(NoteObjects[i]);
        }

        NoteObjects.Clear();
    }

    public SongData[] GetPlaylist()
    {
        return Playlist;
    }

    protected IEnumerator DisplayCountdown()
    {
        //Count down from 4 to 1 and GO at the beginning of a song
        yield return new WaitForSeconds(MyMath.BeatsToSeconds(1f, Player.Song.BeatsPerMinute));
        gamePanel.ShowTimer("3");

        yield return new WaitForSeconds(MyMath.BeatsToSeconds(1f, Player.Song.BeatsPerMinute));
        gamePanel.ShowTimer("2");

        yield return new WaitForSeconds(MyMath.BeatsToSeconds(1f, Player.Song.BeatsPerMinute));
        gamePanel.ShowTimer("1");

        yield return new WaitForSeconds(MyMath.BeatsToSeconds(1f, Player.Song.BeatsPerMinute));
        gamePanel.ShowTimer("Go");
    }

    protected void StopCountdown()
    {
        StopCoroutine("DisplayCountdown");
        gamePanel.ShowTimer("");
    }

    protected void ResetHasHitNoteOnStringIndexArray()
    {
        for (int i = 0; i < 5; ++i)
        {
            HasHitNoteOnStringIndexThisFrame[i] = false;
        }
    }

    protected void ShowInGameMenuOnKeypress()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StopCountdown();
            UIManager.Instance.ShowIngameMenu();
            // GetComponent<InGameMenu>().ShowMenu();
        }
    }

    protected void UpdateNotes()
    {
        for (int i = 0; i < NoteObjects.Count; ++i)
        {
            UpdateNotePosition(i);

            if (IsNoteHit(i))
            {
                HideNote(i);

                Score += 10f * Multiplier;
                Streak++;
                NumNotesHit++;

                if (Streak > MaxStreak)
                {
                    MaxStreak = Streak;
                }

                //Check if there is a trail
                if (Player.Song.Notes[i].Length > 0f)
                {
                    //Handle the trail
                    StartTrailHitRoutineForNote(i);
                }
                else
                {
                    //No trail, just show the fire particles
                    ShowFireParticlesForNote(i);
                }
            }

            if (WasNoteMissed(i))
            {
                HideNote(i);

                Streak = 0;
                Multiplier = 1;

                NumNotesMissed++;
            }
        }
    }

    protected void StartTrailHitRoutineForNote(int index)
    {
        GameObject trail = NoteObjects[index].transform.Find("Trail").gameObject;

        StartCoroutine(TrailHitRoutine(index, trail));
    }

    protected void ShowFireParticlesForNote(int index)
    {
        StartCoroutine(ShowFireParticles(Player.Song.Notes[index].StringIndex));
    }

    protected IEnumerator ShowFireParticles(int stringIndex)
    {
        // KeyboardControl.GetStringButton( stringIndex ).transform.Find( "Flame" ).particleEmitter.ClearParticles();
        // KeyboardControl.GetStringButton( stringIndex ).transform.Find( "Flame" ).particleEmitter.emit = true;
        //
        // //Wait for one frame and disable the emitter again, 
        // //it is set to one shot so everything is emitted on the first frame
        yield return null;
        //
        // KeyboardControl.GetStringButton( stringIndex ).transform.Find( "Flame" ).particleEmitter.emit = false;
    }

    protected IEnumerator TrailHitRoutine(int noteIndex, GameObject trail)
    {
        Note note = Player.Song.Notes[noteIndex];

        //Update the color of the trail
        //Initially it is darkened and now the full bright color is applied
        trail.GetComponent<Renderer>().material.color = Colors[note.StringIndex];

        //Start the spark particles
        // KeyboardControl.GetStringButton( note.StringIndex ).transform.Find( "Sparks" ).particleEmitter.emit = true;
        // KeyboardControl.GetStringButton( note.StringIndex ).transform.Find( "Sparks" ).particleEmitter.renderer.enabled = true;

        Vector3 trailScale = trail.transform.localScale;
        Vector3 trailPosition = trail.transform.localPosition;

        //Do this as long as the button for the specific string is pressed or until the trail reaches its end
        while (KeyboardControl.IsButtonPressed(note.StringIndex) &&
               Player.GetCurrentBeat() + 1 <= note.Time + note.Length)
        {
            //Calculate how far we have progressed in this trail
            float progress = Mathf.Clamp01((1 + Player.GetCurrentBeat() - note.Time) / note.Length);

            //Shrink the trail and adjust its position
            //Since the pivot of the trail is in the center, meaning it will shrink at the start and at the end,
            //we have to reposition the trail each frame so it appears as if its shrinking from the beginning and the
            //end remains fixed
            trail.transform.localScale = new Vector3(trailScale.x, trailScale.y, trailScale.z * (1 - progress));
            trail.transform.localPosition = new Vector3(trailPosition.x, trailPosition.y + trailPosition.y * progress,
                trailPosition.z);

            //Its possible to hit the note before its beat is reached, because the hit zone is wide to make it easier to hit the notes
            //Increate the score only after the notes real hit position is reached
            if (progress > 0)
            {
                Score += 10 * Multiplier * Time.deltaTime;
            }

            yield return null;
        }

        //Hide the trail
        trail.GetComponent<Renderer>().enabled = false;

        //Disable the particles
        // KeyboardControl.GetStringButton( note.StringIndex ).transform.Find( "Sparks" ).particleEmitter.emit = false;
        // KeyboardControl.GetStringButton( note.StringIndex ).transform.Find( "Sparks" ).particleEmitter.renderer.enabled = false;
    }

    protected void HideNote(int index)
    {
        NoteObjects[index].GetComponent<Renderer>().enabled = false;
    }

    protected bool IsNoteHit(int index)
    {
        Note note = Player.Song.Notes[index];

        //If no button is pressed on this notes string, it cannot be hit
        if (!KeyboardControl.WasButtonJustPressed(note.StringIndex))
        {
            return false;
        }

        //If a note was already hit on this string during this frame, dont hit this one aswell
        if (HasHitNoteOnStringIndexThisFrame[note.StringIndex])
        {
            return false;
        }

        //When the renderer is disabled, this note was already hit before
        if (NoteObjects[index].GetComponent<Renderer>().enabled == false)
        {
            return false;
        }

        //Check if this note is in the hit zone
        if (IsInHitZone(NoteObjects[index]))
        {
            //Set this flag so no two notes are hit with the same button press
            HasHitNoteOnStringIndexThisFrame[note.StringIndex] = true;
            gamePanel.ShowComboEffect(NoteObjects[index].transform.position.z);
            return true;
        }

        //The note is not in the hit zone, therefore cannot be hit
        return false;
    }

    protected bool WasNoteMissed(int index)
    {
        //If position.z is greater than 0, this note can still be hit
        if (NoteObjects[index].transform.position.z > 0)
        {
            return false;
        }

        //If the renderer is disabled, this note was hit
        if (NoteObjects[index].GetComponent<Renderer>().enabled == false)
        {
            return false;
        }

        //Yea, this note was missed
        return true;
    }

    protected void ResetGameStateValues()
    {
        Score = 0;
        Streak = 0;
        MaxStreak = 0;
        Multiplier = 1;
        NumNotesMissed = 0;
        NumNotesHit = 0;
    }

    protected void UpdateNotePosition(int index)
    {
        Note note = Player.Song.Notes[index];

        //If the note is farther away then 6 beats, its not visible on the neck and we dont have to update it
        if (note.Time < Player.GetCurrentBeat() + 6)
        {
            //If the note is not active, it is visible on the neck for the first time
            if (!NoteObjects[index].activeSelf)
            {
                //Activate and show the note
                NoteObjects[index].SetActive(true);
                NoteObjects[index].GetComponent<Renderer>().enabled = true;

                //If there is a trail, show that aswell
                if (Player.Song.Notes[index].Length > 0f)
                {
                    NoteObjects[index].transform.Find("Trail").GetComponent<Renderer>().enabled = true;
                }
            }

            //Calculate how far the note has progressed on the neck
            float progress = (note.Time - Player.GetCurrentBeat() - 0.5f) / 6f;

            //Update its position
            Vector3 position = NoteObjects[index].transform.position;
            position.z = progress * GetGuitarNeckLength();
            NoteObjects[index].transform.position = position;
        }
    }

    protected void UpdateGuiScore()
    {
        gamePanel.ShowScore((int)Mathf.Floor(Score));
    }

    protected void UpdateGuiMultiplier()
    {
        Multiplier = Mathf.Ceil(Streak / 10);

        Multiplier = Mathf.Clamp(Multiplier, 1, 10);

        gamePanel.ShowMultiplier((int)Mathf.Floor(Multiplier));
    }

    protected void UpdateNeckTextureOffset()
    {
        //Get the current offset
        Vector2 offset = GuitarNeckObject.GetComponent<Renderer>().material.GetTextureOffset(MainTex);

        //Update its y component
        offset.y = 1 - (Player.GetCurrentBeat() - 0.5f) / 6f;

        //And set it again
        GuitarNeckObject.GetComponent<Renderer>().material.SetTextureOffset(MainTex, offset);
    }

    protected float GetNeckMoveOffset()
    {
        return Time.deltaTime * Player.Song.BeatsPerMinute * (1f / 6f / 60f);
    }

    protected bool IsInHitZone(GameObject note)
    {
        return note.transform.position.z < GetHitZoneBeginning() && note.transform.position.z > GetHitZoneEnd();
    }

    protected float GetGuitarNeckLength()
    {
        return 20f;
    }

    public Color GetColor(int index)
    {
        if (Colors == null || Colors.Length != 5)
        {
            UpdateColorsArray();
        }

        return Colors[index];
    }

    public float GetScore()
    {
        return Score;
    }

    public float GetMaximumStreak()
    {
        return MaxStreak;
    }

    public float GetNumNotesHit()
    {
        return NumNotesHit;
    }

    public float GetNumNotesMissed()
    {
        return NumNotesMissed;
    }

    protected Vector3 GetStartPosition(int stringIndex)
    {
        return new Vector3((float)(stringIndex - 2), 0f, GetGuitarNeckLength());
    }

    protected GameObject InstantiateNoteFromPrefab(int stringIndex)
    {
        GameObject note = Instantiate(NotePrefab
            , GetStartPosition(stringIndex)
            , Quaternion.identity
        ) as GameObject;

        note.GetComponent<Renderer>().material.color = Colors[stringIndex];
        note.transform.Rotate(new Vector3(-90, 0, 0));

        return note;
    }

    protected GameObject CreateTrailObject(GameObject noteObject, Note note)
    {
        if (note.Length == 0)
        {
            return null;
        }

        GameObject trail = GameObject.CreatePrimitive(PrimitiveType.Plane);

        //We don't need the collider of the plane
        Destroy(trail.GetComponent<MeshCollider>());

        //The guitar neck is 20 units long
        //The plane primitive is 10 units long
        //1/6th of the neck is one beat, therefore 1/3rd of the initial plane length
        float scaleZ = 0.33f * note.Length;

        //Scale the plane
        trail.transform.localScale = new Vector3(0.03f, 1f, scaleZ);

        //Add the trail as child of the note
        trail.transform.parent = noteObject.transform;

        //position it so that the trail is behind the note
        trail.transform.localPosition = new Vector3(0f, -10f * scaleZ / 2f, 0.01f);

        //Setup colors and shader
        trail.GetComponent<Renderer>().material.color = Colors[note.StringIndex] * 0.2f;
        trail.GetComponent<Renderer>().material.shader = Shader.Find("Self-Illumin/Diffuse");
        trail.GetComponent<Renderer>().enabled = false;

        trail.name = "Trail";

        return trail;
    }

    protected float GetHitZoneBeginning()
    {
        //Adjust this if you want to enable the player to hit the note earlier
        return 2.4f;
    }

    protected float GetHitZoneEnd()
    {
        //Adjust this if you want to enable the player to hit the note later
        return 0.95f;
    }

    public void OnSongFinished()
    {
        gamePanel.scoreText.enabled = false;
        gamePanel.multiplierText.enabled = false;

        UIManager.Instance.ShowEndOfSong();
        // GetComponent<EndOfSongMenu>().ShowMenu();
    }
}