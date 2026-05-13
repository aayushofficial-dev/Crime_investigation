using UnityEngine;
using UnityEngine.SceneManagement;

public class AccidentCutsceneDirector : MonoBehaviour
{
    private enum CutsceneStep
    {
        CityView,
        AccidentView,
        CrashScreen,
        InvestigationReady
    }

    private static AccidentCutsceneDirector instance;

    private GameObject car;
    private GameObject victim;
    private Camera cutsceneCamera;
    private AudioSource audioSource;
    private CutsceneStep step = CutsceneStep.CityView;
    private float timer;
    private bool skidSoundPlayed;
    private bool crashSoundPlayed;
    private bool tireMarksCreated;
    private string message = "City View";

    private readonly Vector3 carStart = new Vector3(146.2f, 4.2f, -168f);
    private readonly Vector3 carEnd = new Vector3(148.2f, 4.2f, -140.8f);
    private readonly Vector3 victimStart = new Vector3(141.8f, 4.22f, -143.2f);
    private readonly Vector3 victimHit = new Vector3(146.7f, 4.22f, -142.1f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoStart()
    {
        if (!SceneManager.GetActiveScene().name.ToLower().Contains("cityandcar"))
            return;

        if (instance != null)
            return;

        instance = new GameObject("Accident Cutscene Director").AddComponent<AccidentCutsceneDirector>();
    }

    private void Start()
    {
        car = GameObject.Find("Car");
        victim = GameObject.Find("Formal_With Mustache_Base");

        if (car == null || victim == null)
        {
            Debug.LogWarning("Accident cutscene needs objects named Car and Formal_With Mustache_Base.");
            return;
        }

        PrepareSceneObjects();
        CreateCamera();
        CreateAudio();
    }

    private void Update()
    {
        if (car == null || victim == null || cutsceneCamera == null)
            return;

        timer += Time.deltaTime;

        if (step == CutsceneStep.CityView)
            PlayCityView();
        else if (step == CutsceneStep.AccidentView)
            PlayAccidentView();
        else if (step == CutsceneStep.CrashScreen)
            PlayCrashScreen();
        else
            ShowInvestigationReady();
    }

    private void PrepareSceneObjects()
    {
        DisablePhysics(car);
        DisablePhysics(victim);

        CarAutoDrive carAutoDrive = car.GetComponent<CarAutoDrive>();
        if (carAutoDrive != null)
            carAutoDrive.enabled = false;

        Animator victimAnimator = victim.GetComponentInChildren<Animator>();
        if (victimAnimator != null)
            victimAnimator.enabled = false;

        car.transform.position = carStart;
        car.transform.rotation = Quaternion.identity;

        victim.transform.position = victimStart;
        victim.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
    }

    private void DisablePhysics(GameObject target)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void CreateCamera()
    {
        Camera oldCamera = Camera.main;
        if (oldCamera != null)
            oldCamera.gameObject.SetActive(false);

        GameObject cameraObject = new GameObject("Accident Cutscene Camera");
        cutsceneCamera = cameraObject.AddComponent<Camera>();
        cameraObject.tag = "MainCamera";
    }

    private void CreateAudio()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 0.8f;
    }

    private void PlayCityView()
    {
        SetCamera(new Vector3(105f, 65f, -210f), new Vector3(146f, 4f, -145f), 55f);
        message = "Wide city view before the accident";

        if (timer > 3f)
            NextStep(CutsceneStep.AccidentView);
    }

    private void PlayAccidentView()
    {
        SetCamera(new Vector3(135f, 12f, -155f), new Vector3(147f, 4.5f, -145f), 42f);
        message = "Accident scene view";

        float progress = Mathf.Clamp01(timer / 4f);
        car.transform.position = Vector3.Lerp(carStart, carEnd, progress);
        victim.transform.position = Vector3.Lerp(victimStart, victimHit, progress);

        if (!skidSoundPlayed && timer > 2.4f)
        {
            skidSoundPlayed = true;
            PlayGeneratedSound(540f, 0.5f);
        }

        if (!tireMarksCreated && timer > 2.7f)
        {
            tireMarksCreated = true;
            CreateTireMarks();
        }

        if (timer > 4f)
        {
            victim.transform.position = victimHit;
            victim.transform.rotation = Quaternion.Euler(90f, 0f, 90f);
            NextStep(CutsceneStep.CrashScreen);
        }
    }

    private void PlayCrashScreen()
    {
        if (!crashSoundPlayed)
        {
            crashSoundPlayed = true;
            PlayCrashSound();
        }

        message = "Crash";

        if (timer > 2f)
            NextStep(CutsceneStep.InvestigationReady);
    }

    private void ShowInvestigationReady()
    {
        SetCamera(new Vector3(137.8f, 10.5f, -137.8f), new Vector3(147.4f, 4.8f, -143.4f), 48f);
        message = "Investigation Started";
    }

    private void NextStep(CutsceneStep nextStep)
    {
        step = nextStep;
        timer = 0f;
    }

    private void SetCamera(Vector3 position, Vector3 lookAt, float fieldOfView)
    {
        cutsceneCamera.transform.position = position;
        cutsceneCamera.transform.rotation = Quaternion.LookRotation(lookAt - position);
        cutsceneCamera.fieldOfView = fieldOfView;
    }

    private void CreateTireMarks()
    {
        CreateMark(new Vector3(145.6f, 4.25f, -151f));
        CreateMark(new Vector3(146.8f, 4.25f, -151f));
    }

    private void CreateMark(Vector3 position)
    {
        GameObject mark = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mark.name = "Tire Mark";
        mark.transform.position = position;
        mark.transform.localScale = new Vector3(0.25f, 0.03f, 5f);
        mark.GetComponent<Renderer>().material.color = Color.black;
    }

    private void PlayGeneratedSound(float frequency, float duration)
    {
        if (audioSource == null)
            return;

        AudioClip clip = AudioClip.Create("Skid Sound", 11025, 1, 11025, false);
        float[] samples = new float[11025];

        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 11025f;
            float fade = 1f - Mathf.Clamp01(t / duration);
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.4f;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }

    private void PlayCrashSound()
    {
        if (audioSource == null)
            return;

        AudioClip clip = AudioClip.Create("Crash Sound", 11025, 1, 11025, false);
        float[] samples = new float[11025];

        for (int i = 0; i < samples.Length; i++)
        {
            float t = i / 11025f;
            float fade = Mathf.Exp(-t * 8f);
            samples[i] = Random.Range(-1f, 1f) * fade;
        }

        clip.SetData(samples, 0);
        audioSource.PlayOneShot(clip);
    }

    private void OnGUI()
    {
        if (step == CutsceneStep.CrashScreen)
        {
            DrawCrashScreen();
            return;
        }

        GUI.Box(new Rect(20f, 20f, 420f, 70f), message);
    }

    private void DrawCrashScreen()
    {
        GUI.color = Color.black;
        GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);

        GUI.color = Color.white;
        DrawCrack(Screen.width * 0.5f, Screen.height * 0.5f, 180f, 3f);
        DrawCrack(Screen.width * 0.52f, Screen.height * 0.5f, 120f, -35f);
        DrawCrack(Screen.width * 0.48f, Screen.height * 0.48f, 130f, 42f);

        GUI.Label(new Rect(Screen.width / 2f - 80f, Screen.height / 2f + 80f, 200f, 30f), "CRASH");
        GUI.color = Color.white;
    }

    private void DrawCrack(float x, float y, float length, float angle)
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, new Vector2(x, y));
        GUI.DrawTexture(new Rect(x, y, length, 3f), Texture2D.whiteTexture);
        GUI.matrix = oldMatrix;
    }
}
