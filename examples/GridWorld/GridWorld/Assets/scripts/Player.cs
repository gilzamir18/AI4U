using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class Player : Agent
{

    public Game game;
    public int id = 0;

    private bool done;

    public float reward = 0.0f;

    public int lifes = 3;

    private int count_lifes;


    private int touch;

    public void Start() {
        count_lifes = lifes;
    }

    public override void ApplyAction() {
        reward = 0.0f;
        touch = 0;
        string action = GetActionName();
        if (action == "move") {
            if (!done){
                int dir = GetActionArgAsInt();
                reward = game.Act((ACTION) dir, out touch );
                if (reward < 0) {
                    count_lifes--;
                } else if (reward > 0) {
                    count_lifes++;
                }

                if (count_lifes <= 0) {
                    done = true;
                    count_lifes = lifes;
                    ctime = 0;
                    game.ResetPlayer();
                }
            }
        } else if (action == "restart") {
            count_lifes = lifes;
            ctime = 0;
            done = false;
            reward = 0;
            touch = 0;
            game.ResetPlayer();
        } else {
            //Debug.Log("Getting information " + action);
        }
    }


    private float ctime = 0.0f;

    public override void UpdatePhysics(){
        ctime += Time.deltaTime;
        if (ctime >= 10 && !done) {
            ctime = 0;
            done = true;
            game.ResetPlayer();
            reward = count_lifes;
        }
    }

    public override void UpdateState() {
        SetStateAsString(0, "state", game.GetBoardAsString());
        SetStateAsFloat(1, "reward", reward);
        SetStateAsBool(2, "done", done);
        SetStateAsInt(3, "lifes", count_lifes);
        SetStateAsInt(4, "touch", touch);
    }
}
