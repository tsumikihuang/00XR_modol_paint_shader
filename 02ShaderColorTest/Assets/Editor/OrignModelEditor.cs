using UnityEditor;
using UnityEngine;

//顯示模型的客製化調整參數
[CustomEditor(typeof(OrignModel))]
public class SimpleModelEditor : Editor
{
    private OrignModel O_Model;        //目前這個物件
    private bool _isOrignTexture = false;

    public override void OnInspectorGUI()
    {
        //DrawDefaultInspector();
        O_Model = (OrignModel)target;

        //空行
        EditorGUILayout.Space();

        #region 選擇貼圖
        // Toggle(標題, 預設值)，勾選框元件
        _isOrignTexture = EditorGUILayout.Toggle("是否要顯示原本的貼圖", _isOrignTexture);      //預設是HeatMap材質
        if (_isOrignTexture)
        {
            O_Model.OrignTexture();
        }
        else
        {
            O_Model.HeatMapTexture();
        }
        #endregion

        EditorGUILayout.Space();


        #region 如果還沒創建M2M資料...
            EditorGUILayout.LabelField("放入參考的紀錄點(簡化後)模型，需有SimpleModel.cs");
            O_Model.S_Model = (SimpleModel)EditorGUILayout.ObjectField(O_Model.S_Model, typeof(SimpleModel), true);

            EditorGUILayout.Space();

            // 從 BeginDisabledGroup(Boolean) 到 EndDisabledGroup() 中間的範圍是否可以被選取
            // 取決於 BeginDisabledGroup 傳入的布林參數
            EditorGUI.BeginDisabledGroup(O_Model.Read_M2M_PassToShader() != null);  //如果還沒創建就可以改S模型及參考熱點半徑

            // FloatField(標題, 預設值)，浮點數輸入元件
            // 原本的目標物件(Camera)裡的變數都要設定為 Inspector 欄位中修改的數值
            O_Model.O2S_Radius = EditorGUILayout.FloatField("參考熱點半徑範圍(可小數)", O_Model.O2S_Radius);

            EditorGUILayout.HelpBox("必須創建模型對應資料才能開始繪製！請點擊下方按鈕", MessageType.Warning);

            if (GUILayout.Button("創建 OrignModel to SimpleModel 對應資料"))
            {
                M2M_KDT_Search.instance.Generate_M2M(O_Model, O_Model.S_Model, O_Model.O2S_Radius);
                O_Model.PassM2M_OneTime();
            }
        EditorGUI.EndDisabledGroup();
        #endregion

        # region 創建好M2M資料的話...
            EditorGUI.BeginDisabledGroup(O_Model.Read_M2M_PassToShader()==null);
            if (GUILayout.Button("刪除目前 OrignModel to SimpleModel 對應資料"))
                O_Model.Delete_M2M();

            // Slider(標題, 預設值, 最小值, 最大值)，滑桿元件
            O_Model.ShaderRadius = EditorGUILayout.Slider("Shader上色半徑", O_Model.ShaderRadius, 0, O_Model.O2S_Radius); //不可超過M2M時計算的半徑範圍
            EditorGUI.EndDisabledGroup();
        #endregion

        if (GUILayout.Button("清空熱點(上色)資料"))
            O_Model.S_Model.Init_Count();

        /***********************************************************************************************************************************/
        /***********************************************************************************************************************************/
        // 每一次都重畫場景中的物件(為了處理 Gizmos)
        //SceneView.RepaintAll();

    }
}
