using UnityEngine;
using System.Collections;

public class ZoneTriggerDetector : MonoBehaviour
{
    [Header("Settings")]
    public GameObject targetObjectToToggle;

    [Header("Audio Guide Settings")]
    public AudioSource guideAudioSource;
    
    private bool hasPlayedAudio = false;

    private void Start()
    {
        if (targetObjectToToggle != null)
        {
            StartCoroutine(InitialDisableRoutine());
        }
    }

    private IEnumerator InitialDisableRoutine()
    {
        yield return new WaitForEndOfFrame();
        
        if (targetObjectToToggle != null)
        {
            targetObjectToToggle.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(true);
            }

            if (!hasPlayedAudio && guideAudioSource != null)
            {
                guideAudioSource.Play();
                hasPlayedAudio = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.GetComponent<FirstPersonController>() != null)
        {
            if (targetObjectToToggle != null)
            {
                targetObjectToToggle.SetActive(false);
            }
        }
    }
}