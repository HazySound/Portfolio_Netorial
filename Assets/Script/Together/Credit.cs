using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class Credit : MonoBehaviour
{
	public void CreditEnd()
	{
		StartCoroutine(nameof(BackToLobby));
	}

	private IEnumerator BackToLobby()
    {
		VoiceManager.Instance.EndCreditMusic(); //ũ���� ���� ���̵�ƿ�
		yield return new WaitForSecondsRealtime(1f);
		//�κ� ������ ���ư�
		PhotonNetwork.LeaveRoom();
	}

	public void CreditStart()
    {
		StartCoroutine(VoiceManager.Instance.StartCreditMusic());
    }
}
