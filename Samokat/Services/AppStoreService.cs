using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Samokat.Services
{
    public class AppStoreService
    {
        public bool ContainsKey(string key)
        {
            return Preferences.ContainsKey(key);
        }

        public void Remove(string key)
        {
            Preferences.Remove(key);
        }

        public void ClearAll()
        {
            Preferences.Clear();
        }
 
        public void Set<T>(string key, T value)
        {
            if (value is string str)
                Preferences.Set(key, str);
            else if (value is int i)
                Preferences.Set(key, i);
            else if (value is bool b)
                Preferences.Set(key, b);
            else if (value is double d)
                Preferences.Set(key, d);
            else if (value is float f)
                Preferences.Set(key, f);
            else if (value is long l)
                Preferences.Set(key, l);
            else if (value != null)
            {
                var json = JsonSerializer.Serialize(value);
                Preferences.Set(key, json);
            }
        }
 
        public T? Get<T>(string key, T defaultValue = default!)
        {
            try
            {
                if (!Preferences.ContainsKey(key))
                    return defaultValue;

                if (typeof(T) == typeof(string))
                    return (T)(object)Preferences.Get(key, (string)(object)defaultValue!);
                if (typeof(T) == typeof(int))
                    return (T)(object)Preferences.Get(key, (int)(object)defaultValue!);
                if (typeof(T) == typeof(bool))
                    return (T)(object)Preferences.Get(key, (bool)(object)defaultValue!);
                if (typeof(T) == typeof(double))
                    return (T)(object)Preferences.Get(key, (double)(object)defaultValue!);
                if (typeof(T) == typeof(float))
                    return (T)(object)Preferences.Get(key, (float)(object)defaultValue!);
                if (typeof(T) == typeof(long))
                    return (T)(object)Preferences.Get(key, (long)(object)defaultValue!);

                // Deserialize JSON for custom types
                var json = Preferences.Get(key, "");
                return !string.IsNullOrEmpty(json)
                    ? JsonSerializer.Deserialize<T>(json)
                    : defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}
