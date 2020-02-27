using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public enum ACTION {
    left=0,
    right=1,
    up=2,
    down=3,
}

public enum PLAYER {
    player1=1,
    player2=2
}

public class Game : MonoBehaviour
{

    public const int AGENT1 = 1, AGENT2 = 2, FRUIT = 3, PAIN = 4, NOTHING = 0;
    
    private static float[] rewards = {0, 0, 0, 1, -1};


    public const int W = 10, H = 10;

    private int p1x = 0, p1z = 0, p2x = 0, p2z = 0;

    private float xref = -4.5f, yref = 4.5f;

    private int[,] board;

    public GameObject player1, player2, fruit1, fruit2, pain1, pain2;
    
    public static Game instance;

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
        ResetGame();
    }

    public string GetBoardAsString() {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < H; i++) {
            for (int j = 0; j < W; j++) {
                sb.Append(board[i, j]);
                if ( j < W-1) {
                    sb.Append(",");
                }
            }
            if (i < H-1) {
                sb.Append(";");
            }
        }
        return sb.ToString();
    }

    public void CalcXFromInt(int col, int lin, out float x, out float y){
        x = xref + col;
        y = yref + lin;  
    }
    

    public static float Act(PLAYER p, ACTION act){
        GameObject player;
        int col, lin;

        if (p == PLAYER.player1){
            player = instance.player1;
            lin = instance.p1x; col = instance.p1z;
        } else {
            player = instance.player2;
            lin = instance.p2x; col = instance.p2z;
        } 

        switch(act) {
            case ACTION.left:
            col--;
            break;
            case ACTION.right:
            col++;
            break;
            case ACTION.up:
            lin--;
            break;
            case ACTION.down:
            lin++;
            break;
        }

        if (lin < 0 || col < 0 || lin >= H || col >= W) {
            return 0;
        } else {
            float reward = 0;
            if (instance.board[lin, col] != (int)PLAYER.player1  && instance.board[lin, col] != (int)PLAYER.player2){
                reward = rewards[instance.board[lin, col]];            
                if (p == PLAYER.player1){
                    instance.board[instance.p1x, instance.p1z] = NOTHING;
                    
                    if ( (instance.p1x == 5 && instance.p1z == 5) || (instance.p1x == 5 && instance.p1z == 3) ) {
                        instance.board[instance.p1x, instance.p1z] = FRUIT;   
                    } else if ( (instance.p1x == 3 && instance.p1z == 5) || (instance.p1x == 5 && instance.p1z == 6) ) {
                        instance.board[instance.p1x, instance.p1z] = PAIN;   
                    }

                    instance.p1x=lin; instance.p1z=col;
                } else if (p == PLAYER.player2) {
                    instance.board[instance.p2x, instance.p2z] = NOTHING;
                    if ( (instance.p2x == 5 && instance.p2z == 5) || (instance.p2x == 5 && instance.p2z == 3) ) {
                        instance.board[instance.p1x, instance.p1z] = FRUIT;   
                    } else if ( (instance.p2x == 3 && instance.p2z == 5) || (instance.p2x == 5 && instance.p2z == 6) ) {
                        instance.board[instance.p2x, instance.p2z] = PAIN;   
                    }
                    
                    instance.p2x=lin; instance.p2z=col;
                }

                float dv = lin;
                float dh = col;
            
                instance.board[lin, col] = (int)p;
                //Debug.Log(instance.GetBoardAsString());
                player.transform.position = new Vector3(instance.xref + dh, instance.yref - dv, 10);
                return reward;
            } else {
                return 0;
            }
        }
    }

    public void ResetGame() {
        board = new int[W, H];
        for (int i = 0; i < H; i++){
            for (int j = 0; j < W; j++) {
                board[i, j] = NOTHING;
            }
        }

        board[5, 5] = FRUIT;
        board[5, 3] = FRUIT;
        board[3, 5] = PAIN;
        board[5, 6] = PAIN; 

        board[0, 0] = (int)PLAYER.player1;
        board[9, 9] = (int)PLAYER.player2;
        
        player1.transform.position = new Vector3(-4.5f, 4.5f, 10);
        player2.transform.position = new Vector3(4.5f, -4.5f, 10);

        p1x = 0;
        p1z = 0;
        p2x = 9;
        p2z = 9;
    }

    public void ResetPlayer(PLAYER p) {
        if (p == PLAYER.player1) {
            player1.transform.position = new Vector3(-4.5f, 4.5f, 10);
        } else  {
            player2.transform.position = new Vector3(4.5f, -4.5f, 10);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
