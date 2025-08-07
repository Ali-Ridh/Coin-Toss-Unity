// FILE: JsonHelper.cs
// PURPOSE: A helper class to allow Unity's JsonUtility to parse arrays from the root of a JSON file.
using UnityEngine;

[System.Serializable]
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        // Manually add the "wrapper" around the JSON array string.
        // This turns "[...]" into "{ "Items": [...] }", which JsonUtility can understand.
        string newJson = "{ \"Items\": " + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items;
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
