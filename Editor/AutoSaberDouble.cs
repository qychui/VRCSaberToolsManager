using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class DoubleSaber : EditorWindow
{
    [MenuItem("Qychui/DoubleSaber")]
    public static void ShowWindows()
    {

        DoubleSaber.CreateInstance<DoubleSaber>().ShowUtility();

    }

    public string mTag;
    public int mLayer;
    public Object mObject;

    public void OnGUI()
    {
        this.mObject = EditorGUILayout.ObjectField("对象选择器", this.mObject, typeof(GameObject), true);

        if (GUILayout.Button("Close"))
        {
            this.Close();
        }
    }
}
