using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EagleShot : MonoBehaviour
{
    public GameObject Hiteffect;
    public bool CanBeTrigger = false;
    private bool Effect = true;//����Ʈ ��� false �϶��� ���� �ȴ�
    RagdollController ragdoll;
	private EagleContainer eagle;

	private WaitForSecondsRealtime waitRealOne = new WaitForSecondsRealtime(1f);
	private WaitForSeconds waitTwo = new WaitForSeconds(2f);

	private void Start()
    {
		eagle = GetComponentInParent<EagleContainer>();
    }

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
				if (!eagle.isReturning)
				{
					eagle.BackToOrigin();
				}
			}
		}
	}

	public void EffectSpawn(Vector3 origin)
	{
		Effect = false; // ����� ����
		GameObject tempEffect = Instantiate(Hiteffect);
		tempEffect.transform.position = origin;
		//�ڷ�ƾ���� �����ð� �ڿ� Effect true��
		StartCoroutine(EffectStandby());
		Destroy(tempEffect, 2f);
	}
	public void TriggerOnOFF()
	{
		StartCoroutine(TriggerOn());
	}
	IEnumerator TriggerOn()
	{
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
