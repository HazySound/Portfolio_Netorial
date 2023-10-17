using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerController : MonoBehaviourPunCallbacks
{    
    [SerializeField]
    private Slider slider_L, slider_R;
    [SerializeField]
    private Slider slider_leg_L, slider_leg_R;

    private readonly float minVal = 0;
    private readonly float maxVal = 100;
    private float mousePos;

    private PhotonView pv;
    private Animator anim;
    private int player;

    // Start is called before the first frame update
    void Start()
    {
        pv = GetComponent<PhotonView>();
        anim = GetComponent<Animator>();

        anim.speed = 0;

        slider_L.minValue = 0;
        slider_L.maxValue = Screen.height;

        slider_R.minValue = 0;
        slider_R.maxValue = Screen.height;

        slider_leg_L.minValue = minVal;
        slider_leg_L.maxValue = maxVal;

        slider_leg_R.minValue = minVal;
        slider_leg_R.maxValue = maxVal;

        if((bool)PhotonNetwork.LocalPlayer.CustomProperties["isleft"]) //left
        {
            player = 1;

            slider_L.onValueChanged.AddListener(delegate { PushData_PL(); });
            slider_leg_L.onValueChanged.AddListener(delegate { PushData_PL(); });
        }
        else //right
        {
            player = 2;

            slider_R.onValueChanged.AddListener(delegate { PushData_PR(); });
            slider_leg_R.onValueChanged.AddListener(delegate { PushData_PR(); });
        }
    }

    void Update()
    {
        slider_L.maxValue = Screen.height;
        slider_R.maxValue = Screen.height;

        if (GameManager.Instance.isPause) return;

        //¸¶¿ì½º ÀÎÇ²À¸·Î ½½¶óÀÌ´õ°ªÁ¶Á¤. -> 1p 2p ½½¶óÀÌ´õ °¢°¢ ³ª´²Áà¾ß µÊ
        
        mousePos = Input.mousePosition.y;

        if (player == 1) //1PÀÇ °æ¿ì
        {
            slider_L.value = mousePos;
            if (Input.GetKey(KeyCode.A))
            {
                slider_leg_L.value = maxVal;//µþ±ï
            }
            else if (Input.GetKey(KeyCode.D))
            {
                slider_leg_L.value = minVal;//µþ±ï
            }
        }
        else //2PÀÇ °æ¿ì
        {
            slider_R.value = mousePos;
            if (Input.GetKey(KeyCode.A))
            {
                slider_leg_R.value = minVal;//µþ±ï
            }
            else if (Input.GetKey(KeyCode.D))
            {
                slider_leg_R.value = maxVal;//µþ±ï
            }
        }
    }

    void PushData_PL()
    {
        pv.RPC("Hand_R", RpcTarget.AllViaServer, slider_L.normalizedValue);
        pv.RPC("Leg_R", RpcTarget.AllViaServer, slider_leg_L.normalizedValue);
    }

    void PushData_PR()
    {
        pv.RPC("Hand_L", RpcTarget.AllViaServer, slider_R.normalizedValue);
        pv.RPC("Leg_L", RpcTarget.AllViaServer, slider_leg_R.normalizedValue);
    }

    #region PunRPC
    [PunRPC]
    void Hand_L(float value)
    {
        anim.Play("hand_L", -1, value);
    }

    [PunRPC]
    void Hand_R(float value)
    {
        anim.Play("hand_R", -1, value);
    }

    [PunRPC]
    void Leg_L(float value)
    {
        anim.Play("LegL", -1, value);
    }

    [PunRPC]
    void Leg_R(float value)
    {
        anim.Play("LegR", -1, value);
    }
    #endregion
}
