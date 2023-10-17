using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndingSequence : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		//HUD ���ְ� ���� �ν��� ON
		UIManager.Instance.boosterUI.gameObject.SetActive(false);
		UIManager.Instance.BodyUI.SetActive(false);
		UIManager.Instance.HUD.SetActive(false);

		GameManager.Instance.jetCon.unlimitedBooster(); //���� �ν���
		GameManager.Instance.ragdollController.noGravity();
	}
	private void OnTriggerExit(Collider other)
	{
		if(GameManager.Instance.hips.transform.position.y < this.transform.position.y)
		{
			//HUD ���ְ� ���� �ν��� ON
			UIManager.Instance.boosterUI.gameObject.SetActive(true);
			UIManager.Instance.BodyUI.SetActive(true);
			UIManager.Instance.HUD.SetActive(true);

			GameManager.Instance.jetCon.debug = false; //���� �ν���
		}

	}
}
