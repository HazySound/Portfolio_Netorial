using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ending : MonoBehaviour
{
	private RagdollController temp;
	private void Start()
	{
		temp = GameManager.Instance.ragdollController;
	}
	private void OnTriggerEnter(Collider collision)
	{
		GameManager.Instance.isEnding = true;
	}
	private void FixedUpdate()
	{
		if (GameManager.Instance.isEnding)
		{
			//temp.CanControl = false;
			//temp.noGravity();
			//�ν��� ����Ʈ ���� Ȱ��ȭ
			//������addforce �ֱ�, ���൵ �ɵ�
			GameManager.Instance.maincamera.GetComponent<Cameracontroller>().EndingCamera();
			UIManager.Instance.EndingTitle.SetActive(true);
		}
	}
}
