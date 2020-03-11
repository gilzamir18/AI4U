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
}

public class Game : MonoBehaviour
{

    public const int AGENT1 = 1, FRUIT = 3, PAIN = 4, NOTHING = 0;
    
    private float GetReward(int obj){
        switch(obj){
            case FRUIT:
                return 1;
            case PAIN:
                return -1;
            default:
                return 0;
        }
    }


    public const int W = 10, H = 10;

    private int p1x = 0, p1z = 0, p2x = 0, p2z = 0;

    private float xref = -4.5f, yref = 4.5f;

    private int[,] board;

    public GameObject player1, fruit1, fruit2, pain1, pain2;
    
    
    // Start is called before the first frame update
    void Awake()
    {
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
    

    public float Act(ACTION act, out int touch){
        GameObject player;
        int col, lin;
        touch = 0;
        player = this.player1;
        lin = this.p1x; col = this.p1z;
        

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
            touch = -1;
            return 0;
        } else {
            float reward = 0;
            if (this.board[lin, col] != (int)PLAYER.player1){
                if ( (lin == 5 && col == 5) || (lin == 5 && col == 3) ){
                    touch = 2;
                } else if ( (lin == 3 && col == 5) || (lin == 5 && col == 6) ) {
                    touch = -2;
                }
                reward = GetReward(this.board[lin, col]);            
                this.board[this.p1x, this.p1z] = NOTHING;
                
                if ( (this.p1x == 5 && this.p1z == 5) || (this.p1x == 5 && this.p1z == 3) ) {
                    this.board[this.p1x, this.p1z] = FRUIT;
                } else if ( (this.p1x == 3 && this.p1z == 5) || (this.p1x == 5 && this.p1z == 6) ) {
                    this.board[this.p1x, this.p1z] = PAIN;   
                }

                this.p1x=lin; this.p1z=col;
            

                float dv = lin;
                float dh = col;
            
                this.board[lin, col] = (int)PLAYER.player1;
                //Debug.Log(this.GetBoardAsString());
                player.transform.localPosition = new Vector3(this.xref + dh, this.yref - dv, 10);
                return reward;
            } else {
                touch = 3;
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
        
        player1.transform.localPosition = new Vector3(-4.5f, 4.5f, 10);

        p1x = 0;
        p1z = 0;
    }

    public void ResetPlayer() {

        for (int i = 0; i < H; i++){
            for (int j = 0; j < W; j++) {
                if (board[i, j] == (int)PLAYER.player1){
                    board[i, j] = NOTHING;
                }
            }
        }
        if ( (p1x == 5 && p1z == 5) || (p1x == 5 && p1z == 3) ) {
            board[p1x, p1z] = FRUIT;
        } else if ( (p1x == 3 && p1z == 5) || (p1x == 5 && p1z == 6) ) {
            board[p1x, p1z] = PAIN;   
        }
        board[0, 0] = (int)PLAYER.player1;
        player1.transform.localPosition = new Vector3(-4.5f, 4.5f, 10);
        p1x = 0;
        p1z = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
