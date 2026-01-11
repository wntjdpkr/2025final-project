using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CombatVisualSystem : MonoBehaviour
{
    [Header("Post-Processing Volume")]
    [SerializeField] private Volume postProcessVolume;

    [Header("Second Battle Ambience Settings")]
    [SerializeField] private float ambienceIntensity = 0.2f;
    [SerializeField] private Color ambienceColor = new Color(0.8f, 0.2f, 0.2f, 1f);
    [SerializeField] private float ambienceSmoothness = 0.4f;
    [SerializeField] private float ambientDarkness = -0.3f;
    [SerializeField] private float ambientDesaturation = -10f;
    [SerializeField] private float filmGrainIntensity = 0.3f;
    [SerializeField] private float filmGrainResponse = 0.8f;

    [Header("Damage Feedback Settings")]
    [SerializeField] private float damageIntensity = 0.6f;
    [SerializeField] private Color damageColor = new Color(1f, 0f, 0f, 1f);
    [SerializeField] private float damageSmoothness = 0.3f;
    [SerializeField] private float damageHoldTime = 0.2f;
    [SerializeField] private float damageFadeTime = 0.5f;

    [Header("Slow Motion Effect Settings")]
    [SerializeField] private float slowMotionChromaticAberration = 0.8f;
    [SerializeField] private float slowMotionMotionBlurIntensity = 0.5f;
    [SerializeField] private MotionBlurQuality slowMotionMotionBlurQuality = MotionBlurQuality.High;
    [SerializeField] private float slowMotionFadeInTime = 0.3f;
    [SerializeField] private float slowMotionFadeOutTime = 0.5f;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private ChromaticAberration chromaticAberration;
    private MotionBlur motionBlur;
    private FilmGrain filmGrain;

    private Coroutine damageEffectCoroutine;
    private Coroutine slowMotionEffectCoroutine;
    private bool isSecondBattle = false;

    void Start()
    {
        Debug.Log("=== CombatVisualSystem START ===");

        if (!ValidateReferences())
        {
            Debug.LogError("CombatVisualSystem: Failed to validate references. Disabling system.");
            enabled = false;
            return;
        }

        if (!InitializePostProcessing())
        {
            Debug.LogError("CombatVisualSystem: Failed to initialize post-processing. Disabling system.");
            enabled = false;
            return;
        }

        CheckBattleNumber();

        Debug.Log("=== CombatVisualSystem INITIALIZED SUCCESSFULLY ===");
    }

    bool ValidateReferences()
    {
        if (postProcessVolume == null)
        {
            Debug.Log("CombatVisualSystem: Volume not assigned, searching...");
            postProcessVolume = FindObjectOfType<Volume>();

            if (postProcessVolume == null)
            {
                Debug.LogError("CombatVisualSystem: Volume not found in scene! Please add a Global Volume with URP profile.");
                return false;
            }
            else
            {
                Debug.Log($"CombatVisualSystem: Volume found automatically: {postProcessVolume.name}");
            }
        }
        else
        {
            Debug.Log($"CombatVisualSystem: Volume already assigned: {postProcessVolume.name}");
        }

        if (postProcessVolume.profile == null)
        {
            Debug.LogError("CombatVisualSystem: Volume has no profile assigned!");
            return false;
        }

        Debug.Log($"CombatVisualSystem: Using profile: {postProcessVolume.profile.name}");
        return true;
    }

    bool InitializePostProcessing()
    {
        if (postProcessVolume == null || postProcessVolume.profile == null)
        {
            Debug.LogError("CombatVisualSystem: Cannot initialize - Volume or profile is null!");
            return false;
        }

        bool allComponentsFound = true;

        if (postProcessVolume.profile.TryGet(out vignette))
        {
            vignette.active = false;
            vignette.intensity.Override(0f);
            vignette.color.Override(Color.black);
            vignette.smoothness.Override(0.4f);
            Debug.Log("CombatVisualSystem: Vignette found and initialized with overrides");
        }
        else
        {
            Debug.LogError("CombatVisualSystem: Vignette NOT FOUND! Please add Vignette override to Volume Profile.");
            allComponentsFound = false;
        }

        if (postProcessVolume.profile.TryGet(out colorAdjustments))
        {
            colorAdjustments.active = false;
            colorAdjustments.postExposure.Override(0f);
            colorAdjustments.saturation.Override(0f);
            Debug.Log("CombatVisualSystem: ColorAdjustments found and initialized with overrides");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: ColorAdjustments NOT FOUND. Second battle ambience will not work.");
        }

        if (postProcessVolume.profile.TryGet(out chromaticAberration))
        {
            chromaticAberration.active = false;
            chromaticAberration.intensity.Override(0f);
            Debug.Log("CombatVisualSystem: ChromaticAberration found and initialized with overrides");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: ChromaticAberration NOT FOUND. Slow motion visual effect will not work.");
        }

        if (postProcessVolume.profile.TryGet(out motionBlur))
        {
            motionBlur.active = false;
            motionBlur.intensity.Override(0f);
            motionBlur.quality.Override(MotionBlurQuality.Low);
            Debug.Log("CombatVisualSystem: MotionBlur found and initialized with overrides");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: MotionBlur NOT FOUND. Slow motion motion blur will not work.");
        }

        if (postProcessVolume.profile.TryGet(out filmGrain))
        {
            filmGrain.active = false;
            filmGrain.intensity.Override(0f);
            filmGrain.response.Override(0.8f);
            Debug.Log("CombatVisualSystem: FilmGrain found and initialized with overrides");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: FilmGrain NOT FOUND. Second battle film grain effect will not work.");
        }

        return allComponentsFound;
    }

    void CheckBattleNumber()
    {
        int battleNumber = CombatManager.GetBattleCount();
        Debug.Log($"CombatVisualSystem: Current battle number is {battleNumber}");

        if (battleNumber == 2)
        {
            Debug.Log("CombatVisualSystem: Battle 2 detected - enabling ambience effects");
            EnableSecondBattleAmbience();
        }
        else
        {
            Debug.Log("CombatVisualSystem: Battle 1 - ambience effects will not be applied");
        }
    }

    public void EnableSecondBattleAmbience()
    {
        isSecondBattle = true;

        if (vignette != null)
        {
            vignette.active = true;
            vignette.intensity.Override(ambienceIntensity);
            vignette.color.Override(ambienceColor);
            vignette.smoothness.Override(ambienceSmoothness);

            Debug.Log($"CombatVisualSystem: Second battle vignette enabled - Intensity: {ambienceIntensity}, Color: {ambienceColor}");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: Cannot enable ambience - Vignette is null!");
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = true;
            colorAdjustments.postExposure.Override(ambientDarkness);
            colorAdjustments.saturation.Override(ambientDesaturation);

            Debug.Log($"CombatVisualSystem: Color adjustments enabled - Exposure: {ambientDarkness}, Saturation: {ambientDesaturation}");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: Cannot enable color adjustments - ColorAdjustments is null!");
        }

        if (filmGrain != null)
        {
            filmGrain.active = true;
            filmGrain.intensity.Override(filmGrainIntensity);
            filmGrain.response.Override(filmGrainResponse);

            Debug.Log($"CombatVisualSystem: Film grain enabled - Intensity: {filmGrainIntensity}, Response: {filmGrainResponse}");
        }
        else
        {
            Debug.LogWarning("CombatVisualSystem: Cannot enable film grain - FilmGrain is null!");
        }
    }

    public void ShowDamageEffect()
    {
        if (vignette == null)
        {
            Debug.LogWarning("CombatVisualSystem: Cannot show damage effect - Vignette is null!");
            return;
        }

        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }

        damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine());
    }

    IEnumerator DamageEffectCoroutine()
    {
        float baseIntensity = vignette.active ? vignette.intensity.value : 0f;
        Color baseColor = vignette.active ? vignette.color.value : Color.black;
        float baseSmoothness = vignette.active ? vignette.smoothness.value : 0.4f;

        Debug.Log($"CombatVisualSystem: Damage effect started - Base intensity: {baseIntensity}");

        vignette.active = true;
        vignette.intensity.Override(damageIntensity);
        vignette.color.Override(damageColor);
        vignette.smoothness.Override(damageSmoothness);

        yield return new WaitForSeconds(damageHoldTime);

        float elapsed = 0f;
        while (elapsed < damageFadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / damageFadeTime;

            vignette.intensity.value = Mathf.Lerp(damageIntensity, baseIntensity, t);
            vignette.color.value = Color.Lerp(damageColor, baseColor, t);
            vignette.smoothness.value = Mathf.Lerp(damageSmoothness, baseSmoothness, t);

            yield return null;
        }

        vignette.intensity.value = baseIntensity;
        vignette.color.value = baseColor;
        vignette.smoothness.value = baseSmoothness;

        if (!isSecondBattle && baseIntensity == 0f)
        {
            vignette.active = false;
        }

        Debug.Log($"CombatVisualSystem: Damage effect ended - Returned to base intensity: {baseIntensity}");
    }

    public void ShowSlowMotionEffect()
    {
        if (chromaticAberration == null && motionBlur == null)
        {
            Debug.LogWarning("CombatVisualSystem: Cannot show slow motion effect - ChromaticAberration and MotionBlur are null!");
            return;
        }

        if (slowMotionEffectCoroutine != null)
        {
            StopCoroutine(slowMotionEffectCoroutine);
        }

        slowMotionEffectCoroutine = StartCoroutine(SlowMotionEffectCoroutine());
    }

    public void HideSlowMotionEffect()
    {
        if (slowMotionEffectCoroutine != null)
        {
            StopCoroutine(slowMotionEffectCoroutine);
        }

        slowMotionEffectCoroutine = StartCoroutine(SlowMotionFadeOutCoroutine());
    }

    IEnumerator SlowMotionEffectCoroutine()
    {
        Debug.Log("CombatVisualSystem: Slow motion effect started");

        if (chromaticAberration != null)
        {
            chromaticAberration.active = true;
            chromaticAberration.intensity.Override(0f);
        }

        if (motionBlur != null)
        {
            motionBlur.active = true;
            motionBlur.intensity.Override(0f);
            motionBlur.quality.Override(slowMotionMotionBlurQuality);
        }

        float elapsed = 0f;
        while (elapsed < slowMotionFadeInTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slowMotionFadeInTime;

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(0f, slowMotionChromaticAberration, t);
            }

            if (motionBlur != null)
            {
                motionBlur.intensity.value = Mathf.Lerp(0f, slowMotionMotionBlurIntensity, t);
            }

            yield return null;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = slowMotionChromaticAberration;
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = slowMotionMotionBlurIntensity;
        }

        Debug.Log($"CombatVisualSystem: Slow motion effect fully applied - CA: {slowMotionChromaticAberration}, MB: {slowMotionMotionBlurIntensity}, Quality: {slowMotionMotionBlurQuality}");
    }

    IEnumerator SlowMotionFadeOutCoroutine()
    {
        Debug.Log("CombatVisualSystem: Slow motion effect fading out");

        float startCA = chromaticAberration != null ? chromaticAberration.intensity.value : 0f;
        float startMB = motionBlur != null ? motionBlur.intensity.value : 0f;

        float elapsed = 0f;
        while (elapsed < slowMotionFadeOutTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slowMotionFadeOutTime;

            if (chromaticAberration != null)
            {
                chromaticAberration.intensity.value = Mathf.Lerp(startCA, 0f, t);
            }

            if (motionBlur != null)
            {
                motionBlur.intensity.value = Mathf.Lerp(startMB, 0f, t);
            }

            yield return null;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = 0f;
            chromaticAberration.active = false;
        }

        if (motionBlur != null)
        {
            motionBlur.intensity.value = 0f;
            motionBlur.active = false;
        }

        Debug.Log("CombatVisualSystem: Slow motion effect ended");
    }

    void OnDestroy()
    {
        if (vignette != null)
        {
            vignette.active = false;
        }

        if (colorAdjustments != null)
        {
            colorAdjustments.active = false;
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.active = false;
        }

        if (motionBlur != null)
        {
            motionBlur.active = false;
        }

        if (filmGrain != null)
        {
            filmGrain.active = false;
        }

        Debug.Log("CombatVisualSystem: Cleaned up post-processing effects");
    }
}