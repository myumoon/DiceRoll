using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Saikoro : MonoBehaviour
{
    enum State
    {
        Stop,
        Playing,
        Decide,
    }
    [SerializeField]
    private bool  m_playOnAwake = true;

    [SerializeField]
    private float m_rotSpeedMain = 3.0f;

    [SerializeField]
    private float m_rotSpeedSub  = 1.0f;

    [SerializeField]
    private float m_rotAxisSpeed = 1.0f;

    [SerializeField]
    private float m_decideInterTime = 1.0f; // 出目が出てから停止までの補間時間

    [SerializeField]
    private int   m_diceRollValue = 0; // 出目固定(1~5)

    private State      m_state           = State.Stop;
    private float      m_currentTime     = 0.0f;
    private Quaternion m_finalQuat       = Quaternion.identity;
    private float      m_interpolateTime = 0.0f;

    void Awake()
    {
        if(m_playOnAwake) {
            m_state = State.Playing;
        }
    }

    void Update()
    {
        UpdateInput();
        UpdateRot();
    }

    void UpdateInput()
    {
        if (Input.GetMouseButtonDown(0)) {
            switch (m_state) {
                case State.Stop:
                case State.Decide:
                    m_state = State.Playing;
                    break;
                case State.Playing:
                    m_state     = State.Decide;
                    if (0 < m_diceRollValue && m_diceRollValue <= 6) {
                        Debug.Log(string.Format("Fixed dice roll = {0}(index:{1})", m_diceRollValue, m_diceRollValue - 1));
                        SetDiceRoll(m_diceRollValue - 1); // 出目を入れる(本来は外から設定すべき)
                    }
                    else {
                        // 出目を入れる(本来は外から設定すべき)
                        int randValue = Random.Range(0, 6);
                        Debug.Log(string.Format("Rand dice roll = {0}(index:{1})", randValue + 1, randValue));
                        SetDiceRoll(randValue); 
                    }
                    break;
                default:
                    break;
            }
        }
    }

    void UpdateRot()
    {
        switch (m_state) {
            // ランダムに適当に回転させる
            case State.Playing:
                m_currentTime += Time.deltaTime;

                // メインの回転軸
                float   rotAxisMainValue = Mathf.Sin(m_currentTime * Mathf.Deg2Rad * m_rotAxisSpeed) + 1.0f * 0.5f; // 0-1の間のsinカーブ
                Vector3 rotAxisMainVec   = Quaternion.Euler(0.0f, 0.0f, rotAxisMainValue * 360.0f) * Vector3.right;
                transform.Rotate(rotAxisMainVec, m_rotSpeedMain);

                // サブの回転軸
                float   rotAxisSubValue = 1.0f - rotAxisMainValue;
                Vector3 rotAxisSubVec   = Quaternion.Euler(rotAxisSubValue * 360.0f, 0.0f, 0.0f) * Vector3.up;
                transform.Rotate(rotAxisSubVec, m_rotSpeedSub);
                break;

            // 決定したダイスの面を画面に向ける
            case State.Decide:
                m_interpolateTime += Time.deltaTime;
                if(m_decideInterTime <= m_interpolateTime) {
                    m_state = State.Stop;
                }
                float t = EaseOutCubic(m_interpolateTime / m_decideInterTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, m_finalQuat, t);
                break;

            default:
                break;
        }
    }

    public void SetDiceRoll(int diceRoll)
    {
        // 最終姿勢(サイコロモデルによる)
        Quaternion[] quatTbl = {
            Quaternion.Euler(0.0f, -90.0f, 0.0f), // 1
            Quaternion.Euler(180.0f, 0.0f, 0.0f), // 2
            Quaternion.Euler(-90.0f, 0.0f, 0.0f), // 3
            Quaternion.Euler(90.0f, 0.0f, 0.0f),  // 4
            Quaternion.Euler(0.0f, 0.0f, 0.0f),   // 5
            Quaternion.Euler(0.0f, 90.0f, 0.0f),  // 6
        };
        if (diceRoll < 0 || quatTbl.Length <= diceRoll) {
            return;
        }

        m_finalQuat = quatTbl[diceRoll];
        m_interpolateTime = 0.0f;
    }

    float EaseOutCubic(float x)
    {
        return 1 - Mathf.Pow(1 - x, 3);
    }
}
