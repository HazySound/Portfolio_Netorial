using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdArea : MonoBehaviour
{
	public GameObject Hiteffect;
	private bool Effect = true;//����Ʈ ��� false �϶��� ���� �ȴ�
	RagdollController ragdoll;
	[HideInInspector]
	public bool isTriggered = false;
	private AudioSource audio;
	[HideInInspector]
	public float timer = 0f;
	private List<BirdStrike> birds = new List<BirdStrike>();
	[HideInInspector]
	public bool isAllReturned = false;
	private WaitForSecondsRealtime waitRealOne = new WaitForSecondsRealtime(1f);

	private void Start()
    {
		audio = GetComponent<AudioSource>();
		foreach(Transform child in transform)
        {
			birds.Add(child.GetComponent<BirdStrike>());
        }
	}

	private void OnTriggerEnter(Collider collision)
	{
		ragdoll = collision.transform.GetComponentInParent<RagdollController>();
		if (ragdoll != null)
		{
			if (ragdoll.transform.CompareTag("Player"))
			{
				ragdoll.CanControl = false;
				ragdoll.OnHit = true;

				if (!isTriggered)
				{
					isTriggered = true;
					audio.Play();
				}
				if (ragdoll.currentState != RagdollController.PlayerState.Falling) //���ư��ų� �±׶��� ������ �� ������ ������ onHit ���ֱ� (falling�϶��� ����Ʈ�� ���)
				{
					//VoiceManager.Instance.hitMap = VoiceManager.Instance.currentMap;
					//VoiceManager.Instance.FallingMusic();
					//StartCoroutine(VoiceManager.Instance.FallingMusic());
					ragdoll.currentState = RagdollController.PlayerState.Falling;
					ragdoll.FallingBehaviour();
					ragdoll.HitHeight = Mathf.Max(ragdoll.HitHeight, UIManager.Instance.heightcheck.height);
				}
				else
				{
					ragdoll.FallingBehaviour();
				}
				//����Ʈ
				if (Hiteffect != null && Effect)
				{
					//�浹�� ��ġ���ٰ� ����Ʈ ����
					EffectSpawn(collision.transform.position);
				}
			}
		}
	}

	public void EffectSpawn(Vector3 origin)
	{
		Effect = false; // ����� ����
		GameObject tempEffect = Instantiate(Hiteffect);
		float waitTime;
		if (tempEffect.GetComponent<AudioSource>() != null)
		{
			waitTime = tempEffect.GetComponent<AudioSource>().clip.length;
		}
		else { waitTime = 2f; }
		tempEffect.transform.position = origin;
		//�ڷ�ƾ���� �����ð� �ڿ� Effect true��
		StartCoroutine(EffectStandby());
		Destroy(tempEffect, waitTime);
	}

	IEnumerator EffectStandby()
	{
		yield return waitRealOne;
		Effect = true;
	}

	public void IsAllBirdReturned()
    {
		foreach(BirdStrike bird in birds)
        {
            if (!bird.isDone) {
				return;
			}
        }
		isTriggered = false;
		foreach (BirdStrike bird in birds)
		{
			bird.isDone = false;
			bird.timer = 0f;
		}
	}
}
