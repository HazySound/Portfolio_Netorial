using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class AnimationSync : MonoBehaviour,IPunObservable
{
    private Animator anim; // �ִϸ�����
    public float frame;// �ִϸ��̼� Ű �����Ӱ�
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) //�� �����÷��̾��϶� - ���� 1p�� �����÷��̾��� ���°� ���ӵǰ� �־
        {
            //1p ���� �ִϸ��̼� ��� �������� 2p�� ��
            stream.SendNext(anim.GetCurrentAnimatorStateInfo(0).normalizedTime);
        }
        else //�� �����÷��̾ �ƴҶ� - 2p�� �����͸� �޾ƿ��⸸ ��.
        {
            //2P ������ �޾Ƽ� ����
            //this.frame = (float)stream.ReceiveNext();
            anim.Play(0, -1, (float)stream.ReceiveNext());
        }
    }
	private void Start()
	{
        anim = transform.GetComponent<Animator>();
        PhotonNetwork.SendRate = 100;
        if (PhotonNetwork.IsMasterClient)
		{
            //1P �ִ� ����
            anim.PlayInFixedTime(0);
        }
    }
    /***
	private void Update()
	{
		if (!PhotonNetwork.IsMasterClient)
		{
            //2p
            anim.Play(0, -1, frame);
		}
	}
    ***/
}
