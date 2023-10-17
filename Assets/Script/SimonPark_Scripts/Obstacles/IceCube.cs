using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceCube : MonoBehaviour
{
    [SerializeField]
    private float meltingSpeed = 0.01f;

    private GameObject iceCube;
    private Vector3 scale = new Vector3();

    private bool isMelt = false;
    private bool onFoot = false;
    private bool respawn = false;
    private float timer = 0f;
    private Color color;
    [SerializeField]
    private Renderer ICEMesh;
    private Material mat;
    private RagdollController ragdoll;

    WaitForSeconds wait = new WaitForSeconds(3f);
    
    // Start is called before the first frame update
    void Start()
    {
        iceCube = transform.parent.gameObject;
        scale.x = iceCube.transform.localScale.x;
        scale.y = iceCube.transform.localScale.y;
        scale.z = iceCube.transform.localScale.z;
        mat = ICEMesh.material;

        ragdoll = GameManager.Instance.ragdollController;
    }

    private void OnTriggerStay(Collider other)
    {
        //�浹ü�� �÷��̾�� ���� ���̱�
        if(other.gameObject.layer == 6)
        {
            timer = 0f;
            onFoot = true;
            respawn = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //�浹ü�� �÷��̾��
        if (other.gameObject.layer == 6)
        {
            onFoot = false;
            respawn = true;
        }
    }

    private void MeltingCube(float speed)
    {
        scale.y = scale.y > 0f ? scale.y - speed : 0f;
        iceCube.transform.localScale = scale;
        CheckMelt();
    }

    private void CheckMelt()
    {
        if (iceCube.transform.localScale.y <= 0.15f) //�� ������
        {
            InitRespawn(0f);
            iceCube.GetComponent<CheckIce>().freezeDone = false;
            iceCube.GetComponent<BoxCollider>().isTrigger = true;
            gameObject.layer = 3;
            isMelt = true;
        }
    }

    private void InitRespawn(float alpha)
    {
        color = mat.color;
        color.a = alpha;
        mat.color = color;

        scale.y = 1f;
        iceCube.transform.localScale = scale;
    }

    IEnumerator TriggerToCollider()
    {
        yield return wait;
        iceCube.GetComponent<BoxCollider>().isTrigger = false;
        gameObject.GetComponent<BoxCollider>().enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMelt)
        {
            timer += Time.deltaTime;
            if(timer >= 3f)
            {
                color.a = color.a < 1f ? color.a + 0.01f : 1f;
                mat.color = color;
                if (mat.color.a == 1f)
                {
                    iceCube.GetComponent<CheckIce>().freezeDone = true;
                    if (iceCube.GetComponent<CheckIce>().isInIce)
                    {
                        gameObject.GetComponent<BoxCollider>().enabled = false; //�������� ���� ���� �� ��� ������Ʈ ����
                        StartCoroutine(nameof(TriggerToCollider)); //3�� �� ����ť�� �ݶ��̴� Ȱ��ȭ & ������ƮON
                    }
                    else
                    {
                        //�� �������� Ʈ���� ����
                        iceCube.GetComponent<BoxCollider>().isTrigger = false;
                    }
                    gameObject.layer = 0;
                    isMelt = false;
                    timer = 0f;
                }
            }
        }
        else if (onFoot)
        {
            MeltingCube(meltingSpeed);
            //onFoot = false;
        }
        else if (respawn)
        {
            timer += Time.deltaTime;
            if (timer >= 8f)
            {
                timer = 0f;
                respawn = false;
                InitRespawn(1f);
            }
        }
    }
}


