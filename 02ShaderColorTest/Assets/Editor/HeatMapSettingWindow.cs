using UnityEditor;

public class HeatMapSettingWindow : EditorWindow
{
    [MenuItem("Window/HeatMap Setting Window #s")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(HeatMapSettingWindow));
    }

    void OnGUI()
    {
        //自動抓取GoToDraw下所有模型
        //針對每一個模型修改參數
        /*EditorGUILayout.BeginHorizontal();
        S_Model = (SimpleModel)EditorGUILayout.ObjectField(S_Model, typeof(SimpleModel), true);
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Search!"))
        {
        }*/

    }
}