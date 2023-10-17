using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoiceTrigger : MonoBehaviour
{
    float downVolume;
    private AudioSource source;
    [SerializeField]
    private string subtitleKey;
    public enum Type
    {
        Trigger,
        Stay
    }
    [SerializeField]
    private Type type;
    private float timer = 0f;
    [SerializeField]
    private float stayTime;

    private void OnTriggerExit(Collider other)
    {
        if (type == Type.Stay)
        {
            if (other.transform.parent.CompareTag("Player"))
            {
                if (GameManager.Instance.ragdollController.currentState != RagdollController.PlayerState.Falling)
                {
                    timer = 0f;
                }
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (VoiceManager.Instance.audio.isPlaying)
        {
            return;
        }
        if(type == Type.Trigger) //��ġ�� �����ϸ� �÷��� �� ���ִ� Ÿ��
        {
            if (GameManager.Instance.ragdollController.currentState != RagdollController.PlayerState.Falling
                && !VoiceManager.Instance.audio.isPlaying)
            {
                StartCoroutine(nameof(PlayAndDestroy));
            }
        }
        else if(type == Type.Stay) //n�� �̻� ���� �ȿ� ���� �� �ߵ��ϴ� Ʈ���� Ÿ��
        {
            if (other.transform.parent.CompareTag("Player"))
            {
                if (GameManager.Instance.ragdollController.currentState != RagdollController.PlayerState.Falling
                    && !VoiceManager.Instance.audio.isPlaying)
                {
                    timer += Time.deltaTime;
                    if (timer >= stayTime)
                    {
                        stayTime = 999f;
                        StartCoroutine(nameof(PlayAndDestroy));
                    }
                }
            }
        }
    }

    IEnumerator PlayAndDestroy()
    {
        float volume;
        if (VoiceManager.Instance.MapMixer.GetFloat("Map", out volume))
        {
            downVolume = 4f;
        }
        gameObject.GetComponent<BoxCollider>().enabled = false;
        VoiceManager.Instance.MapMixer.SetFloat("Map", volume - downVolume);
        float waitTime = source.clip.length;
        source.Play();
        //��� ���
        SubtitleManager.Instance.SetText(subtitleKey);
        SubtitleManager.Instance.ShowText();
        yield return new WaitForSecondsRealtime(waitTime);
        SubtitleManager.Instance.HideText();
        VoiceManager.Instance.MapMixer.SetFloat("Map", 0f);
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        source = GetComponent<AudioSource>();
    }
}
