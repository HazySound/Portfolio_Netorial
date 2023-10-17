using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmBrokenTrigger : MonoBehaviour
{
    private int collCount = 0;
    [SerializeField]
    private JetController jetCon;
    private Transform hips;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 6)
        {
            collCount++;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.layer == 6)
        {
            collCount--;
            if(collCount == 0)
            {
                if(hips.position.y > 200f) //���� Ʈ���� �������� Ż���ߴٸ�
                {
                    jetCon.ArmBroken(true); //���� ON
                }
                else //�Ʒ������� ��������
                {
                    jetCon.ArmBroken(false); //���� OFF
                }
                
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        hips = GameManager.Instance.hips.transform;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
