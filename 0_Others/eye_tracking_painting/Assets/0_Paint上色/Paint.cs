using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

namespace ViveSR.anipal.Eye
{
    public class Paint : MonoBehaviour
    {
        static float[,] map = new float[512, 512];
        float brushSize = 30f;

        int brushSizeInPourcent;
        Texture2D MaskTex;

        /// /////////////////////////////////////////////////////////////
        private FocusInfo FocusInfo;
        
        private void Start()
        {
            if (!SRanipal_Eye_Framework.Instance.EnableEye)
            {
                enabled = false;
                return;
            }
        }

        /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Update()
        {
            if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
                SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT)
                return;

            Ray ray_C;
            if (SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out ray_C) ) {

                RaycastHit raycastHit = new RaycastHit();

                if (Physics.Raycast(ray_C, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("ground"))) //射线检测名为"ground"的层
                {
                    //返回当前点击的场景游戏物体；选择多个则返回第一个选择的；未选择相应的则返回null
                    Transform CurrentSelect = raycastHit.transform;

                    //获取当前模型的MeshFilter
                    MeshFilter temp = CurrentSelect.GetComponent<MeshFilter>();

                    //圓的大小
                    //笔刷在模型上的正交大小 = float(1-36)   *   當前物體x的scale值    *   当前物體的MeshFilter的x大小  /  200
                    float orthographicSize = (brushSize * CurrentSelect.localScale.x) * (temp.sharedMesh.bounds.size.x / 200);

                    //从材质球中获取Control贴图
                    MaskTex = (Texture2D)CurrentSelect.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control");

                    //笔刷在模型上的直徑大小 = (int)四捨五入 <  float(1-36)  *  Control贴图寬度  /  100    >
                    brushSizeInPourcent = (int)Mathf.Round((brushSize * MaskTex.width) / 100);

                    /*https://gameinstitute.qq.com/community/detail/126030
                    Gizmos.color = new Color(1f, 1f, 0f, 1f);          //(滑鼠)控制器的颜色，黃色(R+G)
                    //Gizmos.DrawWireDisc(raycastHit.point, raycastHit.normal, orthographicSize);    //根据笔刷大小在鼠标位置显示一个圆
                    ///
                    float m_Theta = 0.1f;
                    // 设置矩阵
                    Matrix4x4 defaultMatrix = Gizmos.matrix;
                    //Gizmos.matrix = m_Transform.localToWorldMatrix;
                    Vector3 beginPoint = raycastHit.point;
                    Vector3 firstPoint = raycastHit.point;
                    for (float theta = 0; theta < 2 * Mathf.PI; theta += m_Theta)
                    {
                        float x = orthographicSize * Mathf.Cos(theta);
                        float z = orthographicSize * Mathf.Sin(theta);
                        Vector3 endPoint = new Vector3(x, 0, z);
                        if (theta == 0)
                        {
                            firstPoint = endPoint;
                        }
                        else
                        {
                            Gizmos.DrawLine(beginPoint, endPoint);
                        }
                        beginPoint = endPoint;
                    }
                    // 绘制最后一条线段
                    Gizmos.DrawLine(firstPoint, beginPoint);
                    ///
                    */
                    //选择绘制的通道

                    Vector2 pixelUV = raycastHit.textureCoord;          //取得raycast點到的 纹理坐标(0~1)
                    StartCoroutine(Draw(pixelUV));
                }
            }
            
        }

        IEnumerator Draw(Vector2 pixelUV)
        //private void Draw(Vector2 pixelUV)
        {
            yield return null;

            Color targetColor = new Color(1f, 1f, 1f, 0f);

            //计算笔刷所覆盖的区域

            //計算在texture上的座標(纹理坐标*texture寬高)
            int PuX = Mathf.FloorToInt(pixelUV.x * MaskTex.width);                          //Mathf.FloorToInt無條件捨去
            int PuY = Mathf.FloorToInt(pixelUV.y * MaskTex.height);

            //計算筆刷在貼圖上的區域
            int x = Mathf.Clamp(PuX - brushSizeInPourcent / 2, 0, MaskTex.width - 1);       //clamp(value,min,max)
            int y = Mathf.Clamp(PuY - brushSizeInPourcent / 2, 0, MaskTex.height - 1);

            int width = Mathf.Clamp((PuX + brushSizeInPourcent / 2), 0, MaskTex.width) - x;
            int height = Mathf.Clamp((PuY + brushSizeInPourcent / 2), 0, MaskTex.height) - y;

            //获取Control贴图被笔刷所覆盖的区域的颜色，左到右，上到下
            Color[] terrainBay = MaskTex.GetPixels(x, y, width, height, 0);     //Texture2D.GetPixels 获取一块像素颜色

            Texture2D TBrush = ((Texture)AssetDatabase.LoadAssetAtPath("Assets/0_Paint上色/MeshPaint/Editor/Brushes/Brush3.png", typeof(Texture))) as Texture2D;


            float[] brushAlpha = new float[brushSizeInPourcent * brushSizeInPourcent];  //笔刷透明度
                                                                                        //根据笔刷贴图计算笔刷的透明度
            for (int i = 0; i < brushSizeInPourcent; i++)
            {
                for (int j = 0; j < brushSizeInPourcent; j++)
                {
                    //Texture2D.GetPixelBilinear  获取双线性像素颜色
                    brushAlpha[j * brushSizeInPourcent + i] = TBrush.GetPixelBilinear(((float)i) / brushSizeInPourcent, ((float)j) / brushSizeInPourcent).a;
                }
            }
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////NEW

            for (int j = y; j < (y + height); j++)
            {
                for (int i = x; i < (x + width); i++)
                {

                    int index = ((j - y) * width) + i - x;

                    float Stronger = brushAlpha[
                                                Mathf.Clamp(j - (PuY - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1) * brushSizeInPourcent
                                                + Mathf.Clamp(i - (PuX - brushSizeInPourcent / 2), 0, brushSizeInPourcent - 1)
                                                ];

                    map[i, j] += 10 * Stronger;

                    if (map[i, j] >= 0 && map[i, j] <= 255)
                        targetColor = new Color(1, 1, 1 - map[i, j] % 255 / 255);    //white >> yello
                    else if (map[i, j] > 255 && map[i, j] <= 255 * 2)
                        targetColor = new Color(1 - map[i, j] % 255 / 255, 1, 0);    //yello >> green
                    else if (map[i, j] > 255 * 2 && map[i, j] <= 255 * 3)
                        targetColor = new Color(0, 1, map[i, j] % 255 / 255);
                    else if (map[i, j] > 255 * 3 && map[i, j] <= 255 * 4)
                        targetColor = new Color(0, 1 - map[i, j] % 255 / 255, 1);
                    else if (map[i, j] > 255 * 4 && map[i, j] <= 255 * 5)
                        targetColor = new Color(map[i, j] % 255 / 255, 0, 1);
                    else if (map[i, j] > 255 * 5 && map[i, j] <= 255 * 6)
                        targetColor = new Color(1, 0, 1 - map[i, j] % 255 / 255);
                    else
                        targetColor = new Color(1, 0, 0);

                    terrainBay[index] = Color.Lerp(terrainBay[index], targetColor, Stronger);
                }
            }

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Undo.RegisterCompleteObjectUndo(MaskTex, "meshPaint");//保存历史记录以便撤销

            MaskTex.SetPixels(x, y, width, height, terrainBay, 0);//把绘制后的Control贴图保存起来
            MaskTex.Apply();
        }

        public void SaveTexture()
        {
            var path = AssetDatabase.GetAssetPath(MaskTex);
            var bytes = MaskTex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);

            //AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新
        }
    }
}