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
    private GameObject handLeft, legLeft; //우리가 볼 때 오른쪽

    [Tooltip("Please Tag Left")]
    [SerializeField]
    private GameObject handRight, legRight; //우리가 볼 때 왼쪽
    
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

    public GameObject JetEffect_L, JetEffectLeg_L; //우리가 볼 때 오른쪽

    public GameObject JetEffect_R, JetEffectLeg_R; //우리가 볼 때 왼쪽

    private AudioSource booster;

    [SerializeField]
    private GameObject leftArmIce, rightArmIce;

    private AudioSource errorAudio;

    private float step = 0.01f;
    private float stack = 0.02f;

    private int player;

    private bool armBroken = false;

    public bool debug;

    #region 박시몬 작업 - 속도제한 관련 변수
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

        //1P 2P 판별
        if ((bool)PhotonNetwork.LocalPlayer.CustomProperties["isleft"]) //left
        {
            player = 1;
        }
        else //right
        {
            player = 2;
        }

        //추후 1P2P 설정 바꿀 때 여기 바꾸면 됨
        //제한속도 설정

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

    #region 박시몬 작업 - 함수

    #region -----------이펙트 및 효과음 함수-----------

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
        int hand_index, leg_index; //부스터 키고 꺼줄 이펙트 인덱스
        if (player == 1) //1P일 때
        {
            hand_index = 0;
            leg_index = 2;
        }
        else //2P일 때
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

    

    #region --------마우스/키보드 입력 관련 함수--------

    private void LeftClicked()
    {
        //1. 변수할당
        GameObject hand, leg; //방향 가져올 팔다리 오브젝트
        if (player == 1) //1P일 때
        {
            hand = handLeft;
            leg = legLeft;
        }
        else //2P일 때
        {
            hand = handRight;
            leg = legRight;
        }

        //2. 부스터 판별 및 실행
        if (!Input.GetKey(KeyCode.Space)) //좌클릭만 했을 때
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //에너지 소비
            BoostOnOff(false, "leg");
            BoostOnOff(_energy > 0f, "hand");
            if (_energy > 0f) //에너지가 남아 있다면
            {
                dir = dir.z == 0f ? dir + -hand.transform.forward : dir; //방향을 왼손바닥 반대로 설정
            }
        }
        else //스페이스가 눌려있다면 (스페이스 누른 상태에서 좌클릭이 들어왔다면)
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //에너지 소비
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //한 번 더
            BoostOnOff(_energy > 0f, "hand");
            BoostOnOff(_energy > 0f, "leg");
            if (_energy > 0f) //에너지가 남아 있다면
            {
                dir = dir.y == 0f ? dir + ((-hand.transform.forward) + (-leg.transform.up.normalized)) : dir; //벡터 연산 (왼손바닥 + 왼발바닥 반대)
            }
        }
    }

    private void SpacePressed()
    {
        //1. 변수할당
        GameObject leg; //방향 가져올 팔다리 오브젝트
        if (player == 1) //1P일 때
        {
            leg = legLeft;
        }
        else //2P일 때
        {
            leg = legRight;
        }

        //2. 부스터 판별 및 실행
        if (!Input.GetMouseButton(0)) //스페이스만 눌렀을 때
        {
            pv.RPC("UseEnergy", RpcTarget.AllViaServer); //에너지 소비
            BoostOnOff(false, "hand");
            BoostOnOff(_energy > 0f, "leg");
            if (_energy > 0f) //에너지가 남아 있다면
            {
                dir = dir.y == 0f ? dir + -leg.transform.up.normalized : dir; //방향을 왼발바닥 반대로 설정
            }
        }
    }

    #endregion



    #region ---------이동 및 에너지 소비 함수---------

        #region * 에너지 관련 함수
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

        #region *이동 함수
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


    #region -------속도 제한 함수-------

    private void LimitSpeed()
    {
        if(body.velocity.x > maxSpeed.X) //x방향으로 과속
        {
            speedLimit.x = maxSpeed.X;
            speedLimit.y = body.velocity.y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        else if(body.velocity.x < -maxSpeed.X) //-x방향으로 과속
        {
            speedLimit.x = -maxSpeed.X;
            speedLimit.y = body.velocity.y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        if (body.velocity.y > maxSpeed.Y) //y방향으로 과속
        {
            speedLimit.x = body.velocity.x;
            speedLimit.y = maxSpeed.Y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
        else if (body.velocity.y < -maxSpeed.Y) //-y방향으로 과속
        {
            speedLimit.x = body.velocity.x;
            speedLimit.y = -maxSpeed.Y;
            speedLimit.z = body.velocity.z;

            body.velocity = speedLimit;
        }
    }

    #endregion


    #region --------기타 함수--------

    //설산 맵의 상체 부스터 사용불가 영역에 진입하면 실행됨
    //팔에 얼음 오브젝트 활성화 및 속도 조절 (팔이 봉인되었을 때는 부스터 세기를 더 강하게 해야 정상적인 비행이 가능)
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

        if (Input.GetMouseButton(0)) //좌클릭 입력됐을 때 & 좌클릭+스페이스 입력됐을 때는 여기 코드 실행
        {
            if (armBroken)
            {
                BoostOnOff(false, "hand"); //팔 부스터 끄기
                //소리 키기
                if (!errorAudio.isPlaying) { errorAudio.Play(); }
            }
            else
            {
                boostOn = true;
                LeftClicked();
            }
        }
        else if (Input.GetKey(KeyCode.Space)) //좌클릭X 스페이스만 입력됐을 때 실행
        {
            boostOn = true;
            SpacePressed();
        }
        else //둘 다 안 눌렸을 때 실행
        {
            boostOn = false;
            BoostOnOff(false, "hand");
            BoostOnOff(false, "leg");
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                pv.RPC("StackEnergy", RpcTarget.AllViaServer); //에너지 충전
            }
        }
        CheckSound();
        text.text = _energy.ToString("0.00");
        //부스터바
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
        if (stream.IsWriting) //내 로컬플레이어일때 - 지금 1p가 로컬플레이어인 상태가 지속되고 있어서
        {
            stream.SendNext(timer);
        }
        else //내 로컬플레이어가 아닐때 - 2p가 데이터를 받아오기만 함.
        {
            this.timer = (float)stream.ReceiveNext();
        }
    }
}
