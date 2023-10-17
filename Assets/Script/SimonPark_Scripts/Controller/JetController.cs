using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class JetController : MonoBehaviourPunCallbacks, IPunObservable
{

    [SerializeField]
    private float speed = 10f;

    [Tooltip("Please Tag Right")]
    [SerializeField]
    private GameObject handLeft, legLeft; //�츮�� �� �� ������

    [Tooltip("Please Tag Left")]
    [SerializeField]
    private GameObject handRight, legRight; //�츮�� �� �� ����
    
    [SerializeField]
    private TMP_Text text;
    [SerializeField]
    private Slider Boostbar; 
    private float timer = 0f;
    private float max_energy = 20f;
    private float _energy;
    

    private RagdollController ragdoll;
    public Rigidbody body;
    
    private Vector3 dir = new Vector3();
    private bool boostOn;

    PhotonView pv;

    public List<GameObject> EffectList;

    public GameObject JetEffect_L, JetEffectLeg_L; //�츮�� �� �� ������

    public GameObject JetEffect_R, JetEffectLeg_R; //�츮�� �� �� ����

    private AudioSource booster;

    [SerializeField]
    private GameObject leftArmIce, rightArmIce;

    private AudioSource errorAudio;

    private float step = 0.01f;
    private float stack = 0.02f;

    private int player;

    private bool armBroken = false;

    public bool debug;

    #region �ڽø� �۾� - �ӵ����� ���� ����
    [System.Serializable]
    private struct MaxSpeed
    {
        [SerializeField]
        private float maxX, maxY;

        [HideInInspector]
        public float X
        {
            get { return maxX; }
        }

        [HideInInspector]
        public float Y
        {
            get { return maxY; }
        }
    }

    [Header("Speed Limitation")]
    [SerializeField]
    private MaxSpeed maxSpeed;

    private Vector3 speedLimit = new Vector3();

    #endregion

    void Awake()
    {
        EffectList.Add(JetEffect_L);
        EffectList.Add(JetEffect_R);
        EffectList.Add(JetEffectLeg_L);
        EffectList.Add(JetEffectLeg_R);
		if (debug)
		{
            step = 0f;
		}
    }
    void Start()
    {
        EffectAllOff();
        _energy = max_energy;
        pv = GetComponent<PhotonView>();
        text.text = _energy.ToString("0.00");
        Boostbar.maxValue = _energy;
        Boostbar.value = _energy;
        booster = GetComponent<AudioSource>();
        booster.volume = 1f;

        errorAudio = GetComponentInParent<AudioSource>();

        //1P 2P �Ǻ�
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isleft"]) //left
        {
            player = 1;
        }
        else //right
        {
            player = 2;
        }

        //���� 1P2P ���� �ٲ� �� ���� �ٲٸ� ��
        //���Ѽӵ� ����

        ragdoll = GameManager.Instance.ragdollController;
    }

    public void unlimitedBooster()
    {
        step = 0f;
    }

    public void InfiniteEnergy()
    {
        step = 0.00f;
    }

    public void UsingEnergy()
    {
        step = 0.01f;
    }

    #region �ڽø� �۾� - �Լ�

    #region -----------����Ʈ �� ȿ���� �Լ�-----------

    private void EffectAllOff()
    {
        foreach (GameObject go in EffectList)
        {
            go.SetActive(false);
        }
    }

    private void PlaySound()
    {
        if (!booster.isPlaying)
        {
            booster.Play();
        }
    }

    private void StopSound()
    {
        if (booster.isPlaying)
        {
            booster.Stop();
        }
    }

    private void CheckSound()
    {
        foreach (GameObject effects in EffectList)
        {
            if (effects.activeSelf)
            {
                PlaySound();
                return;
            }
        }
        StopSound();
    }

    private void BoostOnOff(bool isOn, string hand_or_leg)
    {
        int hand_index, leg_index; //�ν��� Ű�� ���� ����Ʈ �ε���
        if (player == 1) //1P�� ��
        {
            hand_index = 0;
            leg_index = 2;
        }
        else //2P�� ��
        {
            hand_index = 1;
            leg_index = 3;
        }

        switch (hand_or_leg)
        {
            case "hand":
                pv.RPC("EffectOff", RpcTarget.All, hand_index, isOn);
                break;
            case "leg":
                pv.RPC("EffectOff", RpcTarget.All, leg_index, isOn);
                break;
        }
    }

    [PunRPC]
    private void EffectOff(int index, bool onOff)
    {
        if (onOff)
        {
            if (!EffectList[index].activeSelf)
            {
                EffectList[index].SetActive(onOff);
            }
        }
        else
        {
            if (EffectList[index].activeSelf) //On
            {
                EffectList[index].SetActive(onOff); //->OFF
            }
        }
    }

    #endregion

    

    #region --------���콺/Ű���� �Է� ���� �Լ�--------

    private void LeftClicked()
    {
        //1. �����Ҵ�
        GameObject hand, leg; //���� ������ �ȴٸ� ������Ʈ
        if (player == 1) //1P�� ��
        {
            hand = handLeft;
            leg = legLeft;
        }
        else //2P�� ��
        {
            hand = handRight;
            leg = legRight;
        }

        //2. �ν��� �Ǻ� �� ����
        if (!Input.GetKey(KeyCode.Space)) //��Ŭ���� ���� ��
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //������ �Һ�
            BoostOnOff(false, "leg");
            BoostOnOff(_energy > 0f, "hand");
            if (_energy > 0f) //�������� ���� �ִٸ�
            {
                dir = dir.z == 0f ? dir + -hand.transform.forward : dir; //������ �޼չٴ� �ݴ�� ����
            }
        }
        else //�����̽��� �����ִٸ� (�����̽� ���� ���¿��� ��Ŭ���� ���Դٸ�)
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //������ �Һ�
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //�� �� ��
            BoostOnOff(_energy > 0f, "hand");
            BoostOnOff(_energy > 0f, "leg");
            if (_energy > 0f) //�������� ���� �ִٸ�
            {
                dir = dir.y == 0f ? dir + ((-hand.transform.forward) + (-leg.transform.up.normalized)) : dir; //���� ���� (�޼չٴ� + �޹߹ٴ� �ݴ�)
            }
        }
    }

    private void SpacePressed()
    {
        //1. �����Ҵ�
        GameObject leg; //���� ������ �ȴٸ� ������Ʈ
        if (player == 1) //1P�� ��
        {
            leg = legLeft;
        }
        else //2P�� ��
        {
            leg = legRight;
        }

        //2. �ν��� �Ǻ� �� ����
        if (!Input.GetMouseButton(0)) //�����̽��� ������ ��
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //������ �Һ�
            BoostOnOff(false, "hand");
            BoostOnOff(_energy > 0f, "leg");
            if (_energy > 0f) //�������� ���� �ִٸ�
            {
                dir = dir.y == 0f ? dir + -leg.transform.up.normalized : dir; //������ �޹߹ٴ� �ݴ�� ����
            }
        }
    }

    #endregion



    #region ---------�̵� �� ������ �Һ� �Լ�---------

        #region * ������ ���� �Լ�
    [PunRPC]
    void UseEnergy()
    {
        timer = 0f;
        _energy = _energy > 0f ? _energy - step : 0f;
    }

    [PunRPC]
    void StackEnergy()
    {
        _energy = _energy < max_energy ? _energy + stack : max_energy;
    }
        #endregion

        #region *�̵� �Լ�
    [PunRPC]
    void Boost(Vector3 dir)
    {
        if (ragdoll.CanControl)
        {
            body.AddForce(dir * speed * Time.fixedDeltaTime, ForceMode.Acceleration);
        }
    }
        #endregion

    #endregion


    #region -------�ӵ� ���� �Լ�-------

    private void LimitSpeed()
    {
        if(body.velocity.x > maxSpeed.X) //x�������� ����
        {
            speedLimit.x = maxSpeed.X;
            speedLimit.y = body.velocity.y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        else if(body.velocity.x < -maxSpeed.X) //-x�������� ����
        {
            speedLimit.x = -maxSpeed.X;
            speedLimit.y = body.velocity.y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        if (body.velocity.y > maxSpeed.Y) //y�������� ����
        {
            speedLimit.x = body.velocity.x;
            speedLimit.y = maxSpeed.Y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        else if (body.velocity.y < -maxSpeed.Y) //-y�������� ����
        {
            speedLimit.x = body.velocity.x;
            speedLimit.y = -maxSpeed.Y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
    }

    #endregion


    #region --------��Ÿ �Լ�--------

    //���� ���� ��ü �ν��� ���Ұ� ������ �����ϸ� �����
    //�ȿ� ���� ������Ʈ Ȱ��ȭ �� �ӵ� ���� (���� ���εǾ��� ���� �ν��� ���⸦ �� ���ϰ� �ؾ� �������� ������ ����)
    public void ArmBroken(bool isFrozen)
    {
        armBroken = isFrozen;
        leftArmIce.SetActive(armBroken);
        rightArmIce.SetActive(armBroken);
        if (isFrozen)
        {
            speed = 4000f;
        }
        else
        {
            speed = 2000f;
        }
    }

    #endregion


    #endregion


    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance.isPause || !ragdoll.CanControl)
        {
            EffectAllOff();
            StopSound();
            return;
        }

        if (Input.GetMouseButton(0)) //��Ŭ�� �Էµ��� �� & ��Ŭ��+�����̽� �Էµ��� ���� ���� �ڵ� ����
        {
            if (armBroken)
            {
                BoostOnOff(false, "hand"); //�� �ν��� ����
                //�Ҹ� Ű��
                if (!errorAudio.isPlaying) { errorAudio.Play(); }
            }
            else
            {
                boostOn = true;
                LeftClicked();
            }
        }
        else if (Input.GetKey(KeyCode.Space)) //��Ŭ��X �����̽��� �Էµ��� �� ����
        {
            boostOn = true;
            SpacePressed();
        }
        else //�� �� �� ������ �� ����
        {
            boostOn = false;
            BoostOnOff(false, "hand");
            BoostOnOff(false, "leg");
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                pv.RPC("StackEnergy", RpcTarget.AllViaServer); //������ ����
            }
        }
        CheckSound();
        text.text = _energy.ToString("0.00");
        //�ν��͹�
        Boostbar.value = _energy;
    }


    private void FixedUpdate()
    {
        if (boostOn)
        {
            pv.RPC("Boost", RpcTarget.AllViaServer, dir);
        }
        LimitSpeed();
        dir = Vector3.zero;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) //�� �����÷��̾��϶� - ���� 1p�� �����÷��̾��� ���°� ���ӵǰ� �־
        {
            stream.SendNext(timer);
        }
        else //�� �����÷��̾ �ƴҶ� - 2p�� �����͸� �޾ƿ��⸸ ��.
        {
            this.timer = (float)stream.ReceiveNext();
        }
    }
}
