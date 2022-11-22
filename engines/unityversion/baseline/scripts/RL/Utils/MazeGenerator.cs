using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ai4u
{

    public class Node
    {
        public Node parent;
        public int action;
        public int line;
        public int column;
        public int depth;
        public Node(Node parent=null, int action=-1, int depth = 0, int column = 0, int line = 0)
        {
            this.parent = parent;
            this.action = action;
            this.depth = depth;
            this.line = line;
            this.column = column;
        }
    }

    [System.Serializable]
    public class Cell
    {
        public int line;
        public int column;

        public Cell()
        {
            line = 0;
            column = 0;
        }
    }

    public class MazeGenerator : MonoBehaviour
    {
        public static int FLOOR = 0;
        public static int WALL = 1;
        private const int ACT_W=0, ACT_A=1, ACT_S=2, ACT_D=3;

        public MeshFilter floor;
        public int H = 10;
        public int W = 10;
        public bool randomStart = false;
        public List<Cell> startPoints;
        public int goalLine = 5;
        public int goalColumn = 5;
        public float height = 1.0f;
        public float fillArea = 1.0f;
        public float wallHeight = 1;
        public int paths;
        public Material wallMaterial;
        public float padding = 1.0f;
        public int maxDepth = 10000;

        private int[,] objType;
        private GameObject[,] objGrid;

        private int column;
        private int line;
        private float wsize, hsize;
        private Vector3 min, max;
        public List<Vector3> floorPositions;


        public int GetObjectType(int i, int j)
        {
            return this.objType[i, j];
        }

        public void ResetGrid()
        {
            floorPositions = new List<Vector3>();
            objType = new int[W, H];
            
            if (objGrid == null)
            {
                objGrid = new GameObject[W, H];
            }

            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (objGrid[i, j] != null)
                    {
                        DestroyImmediate(objGrid[i,j].gameObject);
                        objGrid[i, j] =  null;
                    }
                    objType[i, j] = WALL;
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


            var min = Vector3.one * float.PositiveInfinity;
            var max = Vector3.one * float.NegativeInfinity;
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

            min.x = min.x + padding;
            min.z = min.z + padding;
            max.x = max.x - padding;
            max.z = max.z - padding;

            wsize = (max.x - min.x)/W;
            hsize = (max.z - min.z)/H;
            
            float hW = wsize/2;
            float hH = hsize/2;
            int n = paths;
            if  (!randomStart)
            {
                n = startPoints.Count;
            }

            for (int i = 0; i < n; i++)
            {
                Cell cell = null;
                if (startPoints != null)
                {
                    cell = startPoints[i];
                }
                var node = Search(cell);
                while (node.parent != null)
                {
                    var p = node.parent;
                    node.parent = null;
                    node = p;
                }
            }
            
            for (int i = 0; i < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    if (objType[i, j] == WALL)
                    {
                        Vector3 pos = new Vector3(min.x + i * wsize + hW, min.y + height, min.z + j * hsize + hH);
                        GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        gobj.GetComponent<Renderer>().material = wallMaterial;
                        objGrid[i, j] = gobj;
                        gobj.name = $"Node({i},{j})";
                        gobj.transform.localScale = new Vector3(wsize * fillArea, wallHeight, hsize * fillArea);                 
                        gobj.transform.position = pos;
                    }
                    else
                    {
                        Vector3 pos = new Vector3(min.x + i * wsize + hW, min.y + height, min.z + j * hsize + hH);
                        GameObject gobj = new GameObject($"Node({i},{j})");
                        Instantiate(gobj, Vector3.zero, Quaternion.identity);
                        gobj.transform.position = pos;
                        objGrid[i, j] = gobj;
                        floorPositions.Add(pos);
                    }
                }
            }
        }

        public bool IsWall(int i, int j)
        {
            return objType[i, j] == WALL;
        }

        public Vector3 GetPos(int i, int j)
        {
            return objGrid[i, j].transform.position;
        }

        private string Str(int x, int y)
        {
            return $"{x}{y}";
        }

        private string Str(int[] p)
        {
            return $"{p[0]}{p[1]}";
        }

        private Node Search(Cell startPoint = null)
        {

            if (startPoint == null)
            {
                startPoint = new Cell();
            }

            int firstLine = startPoint.line;
            int firstColumn = startPoint.column;

            if (goalLine < 0)
            {
                goalLine = 0;
            }
            else if (goalLine >= H)
            {
                goalLine = H - 1;
            }

            if (goalColumn < 0)
            {
                goalColumn = 0;
            }
            else if (goalColumn >= W)
            {
                goalColumn = W - 1;
            }

            LinkedList<Node> border = new LinkedList<Node>();
            
            if (randomStart)
            {
                firstLine = Random.Range(0, H);
                firstColumn = Random.Range(0, W);
            }
            Node root = new Node(null, -1, 0, firstColumn, firstLine);
            border.AddLast(root);
            Dictionary<string, bool> visited = new Dictionary<string, bool>();
            for (int i = 0; i  < W; i++)
            {
                for (int j = 0; j < H; j++)
                {
                    visited[Str(i, j)] = false;
                }
            }
            visited[Str(root.column, root.line)] = true;
            while (border.Count > 0)
            {
                var node = border.Last.Value;
                border.RemoveLast();
                objType[node.column, node.line] = FLOOR;
                if (node.line == goalLine && node.column == goalColumn || (maxDepth > 0 && node.depth > maxDepth))
                {
                    Debug.Log($"Path size: {node.depth}");
                    return node;
                }
                else
                {
                    int[] actions = new int[4];
                    Dictionary<int, int> repeated = new Dictionary<int, int>();
                    for (int i = 0; i < 4; i++)
                    {
                        int s = Random.Range(0, 4);
                        while (repeated.ContainsKey(s))
                        {
                            s = Random.Range(0, 4);
                        }
                        repeated[s] = s;
                        actions[i] = s;
                    }
                    foreach(var action in actions)
                    {
                        var pos = GetNextPos(action, node.column, node.line);
                        if (pos != null)
                        {
                            if (visited[Str(pos)]) continue;
                            border.AddLast(new Node(node, action, node.depth+1, pos[0], pos[1]));
                        }
                    }
                }
            }
            return null;
        }

        public int[] GetNextPos(int action, int c, int l)
        {
            int nl = l;
            int nc = c;
            switch(action)
            {
                case ACT_W:
                    nl -= 1;
                    break;
                case ACT_S:
                    nl += 1;
                    break;
                case ACT_A:
                    nc -= 1;
                    break;
                case ACT_D:
                    nc += 1;
                    break;
            }

            if (validPos(nc, nl))
            {
                return new int[]{nc, nl};
            }

            return null;
        }


        public bool validPos(int c, int l)
        {
            return (c >= 0 && c < W && l >= 0 && l < H);
        }
    }
}
