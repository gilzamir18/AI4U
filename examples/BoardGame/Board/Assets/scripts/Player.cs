using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using unityremote;

public class Player : Agent
{

    private Game game;
    public int id = 0;

    private bool done;

    public int maxTimeInSeconds = 2;

    public float reward = 0.0f;

    public int lifes = 3;

    private int count_lifes;

    public void Start() {
        game = Game.instance;
        count_lifes = lifes;
    }

    public override void ApplyAction() {
        reward = 0.0f;
        string action = GetActionName();
        if (action == "move") {
            if (!done){
                int dir = GetActionArgAsInt();
                reward = Game.Act((PLAYER)id, (ACTION) dir );
                if (reward < 0) {
                    count_lifes--;
                }
                if (count_lifes <= 0) {
                    done = true;
                }
            }
        } else if (action == "restart") {
            count_lifes = lifes;
            count_time = 0;
            done = false;
            game.ResetPlayer((PLAYER)id);
        } else {
            //Debug.Log("Getting information " + action);
        }
    }

    private float count_time = 0;

    public override void UpdatePhysics(){
        count_time += Time.deltaTime;
        if (count_time > maxTimeInSeconds){
            done = true;
            count_time = 0;
        }
    }

    public override void UpdateState() {
        SetStateAsString(0, "state", game.GetBoardAsString());
        SetStateAsFloat(1, "reward", reward);
        SetStateAsBool(2, "done", done);
        SetStateAsInt(3, "lifes", count_lifes);
    }
}
