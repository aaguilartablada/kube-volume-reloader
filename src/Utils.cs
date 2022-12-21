namespace Kubevolumereloader;

public static class Utils
{
  public static readonly List<string> ValidObjets =  new List<string>(){
    "deployment",
    "statefulset",
    "daemonset"
  };

  public static string GetAnnotation(IDictionary<string, string> annotations, string annotation)
  {
    if (annotations == null) return "";
    if (!annotations.ContainsKey(annotation)) return "";
    return annotations[annotation];
  }

  public static bool CheckDictionaryEquality(IDictionary<string, string> dict1, IDictionary<string, string> dict2)
  {
    var equal = false;
    if (dict1.Count() == dict2.Count())
    {
      equal = true;
      foreach (var entry in dict1)
      {
        if (dict2.TryGetValue(entry.Key, out var value))
        {
          if (entry.Value != value)
          {
            equal = false;
            break;
          }
        }
        else
        {
          equal = false;
          break;
        }
      }
    }
    return equal;
  }

  public static bool CheckDictionaryEquality(IDictionary<string, byte[]> dict1, IDictionary<string, byte[]> dict2)
  {
    var equal = false;
    if (dict1.Count() == dict2.Count())
    {
      equal = true;
      foreach (var entry in dict1)
      {
        if (dict2.TryGetValue(entry.Key, out var value))
        {
          if (Encoding.Default.GetString(entry.Value) != Encoding.Default.GetString(value))
          {
            equal = false;
            break;
          }
        }
        else
        {
          equal = false;
          break;
        }
      }
    }
    return equal;
  }

  public static Dictionary<string,List<string>> SplitAnnotation(string annotation)
  {
    var response = new Dictionary<string, List<string>>();
    foreach(var kind in ValidObjets)
    {
      response[kind] = new List<string>();
    };

    // Annotation is a comma-separated string with values <object>/<objectName> (for example deployment/mydeployment,statefulset/onestatefulset)
    var splitAnnotation = annotation.Split(",");
    for (var i = 0; i < splitAnnotation.Length; i++)
    {
      var kind = splitAnnotation[i].Split("/")[0];
      var name = splitAnnotation[i].Split("/")[1];
      if (ValidObjets.Contains(kind))
        response[kind].Add(name);
    }
    return response;
  }
}