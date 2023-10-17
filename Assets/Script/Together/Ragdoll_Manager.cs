using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Ragdoll_Manager : MonoBehaviour
{
	private enum PlayerState 
	{
		Playing,
		Ragdoll,
		WakingUp
	}
	private PlayerState currentState = PlayerState.Playing;
	public GameObject Arm_L; //ȸ���� ���� �Ǵ� ������Ʈ 
	public GameObject Arm_R;
	//�� �κ��� �߷� ���� Kinematic üũ
	//�ٸ���?
	public GameObject Leg_L;
	public GameObject Leg_R;
	public List<GameObject> Ragdollbits = new List<GameObject>();
	public List<GameObject> PlayerableJoint = new List<GameObject>();
	public  Animator animator;
	private Transform hipsBone;
	public float WakeUpTime;
	public bool IsRagdollOn;
	public GameObject Player_Ragdoll;
	//������
	public TextMeshProUGUI text;
	private void Awake()
	{
		hipsBone = animator.GetBoneTransform(HumanBodyBones.Hips);
	}
	public void Start()
	{
		//��ĵ
		Rigidbody[] allChildren = Player_Ragdoll.GetComponentsInChildren<Rigidbody>();
		foreach (Rigidbody child in allChildren)
		{
			// �ڱ� �ڽ��� ��쿣 ���� 
			// (���ӿ�����Ʈ���� �� �ٸ��ٰ� �������� �� ���ϴ� �ڵ�)
			if (child.name == transform.name)
				return;

			Ragdollbits.Add(child.gameObject);
			if(child.tag == "PlayerableJoint")
			{
				PlayerableJoint.Add(child.gameObject);
			}
		}
		GameModeOn();
	}
	private void Update()
	{
		switch (currentState)
		{
			case PlayerState.Playing: // �÷��̾ �����ϴ� ���� //���Ӹ�� ON, �ִϸ����� OFF
				PlayingBehaviour();
				break;
			case PlayerState.Ragdoll: // �÷��̾� ���� �Ұ� // ���Ӹ�� OFF, �ִϸ����� OFF
				RadgollBehaviour();
				break;
			case PlayerState.WakingUp: // ���׵����� �Ͼ�� �ִϸ��̼� // �ִϸ����� ON // Ragdoll -> WakingUp -> Playing �̷� ����
				WakingUpBehaviour();
				break;
		}
		text.text = currentState.ToString();
	}
	public void RagdollButton()
	{
		IsRagdollOn = true;
	}
	public void RagdollOFF()
	{
		//���� ���׵� ���� -> ���� �ֳ�?
		//���� ������Ʈ���� �ݶ��̴� ����, ������ٵ� �߷� ���� �ִϸ����� �ѱ�
		IsRagdollOn = false;
		for (int i =0; i < Ragdollbits.Count; i++)
		{
			Ragdollbits[i].gameObject.GetComponent<Rigidbody>().useGravity = false;
			Ragdollbits[i].gameObject.GetComponent<Rigidbody>().isKinematic = true;
			Ragdollbits[i].gameObject.GetComponent<Collider>().enabled = false;
		}
		animator.enabled = true;
	}
	public void RagdollOn()
	{
		//���� ���׵�
		//���� ������Ʈ���� �ݶ��̴� Ű��, �߷� Ű��. �ִϸ����� ����
		for (int i = 0; i < Ragdollbits.Count; i++)
		{
			Ragdollbits[i].gameObject.GetComponent<Rigidbody>().useGravity = true;
			Ragdollbits[i].gameObject.GetComponent<Rigidbody>().isKinematic = false;
			Ragdollbits[i].gameObject.GetComponent<Collider>().enabled = true;
		}
		animator.enabled = false;
		//�Ͼ�� 
		WakeUpTime = Random.Range(3, 5);
	}
	public void GameModeOn()
	{
		//ó�� ���۽�, �Ѿ����ٰ� �Ͼ ����
		//�����Ҽ� �ְ� �Ȱ� �ٸ��κ� ������ٵ� ����
		for (int i = 0; i < PlayerableJoint.Count; i++)
		{
			PlayerableJoint[i].gameObject.GetComponent<Rigidbody>().useGravity = false;
			PlayerableJoint[i].gameObject.GetComponent<Rigidbody>().isKinematic = true;
			//PlayerableJoint[i].gameObject.GetComponent<Collider>().enabled = false;
		}
	}
	public void GameModeOff()
	{
		//��ֹ��� �ε����ų� ������ ���// ��ü ���׵�ȭ �ϴ°Ͱ� ���� ����
		for (int i = 0; i < PlayerableJoint.Count; i++)
		{
			PlayerableJoint[i].gameObject.GetComponent<Rigidbody>().useGravity = true;
			PlayerableJoint[i].gameObject.GetComponent<Rigidbody>().isKinematic = false;
			PlayerableJoint[i].gameObject.GetComponent<Collider>().enabled = true;
		}
	}
	public void PlayingBehaviour() 
	{
		//�÷��̾ �������� ����

		//���콺 �Է� ������ �ִϸ����� ����?
		if (Input.GetMouseButton(0))
		{
			animator.enabled = false;
		}
		else
		{
			animator.enabled = true;
		}

		if (IsRagdollOn)
		{
			RagdollOn();
			currentState = PlayerState.Ragdoll;
		}
	}
	public void RadgollBehaviour()
	{
		//��ֹ� � ��Ƽ� ���׵�ȭ �� ����
		WakeUpTime -= Time.deltaTime;
		if(WakeUpTime <= 0)
		{
			AlignPositionToHips();
			//Hips �� z �����̼��� ������� ��� ��Ȳ���� 
			StandingDetect();
			currentState = PlayerState.WakingUp;
			//�ִϸ����� On 
			RagdollOFF();
			animator.SetTrigger("WakeUp");
		}
	}
	public void WakingUpBehaviour()
	{
		//�Ͼ�� �ִϸ��̼� ��� ��
		if (animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"))
		{
			//AlignPositionToHips();
			currentState = PlayerState.Playing;
			GameModeOn();
		}
	}
	private void AlignPositionToHips()
	{
		//������ ��ġ �����༭ �ִϸ��̼� ��ġ �Ȱ���
		Vector3 OriginalHipsPosition = hipsBone.position;
		transform.position = hipsBone.position;

		if(Physics.Raycast(transform.position,Vector3.down,out RaycastHit hitInfo))
		{
			transform.position = new Vector3(transform.position.x, hitInfo.point.y, transform.position.z);
		}
		hipsBone.position = OriginalHipsPosition;
	}
	private void StandingDetect()
	{
		Vector3 rootBoneForward = hipsBone.rotation * Vector3.forward;
		if (Vector3.Dot(rootBoneForward, Vector3.down) >= 0f)
		{
			Debug.Log("������ �Ѿ���");
			animator.SetInteger("ID", 1);
		}
		else
		{
			Debug.Log("�ڷ� �Ѿ���");
			animator.SetInteger("ID", 2);
		}
		/**
		if (Vector3.Dot(rootBoneForward, Vector3.down) >= 0f) // Check if ragdoll is lying on its back or front, then transition to getup animation
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).fullPathHash.Equals(hash.getupFront))
				animator.SetBool(hash.frontTrigger, true);
			else // if (!anim.GetCurrentAnimatorStateInfo(0).IsName("GetupFrontMirror"))
				animator.SetBool(hash.frontMirrorTrigger, true);
		}
		else
		{
			if (!animator.GetCurrentAnimatorStateInfo(0).fullPathHash.Equals(hash.getupBack))
				animator.SetBool(hash.backTrigger, true);
			else // if (!anim.GetCurrentAnimatorStateInfo(0).IsName("GetupFrontMirror"))
				animator.SetBool(hash.backMirrorTrigger, true);
		}
		**/
	}
}
