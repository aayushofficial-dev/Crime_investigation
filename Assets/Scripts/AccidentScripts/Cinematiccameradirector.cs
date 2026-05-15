using System.Collections;
using UnityEngine;

public class CinematicCameraDirector : MonoBehaviour
{
    [Header("─── Camera ───")]
    public Camera mainCamera;              // drag Main Camera here

    [Header("─── Scene References ───")]
    public Transform car;
    public Transform character;

    [Header("─── Shot Durations (seconds) ───")]
    public float shot1_wideDuration    = 2.0f;
    public float shot2_grilleDuration  = 1.2f;
    public float shot3_slowMoDuration  = 2.5f;
    public float shot4_groundDuration  = 1.8f;
    public float shot5_pullbackDuration= 2.5f;

    [Header("─── Slow Motion ───")]
    public float slowMoScale = 0.25f;     // 0.25 = 25% speed during impact shot

    [Header("─── Offsets (tweak in Inspector) ───")]
    public Vector3 shot1_offset = new Vector3(-8f,  2.5f, -4f);
    public Vector3 shot2_offset = new Vector3( 0f,  0.8f, -6f);   // relative to car front
    public Vector3 shot3_offset = new Vector3( 4f,  2.0f,  3f);   // side angle on character
    public Vector3 shot4_offset = new Vector3( 0f,  0.15f, 2f);   // ground level behind char
    public Vector3 shot5_offset = new Vector3(-5f,  4f,   -6f);   // wide pullback

    // ── private ──────────────────────────────────────────────────────────────
    private Vector3 _savedCamPos;
    private Quaternion _savedCamRot;
    private bool _running = false;

    public void BeginCinematic()
    {
        if (_running) return;
        _running = true;
        _savedCamPos = mainCamera.transform.position;
        _savedCamRot = mainCamera.transform.rotation;
        StartCoroutine(PlayAllShots());
    }

    IEnumerator PlayAllShots()
    {
       
        yield return StartCoroutine(
            MoveCamera(
                character.position + shot1_offset,
                character.position + Vector3.up * 1f,
                shot1_wideDuration,
                easeOut: false
            )
        );

        Vector3 carFront = car.position + car.forward * 3f;
        yield return StartCoroutine(
            MoveCamera(
                carFront + shot2_offset,
                carFront,
                shot2_grilleDuration,
                easeOut: false
            )
        );

        Time.timeScale     = slowMoScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return StartCoroutine(
            MoveCamera(
                character.position + shot3_offset,
                character.position + Vector3.up * 0.8f,
                shot3_slowMoDuration,   // this is REAL-time seconds (unscaled)
                easeOut: true,
                unscaled: true
            )
        );

        // ── Resume normal time ────────────────────────────────────────────────
        Time.timeScale      = 1f;
        Time.fixedDeltaTime = 0.02f;

        // ── SHOT 4 — Ground level: character landing ──────────────────────────
        yield return StartCoroutine(
            MoveCamera(
                character.position + shot4_offset,
                character.position,
                shot4_groundDuration,
                easeOut: true
            )
        );

        // ── SHOT 5 — Wide pullback reveal ─────────────────────────────────────
        Vector3 midPoint = (car.position + character.position) * 0.5f;
        yield return StartCoroutine(
            MoveCamera(
                midPoint + shot5_offset,
                midPoint + Vector3.up * 0.5f,
                shot5_pullbackDuration,
                easeOut: true
            )
        );

        // ── Cinematic done — tell director ────────────────────────────────────
        FindFirstObjectByType<AccidentSceneDirector>()?.OnCinematicComplete();
    }

    /// <summary>Smoothly moves the camera to a world position, looking at a target.</summary>
    IEnumerator MoveCamera(Vector3 targetPos, Vector3 lookAt,
                           float duration, bool easeOut = true, bool unscaled = false)
    {
        Vector3    startPos = mainCamera.transform.position;
        Quaternion startRot = mainCamera.transform.rotation;
        Quaternion endRot   = Quaternion.LookRotation(lookAt - targetPos);

        float t = 0f;
        while (t < duration)
        {
            float dt = unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            t += dt;
            float p = Mathf.Clamp01(t / duration);
            float smooth = easeOut ? EaseInOut(p) : p;

            mainCamera.transform.position = Vector3.Lerp(startPos, targetPos, smooth);
            mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, smooth);
            yield return null;
        }

        mainCamera.transform.position = targetPos;
        mainCamera.transform.rotation = endRot;
    }

    float EaseInOut(float t) => t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;
}