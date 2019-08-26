using UnityEngine;
using unityremote;

public class GameManager : MonoBehaviour
{

    public Ball ball;
    public Paddle paddle;


    public static Vector2 bottomLeft;
    public static Vector2 topRight;

    public static bool paused = false;
    public static bool isDone = false;

    private Paddle paddle1;
    private Paddle paddle2;

    public float topPad;
    public float rightPad;

    private static GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 2.0f;
        bottomLeft = Camera.main.ScreenToWorldPoint(new Vector2(0, 0));
        topRight = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width-rightPad, Screen.height-topPad));

        ball = Instantiate(ball);
        paddle1 = Instantiate(paddle) as Paddle;
        paddle2 = Instantiate(paddle) as Paddle;
        paddle1.numberOfFields = 3;
        paddle2.numberOfFields = 3;
        paddle1.gameObject.AddComponent<RemoteBrain>();
        RemoteBrain remote = paddle1.GetComponent<RemoteBrain>();
        paddle2.gameObject.AddComponent<LocalBrain>();
        paddle2.userControl = false;
        LocalBrain local = paddle2.GetComponent<LocalBrain>();
        remote.agent = paddle1;
        paddle1.SetBrain(remote);
        paddle1.Init(true); //Is Right Paddle = True
        local.agent = paddle2;
        paddle2.SetBrain(local);
        paddle2.Init(false); //Is Right Paddle = False
        gameManager = this;
    }

    public static void Restart()
    {
        gameManager.RestartGame();
    }

    public void RestartGame()
    {
        paused = false;
        isDone = false;
        ball.SendMessage("ResetBall");
        ball.SendMessage("Replay");
        paddle1.SendMessage("ResetPlayer");
        paddle2.SendMessage("ResetPlayer");

    }
}
