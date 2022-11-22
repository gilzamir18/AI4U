using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ai4u
{
    public class GridBuilder : MonoBehaviour
    {
        public static int FLOOR = 1;
        public static int WALL = 50;
        public static int ENEMY = 100;
        public static int NPC = 150;
        public static int DIRTY = 200;
        public static int OTHER = 255;

        public MeshFilter floor;
        public int H = 10;
        public int W = 10;
        public float height = 1.0f;
        public float fillArea = 1.0f;
        public Material enemyMaterial;
        public Material dirtyMaterial;
        public Material otherMaterial;
        public Material wallMaterial;

        public GameObject npc;

        private int[,] objType;
        private GameObject[,] objGrid;

        private int column;
        private int line;
        private float wsize, hsize;
        private Vector3 min, max;

        private int numberOfDirtyRooms = 0;

        public void Awake()
        {
            ResetGrid();
        }

        public float GetObjectType(int i, int j)
        {
            return this.objType[i, j];
        }

        public void ResetGrid(int depth = 0)
        {
            if (depth == 0)
            {
                _ResetGrid();
            }
            else
            {
                _ResetGrid2(depth);
            }
        }

        private void _ResetGrid()
        {
            numberOfDirtyRooms = 0;
            if (objGrid != null && objType != null)
            {
                for (int i = 0; i < W; i++)
                {
                    for (int j = 0; j < H; j++)
                    {
                        if (objType[i,j] != NPC && objGrid[i, j] != null)
                        {
                            DestroyImmediate(objGrid[i,j].gameObject);
                        }
                    }
                }
            }

            Vector3 scale = floor.gameObject.transform.localScale;
            float sx = scale.x;
            float sy = scale.y;
            float sz = scale.z;
            Vector3[] vertices = floor.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].x * sx, vertices[i].y * sy, vertices[i].z * sz);
            }

            min = Vector3.one * float.PositiveInfinity;
            max = Vector3.one * float.NegativeInfinity;
            for(int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;
            }

            wsize = (max.x - min.x)/W;
            hsize = (max.z - min.z)/H;

            objType = new int[W, H];
            objGrid = new GameObject[W, H];
            float hW = wsize/2;
            float hH = hsize/2;
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    int p = Random.Range(0, 100);
                    if (p < 30)
                    {
                        objType[i, j] = WALL;
                    }
                    else if (p >= 30 && p < 35)
                    {
                        objType[i, j] = DIRTY;
                        numberOfDirtyRooms++;
                    }
                    else
                    {
                        objType[i, j] = FLOOR;
                    }

                    Vector3 pos = new Vector3(min.x + i * wsize + hW, min.y + height, min.z + j * hsize + hH);
                    GameObject gobj = null;
                    if (objType[i, j] == WALL)
                    {
                        gobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        gobj.GetComponent<Renderer>().material = wallMaterial;
                    }
                    else if (objType[i, j] == DIRTY)
                    {
                        gobj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                        gobj.GetComponent<Renderer>().material = dirtyMaterial;
                    
                    }
                    if (gobj != null)
                    {
                        objGrid[i, j] = gobj;
                        gobj.transform.localScale = new Vector3(wsize * fillArea, 1, hsize * fillArea);                 
                        gobj.transform.position = pos;
                    }
                }
            }

            int c = Random.Range(0, W);
            int l = Random.Range(0, H);
            while (objType[c, l] != FLOOR)
            {
                c = Random.Range(0, W);
                l = Random.Range(0, H);
            }
            objType[c,l] = NPC;
            objGrid[c,l] = npc;
            line = l;
            column = c;
            Vector3 pos2 = new Vector3(min.x + c * wsize + hW, min.y + height, min.z + l * hsize + hH);
            npc.transform.position = pos2;
            npc.transform.localScale = new Vector3(wsize * fillArea, 1, hsize * fillArea);
        }



        private void _ResetGrid2(int d)
        {
            numberOfDirtyRooms = 0;
            if (objGrid != null && objType != null)
            {
                for (int i = 0; i < W; i++)
                {
                    for (int j = 0; j < H; j++)
                    {
                        if (objType[i,j] != NPC && objGrid[i, j] != null)
                        {
                            DestroyImmediate(objGrid[i,j].gameObject);
                        }
                    }
                }
            }

            Vector3 scale = floor.gameObject.transform.localScale;
            float sx = scale.x;
            float sy = scale.y;
            float sz = scale.z;
            Vector3[] vertices = floor.mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                vertices[i] = new Vector3(vertices[i].x * sx, vertices[i].y * sy, vertices[i].z * sz);
            }

            min = Vector3.one * float.PositiveInfinity;
            max = Vector3.one * float.NegativeInfinity;
            for(int i = 0; i < vertices.Length; i++)
            {
                var v = vertices[i];
                if (v.x < min.x) min.x = v.x;
                if (v.y < min.y) min.y = v.y;
                if (v.z < min.z) min.z = v.z;
                if (v.x > max.x) max.x = v.x;
                if (v.y > max.y) max.y = v.y;
                if (v.z > max.z) max.z = v.z;
            }

            wsize = (max.x - min.x)/W;
            hsize = (max.z - min.z)/H;

            objType = new int[W, H];
            objGrid = new GameObject[W, H];
            float hW = wsize/2;
            float hH = hsize/2;
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    int p = Random.Range(0, 100);
                    if (p < 30)
                    {
                        objType[i, j] = WALL;
                    }
                    else
                    {
                        objType[i, j] = FLOOR;
                    }

                    Vector3 pos = new Vector3(min.x + i * wsize + hW, min.y + height, min.z + j * hsize + hH);
                    GameObject gobj = null;
                    if (objType[i, j] == WALL)
                    {
                        gobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        gobj.GetComponent<Renderer>().material = wallMaterial;
                    }

                    if (gobj != null)
                    {
                        objGrid[i, j] = gobj;
                        gobj.transform.localScale = new Vector3(wsize * fillArea, 1, hsize * fillArea);                 
                        gobj.transform.position = pos;
                    }
                }
            }

            int c = Random.Range(0, W);
            int l = Random.Range(0, H);
            while (objType[c, l] != FLOOR && !validPos(c, l) )
            {
                c = Random.Range(0, W);
                l = Random.Range(0, H);
            }
            objType[c,l] = NPC;
            objGrid[c,l] = npc;
            line = l;
            column = c;

            int cd = d;
            int dx = -1;
            int dy = -1;
            int opt = 0;  
            while (true)
            {
                dx = c;
                dy = l;
                if (opt == 0) 
                {
                    dx += cd;
                    if (dx < W && objType[dx, dy] == FLOOR)
                    {
                        break;
                    }
                    opt++;
                }
                else if (opt == 1)
                {
                    dx -= cd;
                    if (dx >= 0 && objType[dx, dy] == FLOOR)
                    {
                        break;
                    }
                    opt++;
                }
                else if (opt == 2)
                {
                    dy += cd;
                    if (dy < H && objType[dx, dy] == FLOOR)
                    {
                        break;
                    }
                    opt++;
                }
                else if (opt == 3)
                {
                    dy -= cd;
                    if (dy >= 0 && objType[dx, dy] == FLOOR)
                    {
                        break;
                    }
                    opt++;
                }
                if (opt >= 4)
                {
                    opt = 0;
                    cd++;
                }
            }
            Vector3 posd = new Vector3(min.x + dx * wsize + hW, min.y + height, min.z + dy * hsize + hH);
            objType[dx, dy] = DIRTY;
            GameObject gobj2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            gobj2.GetComponent<Renderer>().material = dirtyMaterial;
            objGrid[dx, dy] = gobj2;
            gobj2.transform.localScale = new Vector3(wsize * fillArea, 1, hsize * fillArea);                 
            gobj2.transform.position = posd;

            Vector3 pos2 = new Vector3(min.x + c * wsize + hW, min.y + height, min.z + l * hsize + hH);
            npc.transform.position = pos2;
            npc.transform.localScale = new Vector3(wsize * fillArea, 1, hsize * fillArea);
        }

        public bool validPos(int c, int l)
        {
            int v1x = c -1;
            int v1y = l;

            int v2x = c + 1;
            int v2y = l;

            int v3x = c;
            int v3y = l - 1;

            int v4x = c;
            int v4y = l - 1;

            return ( (v1x >= 0 && objType[v1x, v1y] == FLOOR) ||
                     (v2x < W && objType[v2x, v2y] == FLOOR) ||
                     (v3y >= 0 && objType[v3x, v3y] == FLOOR) ||
                     (v4y < H && objType[v4x, v4y] == FLOOR) );

        }


        public int Score()
        {
            int n = 0;
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (objType[i,j] == DIRTY)
                    {
                        n++;
                    }
                }
            }
            return numberOfDirtyRooms-n;
        }

        public void Move(int action)
        {
            int nl = line;
            int nc = column;
            float hW = wsize/2;
            float hH = hsize/2;
            if (action == 0) //UP
            {
                nl = nl + 1;
                if (nl >= 0 && nl < H && objType[nc, nl] ==  FLOOR)
                {
                    objType[column, line] = FLOOR;
                    objType[nc, nl] = NPC;
                    objGrid[nc, nl] = npc;
                    objGrid[column, line] = null;
                    column = nc;
                    line = nl;
                    npc.transform.position = new Vector3(min.x + nc * wsize + hW, min.y + height, min.z + nl * hsize + hH);
                }
            } else if (action == 1) //DOWN
            {
                nl = nl - 1;
                if (nl >= 0  && nl < H && objType[nc, nl] ==  FLOOR)
                {
                    objType[column, line] = FLOOR;
                    objType[nc, nl] = NPC;
                    objGrid[nc, nl] = npc;
                    objGrid[column, line] = null;
                    column = nc;
                    line = nl;
                    npc.transform.position = new Vector3(min.x + nc * wsize + hW, min.y + height, min.z + nl * hsize + hH);
                }
            } else if (action == 2)
            {
                int[] nbc = new int[4];
                int[] nbl = new int[4];
                nbc[0] = column-1;
                nbl[0] = line;
                nbc[1] = column+1;
                nbl[1] = line;
                nbc[2] = column;
                nbl[2] = line-1;
                nbc[3] = column;
                nbl[3] = line+1;
                for (int i = 0; i < 4; i++)
                {
                    int c = nbc[i];
                    int l = nbl[i];
                    if (c >= 0 && c < W && l >= 0 && l < H)
                    {
                        if (objType[c, l] == DIRTY)
                        {
                            objType[c, l] = FLOOR;
                            Destroy(objGrid[c, l].gameObject);
                            objGrid[c, l] = null;
                        } 
                    }
                }
            } else if (action == 3) //LEFT
            {
                nc = nc - 1;
                if (nc >= 0 && nc < H && objType[nc, nl] ==  FLOOR)
                {
                    objType[column, line] = FLOOR;
                    objType[nc, nl] = NPC;
                    objGrid[nc, nl] = npc;
                    objGrid[column, line] = null;
                    column = nc;
                    line = nl;
                    npc.transform.position = new Vector3(min.x + nc * wsize + hW, min.y + height, min.z + nl * hsize + hH);
                }
            } else if (action == 4) //RIGHT
            {
                nc = nc + 1;
                if (nc >= 0 && nc < H && objType[nc, nl] ==  FLOOR)
                {
                    objType[column, line] = FLOOR;
                    objType[nc, nl] = NPC;
                    objGrid[nc, nl] = npc;
                    objGrid[column, line] = null;
                    column = nc;
                    line = nl;
                    npc.transform.position = new Vector3(min.x + nc * wsize + hW, min.y + height, min.z + nl * hsize + hH);
                }
            }
            npc.GetComponent<BasicAgent>().AddReward(Score());
        }
    }
}