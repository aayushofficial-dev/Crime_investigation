using System.Collections;
using UnityEngine;

public class CarAccidentController : MonoBehaviour
{
    [Header("─── Target ───")]
    public Transform character;               // drag your character GO here

    [Header("─── Speed ───")]
    public float driveSpeed = 6f;             // normal driving speed
    public float brakeDistance = 4f;          // units before character to start braking
    public float brakeForce = 8f;             // how fast it slows down

    [Header("─── After Impact ───")]
    public float pushDistanceAfterHit = 1.2f; // car nudges forward a bit after hit
    public float pushSpeed = 2f;

    [Header("─── Audio ───")]
    public AudioSource engineAudio;           // AudioSource on the car
    public AudioClip engineClip;
    public AudioClip skidClip;
    public AudioClip impactClip;

    [Header("─── VFX ───")]
    public ParticleSystem dustVFX;

    [Header("─── Events ───")]
    public AccidentSceneDirector director;    // drag AccidentSceneDirector GO here

    // ── private ──────────────────────────────────────────────────────────────
    private enum DriveState { Driving, Braking, Impact, PushingThrough, Done }
    private DriveState _state = DriveState.Driving;
    private float _speed;
    private bool _skidPlayed;

    void Start()
    {
        _speed = 0f;

        // Start engine sound
        if (engineAudio && engineClip)
        {
            engineAudio.clip  = engineClip;
            engineAudio.loop  = true;
            engineAudio.Play();
        }
    }

    void Update()
    {
        if (_state == DriveState.Done) return;

        float dist = Vector3.Distance(
            new Vector3(transform.position.x, 0, transform.position.z),
            new Vector3(character.position.x, 0, character.position.z)
        );

        switch (_state)
        {
            case DriveState.Driving:
                _speed = Mathf.MoveTowards(_speed, driveSpeed, 3f * Time.deltaTime);
                if (engineAudio) engineAudio.pitch = 0.85f + (_speed / driveSpeed) * 0.5f;

                if (dist <= brakeDistance)
                {
                    _state = DriveState.Braking;
                    StartSkid();
                }
                break;

            case DriveState.Braking:
                _speed = Mathf.MoveTowards(_speed, driveSpeed * 0.5f, brakeForce * Time.deltaTime);
                if (engineAudio) engineAudio.pitch = 0.7f + (_speed / driveSpeed) * 0.3f;

                if (dist <= 0.8f)
                {
                    _state = DriveState.Impact;
                    OnHitCharacter();
                }
                break;

            case DriveState.PushingThrough:
                _speed = Mathf.MoveTowards(_speed, 0f, 5f * Time.deltaTime);
                if (_speed <= 0.05f)
                {
                    _speed = 0f;
                    _state = DriveState.Done;
                    StopAllVFX();
                }
                break;
        }

        transform.Translate(Vector3.forward * _speed * Time.deltaTime);
    }

    void StartSkid()
    {
        if (_skidPlayed) return;
        _skidPlayed = true;
        if (skidClip)  AudioSource.PlayClipAtPoint(skidClip, transform.position, 0.9f);
    }

    void OnHitCharacter()
    {
        // stop engine, play crash
        if (engineAudio) engineAudio.Stop();
        if (impactClip)  AudioSource.PlayClipAtPoint(impactClip, transform.position, 1f);

        // dust burst
        if (dustVFX) dustVFX.Play();

        // tell director
        director.OnCarImpact(transform.forward);

        // car nudges forward slightly
        _state = DriveState.PushingThrough;
        _speed = pushSpeed;
    }

    void StopAllVFX()
    {
        if (dustVFX && dustVFX.isPlaying) dustVFX.Stop();
    }
}