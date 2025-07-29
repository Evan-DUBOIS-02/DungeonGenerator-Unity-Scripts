namespace DungeonGenerator.DungeonGeneratorEditor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(DungeonGenerator))]
    public class DungeonGeneratorEditor : Editor
    {
        override public void  OnInspectorGUI () 
        {
            base.OnInspectorGUI ();
        
            DungeonGenerator dungeonGenerator = (DungeonGenerator)target;
        
            if(GUILayout.Button("Generate")) {
                dungeonGenerator.Generate();
            }
        }
    }

}