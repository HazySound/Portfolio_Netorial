using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
	public GameObject Hiteffect;
	public bool CanBeTrigger = false;
	private bool Effect = true;//����Ʈ ��� false �϶��� ���� �ȴ�
	RagdollController ragdoll;

	private WaitForSeconds waitTwo = new WaitForSeconds(2f);
	private WaitForSeconds waitPointTwo = new WaitForSeconds(0.2f);
	private WaitForSecondsRealtime waitRealOne = new WaitForSecondsRealtime(1f);

	private void OnCollisionEnter(Collision collision)
	{
		ragdoll = collision.transform.GetComponentInParent<RagdollController>();
		if (ragdoll != null)
		{
			if (ragdoll.transform.CompareTag("Player"))
			{
				ragdoll.CanControl = false;
				ragdoll.OnHit = true;

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
				//.5�� �� Ʈ���� �״ٰ� .5�ڿ� �ٽ� Ʈ���� off 
				if (CanBeTrigger)
				{
					TriggerOnOFF();
				}
			}
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
		GameObject ragdollEffect = Instantiate(Hiteffect);
		float waitTime;
		if (ragdollEffect.GetComponent<AudioSource>() != null)
		{
			waitTime = ragdollEffect.GetComponent<AudioSource>().clip.length;
		}
		else { waitTime = 2f; }
		ragdollEffect.transform.position = origin;
		//�ڷ�ƾ���� �����ð� �ڿ� Effect true��
		StartCoroutine(nameof(EffectStandby));
		Destroy(ragdollEffect, waitTime);
	}
	public void TriggerOnOFF()
	{
		StartCoroutine(TriggerOn());
	}
	IEnumerator TriggerOn()
	{
		yield return waitPointTwo;
		transform.GetComponent<Collider>().isTrigger = true;
		yield return waitTwo;
		transform.GetComponent<Collider>().isTrigger = false;
	}
	IEnumerator EffectStandby()
	{
		yield return waitRealOne;
		Effect = true;
	}
}
