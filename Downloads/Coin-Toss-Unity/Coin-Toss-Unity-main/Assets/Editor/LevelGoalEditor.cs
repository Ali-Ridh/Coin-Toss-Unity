using UnityEngine;
using UnityEditor; // This is needed for custom editors

[CustomEditor(typeof(LevelGoal))]
public class LevelGoalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Get the target script that we are creating an editor for.
        LevelGoal levelGoal = (LevelGoal)target;

        // --- Draw the Win Conditions List ---
        EditorGUILayout.LabelField("Win Conditions", EditorStyles.boldLabel);
        DrawConditionList(levelGoal.winConditions);

        EditorGUILayout.Space(10); // Add some space

        // --- Draw the Lose Conditions List ---
        EditorGUILayout.LabelField("Lose Conditions", EditorStyles.boldLabel);
        DrawConditionList(levelGoal.loseConditions);

        // This applies any changes made in the Inspector.
        if (GUI.changed)
        {
            EditorUtility.SetDirty(levelGoal);
        }
    }

    private void DrawConditionList(System.Collections.Generic.List<Condition> list)
    {
        // --- List Size ---
        int newCount = Mathf.Max(1, EditorGUILayout.IntField("Number of Conditions", list.Count));
        while (newCount < list.Count)
            list.RemoveAt(list.Count - 1);
        while (newCount > list.Count)
            list.Add(new Condition());

        EditorGUILayout.Space(5);

        // --- Draw Each Condition ---
        for (int i = 0; i < list.Count; i++)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box); // Draw a box around each condition

            // Draw the dropdown for the condition type
            list[i].type = (ConditionType)EditorGUILayout.EnumPopup("Condition Type", list[i].type);

            // Use a switch statement to draw the correct fields for the selected type
            switch (list[i].type)
            {
                case ConditionType.DefeatAllTagged:
                    list[i].targetTag = EditorGUILayout.TagField("Target Tag", list[i].targetTag);
                    break;

                case ConditionType.SurviveForTime:
                    list[i].timeValue = EditorGUILayout.FloatField("Time To Survive (s)", list[i].timeValue);
                    break;

                case ConditionType.PlayerDies:
                    // This condition needs no extra variables.
                    EditorGUILayout.HelpBox("Loses the level if the player is destroyed.", MessageType.Info);
                    break;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(5);
        }
    }
}
