using UnityEngine;
using unityremote;

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
            if (isRight)
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

    public override void UpdateState()
    {
        if (GetFrame.currentFrame.Length > 0)
        {
            SetStateAsByteArray(0, "frame", GetFrame.currentFrame);
            if (isRight)
            {
                int r = Ball.instance.GetRightReward();
                SetStateAsFloat(1, "reward", r);
            }
            else
            {
                SetStateAsFloat(1, "reward", Ball.instance.GetLeftReward());
            }
            SetStateAsBool(2, "done", GameManager.isDone);    
        }
    }
    
    public override void ApplyAction()
    {
        switch (GetActionName())
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
