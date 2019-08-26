using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;
using System.Text;
using UnityEngine.SceneManagement;


public class Paddle : Agent
{
    private float remoteAxis = 0.0f;

    bool isRight = false;
    
    float height;

    public int frameWidth;
    public int frameHeight;
    
    [SerializeField]
    float speed;

    string input;

    // Start is called before the first frame update
    void Start()
    {
        ResetPlayer();
    }

    void ResetPlayer()
    {
        height = transform.localScale.y;
        Init(isRight);
    }

    // Update is called once per frame
    public override void UpdatePhysics()
    {
        if (!GameManager.paused)
        {
            float move = 0.0f;
            if (isRight || !userControl)
            {
                move = remoteAxis * Time.deltaTime * speed;
            }
            else
            {
                move = Input.GetAxis(input) * Time.deltaTime * speed;
            }
            if (transform.position.y < GameManager.bottomLeft.y + height / 2 && move < 0)
            {
                move = 0;
            }

            if (transform.position.y > GameManager.topRight.y - height / 2 && move > 0)
            {
                move = 0;
            }

            transform.Translate(move * Vector2.up);
            if (this.remoteAxis != 0)
            {
                this.remoteAxis = 0.0f;
            }

        }
    }

    public override object[] LocalDecision()
    {
        string cmd = null;
        if (Ball.instance != null)
        {
            Vector2 pos = Ball.instance.GetPosition();
            if (pos.y - transform.position.y > 0)
            {
                cmd = "Up";
            } else if (pos.y - transform.position.y < 0)
            {
                cmd = "Down";
            }
        }

        return new object[] { cmd, new string[] { } };
    }

    public bool IsRight()
    {
        return isRight;
    }

    public void Init(bool isRightPaddle)
    {
        this.isRight = isRightPaddle;

        Vector2 pos = Vector2.zero;

        if (isRightPaddle)
        {
            pos = new Vector2(GameManager.topRight.x, 0);
            pos -= Vector2.right * transform.localScale.x;
            input = "PaddleRight";
        } else
        {
            pos = new Vector2(GameManager.bottomLeft.x, 0);
            pos += Vector2.right * transform.localScale;
            input = "PaddleLeft";
        }

        transform.position = pos;
        transform.name = input;
    }

    public override bool UpdateState()
    {
        if (GetFrame.currentFrame.Length > 0)
        {
            SetState(0, "frame", Brain.OTHER, System.Convert.ToBase64String(GetFrame.currentFrame));
            if (isRight)
            {
                int r = Ball.instance.GetRightReward();
                SetState(1, "reward", Brain.FLOAT, "" + r);
            }
            else
            {
                SetState(1, "reward", Brain.FLOAT, "" + Ball.instance.GetLeftReward());
            }
            SetState(2, "done", Brain.BOOL, GameManager.isDone ? "1" : "0");
            
            return true;
        }
        return false;
    }
    
    public override void ApplyAction(string cmdname, string[] args)
    {
        switch (cmdname)
        {
            case "GetState":
                 break;
            case "Up":
                Up(); break;
            case "Down":
                Down(); break;
            case "PauseGame":
                PauseGame(); break;
            case "StopGame":
                Stop(); break;
            case "RestartGame":
                RestartGame();break;
            case "ResumeGame":
                ResumeGame(); break;
        }
    }

    void Up()
    {
        if (!GameManager.paused)
        {

                this.remoteAxis += 1.0f;
                if (remoteAxis > 1.0) remoteAxis = 1.0f;
        }
    }

    void Down()
    {
        if (!GameManager.paused)
        {
                this.remoteAxis -= 1.0f;
                if (remoteAxis < -1.0) remoteAxis = -1.0f;
        }
    }

    void Stop()
    {
        GameManager.isDone = true;
    }

    void RestartGame()
    {
        GameManager.Restart();
    }

    void PauseGame()
    {
        GameManager.paused = true;
    }

    void ResumeGame()
    {
        GameManager.paused = false;
    }
}
