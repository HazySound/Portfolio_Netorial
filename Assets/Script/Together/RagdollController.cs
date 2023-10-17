using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class RagdollController : MonoBehaviourPunCallbacks
{
	//public float tempTimer = 0f;

	public enum PlayerState
	{
		Onground,
		Flying,
		Falling,
	}

	[SerializeField]
	private Portrait portrait;

	public PlayerState currentState;

	[HideInInspector]
	public List<ConfigurableJoint> Joints = new List<ConfigurableJoint>();

	public bool CanControl = true;
	public bool OnHit = false;

	public PhotonView pv;
	public Cameracontroller Cam;

	//public float timer = 0f;

	[HideInInspector]
	public float HitHeight;

	private float GroundHeight;
	private float Failamount;

	private void Start()
	{
		ConfigurableJoint[] allChildren = GetComponentsInChildren<ConfigurableJoint>();
		foreach (ConfigurableJoint child in allChildren)
		{
			// �ڱ� �ڽ��� ��쿣 ���� 
			// (���ӿ�����Ʈ���� �� �ٸ��ٰ� �������� �� ���ϴ� �ڵ�)
			if (child.name == transform.name)
				return;

			Joints.Add(child);
		}
		OnGroundBehaviour();
	}

	public void OnGroundBehaviour()
	{
		pv.RPC(nameof(SpringOnRPC), RpcTarget.All);

		transform.GetComponent<FaceContoller>().MouthControl(0f);
		Cam.SetCameraMain();
		portrait.PortraitOFF();

		GroundHeight = UIManager.Instance.heightcheck.height;
	}

	public void FlytoGround()
    {
		currentState = PlayerState.Onground;

		transform.GetComponent<FaceContoller>().MouthControl(0f);
		Cam.SetCameraMain();
		portrait.PortraitOFF();

		GroundHeight = UIManager.Instance.heightcheck.height;
	}

	[PunRPC]
	public void SpringOnRPC()
    {
		StartCoroutine(nameof(SpringOn));
	}

	public void FlyingBehaviour()
    {
		currentState = PlayerState.Flying;
		Cam.SetCameraFly();
    }

	public void FallingBehaviour()
    {
		pv.RPC(nameof(PlayFallingMusic), RpcTarget.All);

		pv.RPC(nameof(SpringOFF), RpcTarget.All);

		transform.GetComponent<FaceContoller>().MouthControl(100f);
		Cam.SetCameraFalling();
		portrait.PortraitON();
	}

	[PunRPC]
	public void SpringOFF()
	{
		CanControl = false;
		OnHit = true;
		currentState = PlayerState.Falling;

		for (int i = 0; i < Joints.Count; i++)
		{
			JointDrive jointXDrive = Joints[i].angularXDrive;
			jointXDrive.positionSpring = 20f;
			Joints[i].angularXDrive = jointXDrive;

			JointDrive jointYZDrive = Joints[i].angularYZDrive;
			jointYZDrive.positionSpring = 20f;
			Joints[i].angularYZDrive = jointYZDrive;
			//Ű�׸�ƽ�� ������
			Joints[i].GetComponent<Rigidbody>().isKinematic = false;
		}
	}

	IEnumerator SpringOn()
    {
		JointDrive temp = Joints[0].angularXDrive;
		if (temp.positionSpring == 500f) //Fly���� Ground��
		{
			currentState = PlayerState.Onground;
			CanControl = true;
			OnHit = false;
			yield break;
		}
		else if(temp.positionSpring == 20f)//Falling���� Ground�� 
		{
			StartCoroutine(nameof(GetUp_temp));
			//�ڷ�ƾ�� �������� �������� �Ѿ������ ������ ���� �� ���ƿ��� ���� CanControl�� ��������
			//�̰� �����ϱ� ���ؼ� GetUp�� ���� ������ ����Ʈ ������ �� ������ �Ϸ�� ������ ��ٸ��� yield return �� ����.
			yield return new WaitUntil(() => Joints[Joints.Count - 1].angularYZDrive.positionSpring == 500);

			currentState = PlayerState.Onground;
			CanControl = true;
			OnHit = false;

			GameManager.Instance.fallCount++;
			pv.RPC(nameof(SpeakShit), RpcTarget.All);
		}
	}

	IEnumerator GetUp_temp()
	{
		int startValue = 20;
		int endValue = 500;
		float lerpTime = 0.5f;
		float currentTime = 0;
		float currentValue = startValue;

		JointDrive jointXDrive, jointYZDrive;
		
		while (currentValue < endValue)
		{
			currentTime += Time.deltaTime;
			if (currentTime >= lerpTime)
			{
				currentTime = lerpTime;
			}
			currentValue = Mathf.Lerp(startValue, endValue, currentTime / lerpTime);
			for(int i=0; i < Joints.Count; i++)
            {
				jointXDrive = Joints[i].angularXDrive;
				jointYZDrive = Joints[i].angularYZDrive;

				jointXDrive.positionSpring = currentValue;
				Joints[i].angularXDrive = jointXDrive;
				jointYZDrive.positionSpring = currentValue;
				Joints[i].angularYZDrive = jointYZDrive;
			}
			yield return null;
		}
		//���⼭ CanControl�� �ع����� ù ���� ����Ʈ ������ �ǰ� ���� �ٷ� ���� �����ϰ� ��
		//���� �ڵ忡���� ����Ʈ ������ŭ CanControl�� true�� �ƾ��ٰ� �����ϸ� �� ��
	}

	[PunRPC]
	private void PlayFallingMusic()
    {
		StopAllCoroutines();
		//StopCoroutine(nameof(GetUp_temp));
		//StopCoroutine(nameof(SpringOn));
		VoiceManager.Instance.StartFallingMusic();
		//StartCoroutine(VoiceManager.Instance.FallingMusic());
	}

	[PunRPC]
	public void SpeakShit()
	{
		//��� ġ��
		if (HitHeight - GroundHeight >= 150f) //���� 150
		{
			if (GroundHeight < 1f)
			{
				VoiceManager.Instance.playVoice = true;
				if (SubtitleManager.Instance.CheckIsLeft("Reset"))
				{
					VoiceManager.Instance.subtitleKey = "Reset";
				}
				else
				{
					VoiceManager.Instance.subtitleKey = "Eliot";
				}
			}
			else
			{
				VoiceManager.Instance.playVoice = true;
				VoiceManager.Instance.subtitleKey = "SlowWalker";
			}
		}
		else if (GameManager.Instance.fallCount >= 20f) //���� 20
		{
			VoiceManager.Instance.playVoice = true;
			VoiceManager.Instance.subtitleKey = "Person";
		}
		//���� ��� �ǵ�����
		//VoiceManager.Instance.BackToBGM();
		StartCoroutine(VoiceManager.Instance.BackToBGM());

		//���� ����
		HitHeight = 0;
		GroundHeight = 0;
	}
	public void noGravity()
	{
		//Spring off �ϰ� �׷���Ƽ ���ش�
		for (int i = 0; i < Joints.Count; i++)
		{
			Joints[i].GetComponent<Rigidbody>().isKinematic = false;
			Joints[i].GetComponent<Rigidbody>().useGravity = false;
		}
	}
}
