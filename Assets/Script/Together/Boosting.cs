using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Boosting : MonoBehaviourPunCallbacks, IPunObservable
{
    private float _energy = 100f;
    public float energy
    {
        get { return _energy; }
    }

    [SerializeField]
    private TMP_Text text;
    private float timer = 0f;

    public PhotonView pv;

    // Start is called before the first frame update
    void Start()
    {
        text.text = _energy.ToString("0.00");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space))
        {
            pv.RPC("UseEnergy", RpcTarget.All);

        }
        else
        {
            pv.RPC("StackEnergy", RpcTarget.All);
        }

        text.text = _energy.ToString("0.00");
    }

    [PunRPC]
    void UseEnergy()
    {
        _energy = _energy > 0f ? _energy - 0.01f : 0f;
        timer = 0f;
    }

    [PunRPC]
    void StackEnergy()
    {
        timer += Time.deltaTime;
        if (timer >= 1.0f)
        {
            _energy = _energy < 10f ? _energy + 0.01f : 10f;
        }
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
