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
        PauseManager.instance.callBackOnPauseEnable += EnablePause;
        PauseManager.instance.callBackOnPauseDisable += DisablePause;
    }

    #region Animator

    private void StopAnimator(Animator animator)
    {
        string key = $"animator{animator.GetHashCode()}speed";
        if (objects.ContainsKey(key))
            objects[key] = animator.speed;
        else
            objects.Add(key, animator.speed);

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
                continue;
            }

            if (comp is ParticleSystem particleSystem)
            {
                ResumeParticleSystem(particleSystem);
                continue;
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
                continue;
            }

            if (comp is ParticleSystem particleSystem)
            {
                StopParticleSystem(particleSystem);
                continue;
            }

            string errorMsg = $"Component of type:{comp.GetType()} is not supported to stop at pause";
            LogManager.instance.AddLog(errorMsg, new object[] { comp.name, comp.gameObject.name });
            Debug.Log(errorMsg);
        }
    }

    private void OnDestroy()
    {
        PauseManager.instance.callBackOnPauseDisable -= DisablePause;
        PauseManager.instance.callBackOnPauseEnable -= EnablePause;
    }
}
