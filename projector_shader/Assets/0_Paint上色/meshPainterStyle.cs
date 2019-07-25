

//用作自定义脚本的Inspector显示

//selection參考: https://blog.csdn.net/qq_33337811/article/details/72858209

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;

[CustomEditor(typeof(meshPainter))] //自定义编辑脚本Inspector面板。宣告這個 Editor Script 是為了編輯哪個類別
[CanEditMultipleObjects]            //可以一起编辑多个物体

public class MeshPainterStyle : Editor
{
    string contolTexName = "";

    bool isPaint;

    static bool ToggleF = false;
    static float[,] map = new float[512, 512];    ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////NEW
    float brushSize = 16f;
    float brushStronger = 0.5f;

    Texture[] brushTex;
    Texture[] texLayer;

    int selBrush = 0;
    int selTex = 0;

    int brushSizeInPourcent;
    Texture2D MaskTex;

    void OnSceneGUI()
    {
        if (isPaint)          //會一直檢查
        {
            Painter();
        }

    }

    //Unity的Editor類裏的相關函數，通過對該方法的重寫，可以自定義對Inspector面板的繪制
    public override void OnInspectorGUI()

    {
        if (Cheak())
        {
            //得到Button样式
            GUIStyle boolBtnOn = new GUIStyle(GUI.skin.GetStyle("Button"));

            GUILayout.BeginHorizontal();    //水平线性布局，将控件添加至线性布局当中，最后使用EndHorizontal()方法来结束当前线性布局
            GUILayout.FlexibleSpace();      //插入一个弹性空白元素

            //编辑模式开关
            isPaint = GUILayout.Toggle(isPaint, EditorGUIUtility.IconContent("EditCollider"), boolBtnOn, GUILayout.Width(35), GUILayout.Height(25));

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            brushSize = (int)EditorGUILayout.Slider("Brush Size", brushSize, 1, 36);//笔刷大小

            IniBrush();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal("box", GUILayout.Width(318));
            selBrush = GUILayout.SelectionGrid(selBrush, brushTex, 9, "gridlist", GUILayout.Width(340), GUILayout.Height(70));
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

    }
    
    //获取笔刷  
    void IniBrush()
    {
        string MeshPaintEditorFolder = "Assets/0_Paint上色/MeshPaint/Editor/";
        ArrayList BrushList = new ArrayList();
        Texture BrushesTL;
        int BrushNum = 0;
        do
        {
            BrushesTL = (Texture)AssetDatabase.LoadAssetAtPath(MeshPaintEditorFolder + "Brushes/Brush" + BrushNum + ".png", typeof(Texture));

            if (BrushesTL)
            {
                BrushList.Add(BrushesTL);
            }
            BrushNum++;
        } while (BrushesTL);

        brushTex = BrushList.ToArray(typeof(Texture)) as Texture[];//ToArray是將ArrayList轉成array(Texture[])
    }

    //检查
    bool Cheak()
    {
        bool Cheak = false;

        //返回一个数组，内容为当前点击的场景物体；不符合条件的当前选择不会加入到数组；为选择返回长度为0的数组而不是null
        Transform Select = Selection.activeTransform;

        //讀取物件的shader裡的紋理名稱
        Texture ControlTex = Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control");

        //確認是否使用指定的紋理
        if (Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader == Shader.Find("mya/terrainTextrueBlend"))
        //if (Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader == Shader.Find("Mya/texBlend/mya_4tex_blend_diffuce") || Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.shader == Shader.Find("Mya/texBlend/mya_4tex_blend_normal"))
        {
            if (ControlTex == null)
            {
                EditorGUILayout.HelpBox("当前模型材质球中未找到Control贴图，绘制功能不可用！", MessageType.Error);
                if (GUILayout.Button("创建Control贴图"))
                {
                    creatContolTex();
                    //Select.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", creatContolTex());
                }
            }
            else
            {
                Cheak = true;
            }
        }
        else
        {
            EditorGUILayout.HelpBox("当前模型shader错误！请更换！", MessageType.Error);
        }
        return Cheak;
    }

    //创建Contol贴图
    void creatContolTex()
    {
        for (int i = 0; i < 512; i++)
            for (int j = 0; j < 512; j++)
                map[i, j] = 0f;

        //创建一个新的Contol贴图
        string ContolTexFolder = "Assets/0_Paint上色/MeshPaint/Controler/";
        Texture2D newMaskTex = new Texture2D(512, 512, TextureFormat.ARGB32, true);
        Color[] colorBase = new Color[512 * 512];
        for (int t = 0; t < colorBase.Length; t++)  //每一點都是白色
        {
            colorBase[t] = new Color(252, 255, 255, 0);
        }
        newMaskTex.SetPixels(colorBase);            //將Color給Texture2D

        //判断是否重名，若重複則底線+數字
        bool exporNameSuccess = true;
        for (int num = 1; exporNameSuccess; num++)
        {
            string Next = Selection.activeTransform.name + "_" + num;
            if (!File.Exists(ContolTexFolder + Selection.activeTransform.name + ".png"))
            {
                //取得scene中選到的物件
                contolTexName = Selection.activeTransform.name;
                exporNameSuccess = false;
            }
            else if (!File.Exists(ContolTexFolder + Next + ".png"))
            {
                contolTexName = Next;
                exporNameSuccess = false;
            }

        }

        string path = ContolTexFolder + contolTexName + ".png";
        byte[] bytes = newMaskTex.EncodeToPNG();                            //Texture2D轉成PNG圖片
        File.WriteAllBytes(path, bytes);                                    //保存

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);    //导入资源

        //Contol贴图的导入设置
        TextureImporter textureIm = AssetImporter.GetAtPath(path) as TextureImporter;
        textureIm.textureFormat = TextureImporterFormat.ARGB32;
        textureIm.isReadable = true;
        textureIm.anisoLevel = 9;               ////////////////////////////////////////////////////////////////////////////////////////
        textureIm.mipmapEnabled = false;
        textureIm.wrapMode = TextureWrapMode.Clamp;

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);//刷新


        setContolTex(path);//设置Contol贴图

    }

    //设置Contol贴图
    void setContolTex(string peth)
    {
        Texture2D ControlTex = (Texture2D)AssetDatabase.LoadAssetAtPath(peth, typeof(Texture2D));
        Selection.activeTransform.gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_Control", ControlTex);
    }

    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    void Painter()
    {
        //返回当前点击的场景游戏物体；选择多个则返回第一个选择的；未选择相应的则返回null
        Transform CurrentSelect = Selection.activeTransform;

        //获取当前模型的MeshFilter
        MeshFilter temp = CurrentSelect.GetComponent<MeshFilter>();

        //圓的大小
      //笔刷在模型上的正交大小 = float(1-36)   *   當前物體x的scale值    *   当前物體的MeshFilter的x大小  /  200
        float orthographicSize = (brushSize * CurrentSelect.localScale.x) * (temp.sharedMesh.bounds.size.x / 200);
        
        //从材质球中获取Control贴图
        MaskTex = (Texture2D)CurrentSelect.gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_Control");

      //笔刷在模型上的直徑大小 = (int)四捨五入 <  float(1-36)  *  Control贴图寬度  /  100    >
        brushSizeInPourcent = (int)Mathf.Round((brushSize * MaskTex.width) / 100); 

        //bool ToggleF = false;
        Event e = Event.current;//检测输入，Event.current当前窗口的事件

        //不想让SceneView视图接收鼠标点击选择事件，只希望在Hierarchy视图选择
        HandleUtility.AddDefaultControl(0);

        RaycastHit raycastHit = new RaycastHit();
        Ray terrain = HandleUtility.GUIPointToWorldRay(e.mousePosition);    //从鼠标位置发射一条射线
        if (Physics.Raycast(terrain, out raycastHit, Mathf.Infinity, 1 << LayerMask.NameToLayer("ground"))) //射线检测名为"ground"的层
        {
            Handles.color = new Color(1f, 1f, 0f, 1f);          //(滑鼠)控制器的颜色，黃色(R+G)
            Handles.DrawWireDisc(raycastHit.point, raycastHit.normal, orthographicSize);    //根据笔刷大小在鼠标位置显示一个圆

            //鼠标点击或按下并拖动进行绘制
            if ((e.type == EventType.MouseDrag && e.alt == false && e.control == false && e.shift == false && e.button == 0) || (e.type == EventType.MouseDown && e.shift == false && e.alt == false && e.control == false && e.button == 0 && ToggleF == false))
            {
                //选择绘制的通道
                
                Vector2 pixelUV = raycastHit.textureCoord;          //取得raycast點到的 纹理坐标(0~1)
                EditorCoroutineRunner.StartEditorCoroutine(Draw(pixelUV));
                //Draw(pixelUV);
                ToggleF = true;
            }
            
        }
        else if (e.type == EventType.Layout && e.alt == false && e.button == 0 && ToggleF == true)
        {

            SaveTexture();//绘制结束保存Control贴图
            ToggleF = false;
        }

    }

    private IEnumerator Draw(Vector2 pixelUV)
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
        Texture2D TBrush = brushTex[selBrush] as Texture2D;
        
        float[] brushAlpha = new float[brushSizeInPourcent * brushSizeInPourcent];  //笔刷透明度
                                                                                    //根据笔刷贴图计算笔刷的透明度
        for (int i = 0; i < brushSizeInPourcent; i++)
        {
            for (int j = 0; j < brushSizeInPourcent; j++)
            {
                //Texture2D.GetPixelBilinear  获取双线性像素颜色
                brushAlpha[j * brushSizeInPourcent + i] = TBrush.GetPixelBilinear(  ((float)i) / brushSizeInPourcent  ,  ((float)j) / brushSizeInPourcent  ).a;
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

                map[i, j] += 10* Stronger;

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
