using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ball : MonoBehaviour
{


    [SerializeField]
    float speed;
    public float timeToStart=1.0f;
    private Text localPlayerScore;
    private Text remotePlayerScore;
    private static int leftPlayerWins = 0;
    private static int rightPlayerWins = 0;
    public static Ball instance = null;
    public float radius;
    private float acmTime = 0.0f;
    private int leftReward = 0, rightReward = 0;

    Vector2 direction;

    public Vector2 GetDirection()
    {
        return direction;
    }

    public Vector2 GetPosition()
    {
        return transform.position;
    }

    public int GetRightReward()
    {
        int r = rightReward;
        rightReward = 0;
        return r;
    }

    public int GetLeftReward()
    {
        int r =  leftReward;
        leftReward = 0;
        return r;
    }

    // Start is called before the first frame update
    void Start()

    {
        acmTime = timeToStart;
        localPlayerScore = GameObject.Find("LocalPlayerScore").GetComponent<Text>();
        remotePlayerScore = GameObject.Find("RemotePlayerScore").GetComponent<Text>();
        ResetBall();
        instance = this;
    }

    void ResetBall()
    {
        transform.Translate(-transform.position.x, -transform.position.y, -transform.position.z);

        if (Random.value < 0.5)
            direction = Vector2.one.normalized;
        else
        {
            direction = Vector2.one.normalized;
            direction.x = -direction.x;
            direction.y = -direction.y;
            direction = direction.normalized;
        }

        radius = transform.localScale.x / 2;
        acmTime = 0.0f;
    }
    

    // Update is called once per frame
    public void FixedUpdate()
    {
        acmTime += Time.deltaTime;

        if (!GameManager.paused && !GameManager.isDone && acmTime >= timeToStart)
        {            
            transform.Translate(direction * speed * Time.deltaTime);

            if (transform.position.y < GameManager.bottomLeft.y + radius && direction.y < 0)
            {
                direction.y = -direction.y;
            }

            if (transform.position.y > GameManager.topRight.y - radius && direction.y > 0)
            {
                direction.y = -direction.y;
            }

            if (transform.position.x < GameManager.bottomLeft.x + radius && direction.x < 0)
            {
                leftReward = -1;
                rightPlayerWins++;
                ResetBall();
                stop();
            }
            else if (transform.position.x > GameManager.topRight.x - radius && direction.x > 0)
            {
                rightReward = -1;
                leftPlayerWins++;
                ResetBall();
                stop();
            }

            localPlayerScore.text = "Score: " + leftPlayerWins;
            remotePlayerScore.text = "Score: " + rightPlayerWins;
        }
    }

    private void stop()
    {
        GameManager.isDone = true;
        enabled = false;
    }

    void Replay() {
        enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!GameManager.paused && !GameManager.isDone)
        {
            if (collision.tag.Equals("Paddle"))
            {
                bool isRight = collision.gameObject.GetComponent<Paddle>().IsRight();
                if (isRight && direction.x > 0)
                {
                    direction.x = -direction.x;
                    rightReward = 1;
                }
                else if (!isRight && direction.x < 0)
                {
                    direction.x = -direction.x;
                    leftReward = 1;
                }
            }
        }
    }
}
