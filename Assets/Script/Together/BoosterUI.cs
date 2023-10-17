using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoosterUI : MonoBehaviour
{
    public TextMeshProUGUI BoosterText; // ���� �ؽ�Ʈ
    public Slider BoosterBar;
	public GameObject Alert;
	private void FixedUpdate()
	{
		if(BoosterBar.value < 3f)
		{
			Alert.SetActive(true);
		}
		else
		{
			Alert.SetActive(false);
		}
	}
}
