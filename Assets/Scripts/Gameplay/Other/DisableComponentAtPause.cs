using System.Collections.Generic;
using UnityEngine;

public class DisableComponentAtPause : MonoBehaviour
{
    private Dictionary<string, object> objects;

    [SerializeField] private Behaviour[] componentsToDisable;
    [SerializeField] private Component[] componentsToStopAtPause;

    private void Start()
    {
        objects = new Dictionary<string, object>();
        PauseManager.instance.callBackOnPauseDisable += EnablePause;
        PauseManager.instance.callBackOnPauseEnable += DisablePause;
    }

    #region Animator

    private void StopAnimator(Animator animator)
    {
        objects[$"animator{animator.GetHashCode()}speed"] = animator.speed;
        animator.speed = 0f;
    }

    private void ResumeAnimator(Animator animator)
    {
        object animSpeedObj = objects[$"animator{animator.GetHashCode()}speed"];
        animator.speed = animSpeedObj == null ? 1f : (float)animSpeedObj;
    }

    #endregion

    #region ParticleSystem

    private void StopParticleSystem(ParticleSystem particleSystem)
    {
        particleSystem.Play(false);
    }

    private void ResumeParticleSystem(ParticleSystem particleSystem)
    {
        particleSystem.Pause();
    }

    #endregion

    private void DisablePause()
    {
        foreach (Behaviour comp in componentsToDisable)
        {
            comp.enabled = false;
        }

        foreach (Component comp in componentsToStopAtPause)
        {
            if (comp is Animator animator)
            {
                ResumeAnimator(animator);
            }

            if (comp is ParticleSystem particleSystem)
            {
                ResumeParticleSystem(particleSystem);
            }

            string errorMsg = $"Component of type:{comp.GetType()} is not supported to resume at pause";
            LogManager.instance.AddLog(errorMsg, new object[] { comp.name, comp.gameObject.name });
            Debug.Log(errorMsg);
        }
    }

    private void EnablePause()
    {
        foreach (Behaviour comp in componentsToDisable)
        {
            comp.enabled = true;
        }

        foreach(Component comp in componentsToStopAtPause)
        {
            if(comp is Animator animator)
            {
                StopAnimator(animator);
            }

            if (comp is ParticleSystem particleSystem)
            {
                StopParticleSystem(particleSystem);
            }

            string errorMsg = $"Component of type:{comp.GetType()} is not supported to stop at pause";
            LogManager.instance.AddLog(errorMsg, new object[] { comp.name, comp.gameObject.name });
            Debug.Log(errorMsg);
        }
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseEnable -= DisablePause;
        PauseManager.instance.callBackOnPauseDisable -= EnablePause;
    }
}
