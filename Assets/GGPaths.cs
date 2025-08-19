// 8/19/2025 AI-Tag
// This was created with the help of Assistant, a Unity Artificial Intelligence product.

// Fix for GGPaths.Json being used as a method instead of a property
using UnityEngine;

public static class GGPaths
{
    // Example method to retrieve a path (modify as per your project logic)
    public static string GetPath(string key)
    {
        // Replace with actual path logic
        return Application.dataPath + "/" + key;
    }

    // Example property to simulate a Json path
    public static string Json
    {
        get { return GetPath("Json"); }
    }
}