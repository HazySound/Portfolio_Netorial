using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class WindManager : MonoBehaviour
{
	public bool WindIsOn;
	public float power; // �ʱⰪ
	public float powerMultiplier; // ������
	public Vector3 direction; //x�� -1�� ->����, 1�� <-����
	private Rigidbody hips;
	private void Start()
	{
		hips = GameManager.Instance.hips.GetComponent<Rigidbody>();
	}
	private void FixedUpdate()
	{
		if (WindIsOn)
		{
			if (hips != null)
			{
				hips.AddForce(direction * power* powerMultiplier * Time.fixedDeltaTime);
			}
		}
	}
}
