using UnityEngine;
using FMODUnity;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    // =========================================================
    //  FMOD EVENTS : MUSIQUES
    // =========================================================
    [Header("Music")]
    public EventReference MenuMusicEvent;
    public EventReference GameplayMusicEvent;

    private FMOD.Studio.EventInstance musicInstance;

    // =========================================================
    //  PLAYER
    // =========================================================
    [Header("Player")]
    public EventReference DashEvent;
    public EventReference WallBumpEvent;
    public EventReference PlayerSpawnEvent;

    // =========================================================
    //  ENEMIES
    // =========================================================
    [Header("Enemies")]
    public EventReference EnemyDeathEvent;

    // =========================================================
    //  CELLS
    // =========================================================
    [Header("Cells")]
    public EventReference CellInfectEvent;
    public EventReference CellPurifyEvent;

    // =========================================================
    //  UI
    // =========================================================
    [Header("UI")]
    public EventReference UIClickEvent;
    public EventReference UIBackEvent;


    // =========================================================
    //  INIT
    // =========================================================
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // =========================================================
    //  MUSIC CONTROL
    // =========================================================
    public void PlayMenuMusic()
    {
        StopCurrentMusic();

        if (!MenuMusicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(MenuMusicEvent);
            musicInstance.start();
        }
    }

    public void PlayGameplayMusic()
    {
        StopCurrentMusic();

        if (!GameplayMusicEvent.IsNull)
        {
            musicInstance = RuntimeManager.CreateInstance(GameplayMusicEvent);
            musicInstance.start();
        }
    }

    private void StopCurrentMusic()
    {
        if (musicInstance.isValid())
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            musicInstance.release();
        }
    }


    // =========================================================
    //  UI SOUNDS
    // =========================================================
    public void PlayUIClick()
    {
        if (!UIClickEvent.IsNull)
            RuntimeManager.PlayOneShot(UIClickEvent);
    }

    public void PlayUIBack()
    {
        if (!UIBackEvent.IsNull)
            RuntimeManager.PlayOneShot(UIBackEvent);
    }


    // =========================================================
    //  PLAYER SOUNDS
    // =========================================================
    public void PlayDash()
    {
        if (!DashEvent.IsNull)
            RuntimeManager.PlayOneShot(DashEvent);
    }

    public void PlayWallBump()
    {
        if (!WallBumpEvent.IsNull)
            RuntimeManager.PlayOneShot(WallBumpEvent);
    }

    public void PlayPlayerSpawn()
    {
        if (!PlayerSpawnEvent.IsNull)
            RuntimeManager.PlayOneShot(PlayerSpawnEvent);
    }


    // =========================================================
    //  ENEMY SOUNDS
    // =========================================================
    public void PlayEnemyDeath()
    {
        if (!EnemyDeathEvent.IsNull)
            RuntimeManager.PlayOneShot(EnemyDeathEvent);
    }


    // =========================================================
    //  CELL SOUNDS
    // =========================================================
    public void PlayCellInfect()
    {
        if (!CellInfectEvent.IsNull)
            RuntimeManager.PlayOneShot(CellInfectEvent);
    }

    public void PlayCellPurify()
    {
        if (!CellPurifyEvent.IsNull)
            RuntimeManager.PlayOneShot(CellPurifyEvent);
    }


    // =========================================================
    //  CLEANUP
    // =========================================================
    void OnDestroy()
    {
        StopCurrentMusic();
    }
}
