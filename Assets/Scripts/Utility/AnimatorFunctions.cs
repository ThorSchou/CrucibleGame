using UnityEngine;
using UnityEngine.SceneManagement;

public class AnimatorFunctions : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private Animator setBoolInAnimator;

    void Start()
    {
        if (!audioSource) audioSource = NewPlayer.Instance.audioSource;
    }

    public void HidePlayer(bool hide) => NewPlayer.Instance.combat.Hide(hide);
    public void JumpPlayer(float power = 1f) => NewPlayer.Instance.Jump(power);
    public void FreezePlayer(bool freeze) => NewPlayer.Instance.Freeze(freeze);
    public void PlaySound(AudioClip sound) => audioSource.PlayOneShot(sound);
    public void EmitParticles(int amount) => particleSystem.Emit(amount);
    public void ScreenShake(float power) => NewPlayer.Instance.cameraEffects.Shake(power, 1f);
    public void SetTimeScaleTo(float t) => Time.timeScale = t;
    public void LoadScene(string level) => SceneManager.LoadScene(level);

    public void SetAnimBoolToFalse(string boolName) => setBoolInAnimator.SetBool(boolName, false);
    public void SetAnimBoolToTrue(string boolName) => setBoolInAnimator.SetBool(boolName, true);

    public void FadeOutMusic()
    {
        if (GameManager.Instance.gameMusic != null)
            GameManager.Instance.gameMusic.maxVolume = 0f;
    }
}