using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceContoller : MonoBehaviour
{
    public SkinnedMeshRenderer Face;
    public void MouthControl(float value)
	{
		Face.SetBlendShapeWeight(0, value); // 100�� �Թ��� 0�� ����
	}
}
