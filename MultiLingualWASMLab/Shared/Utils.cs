using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.Shared;

public static class Utils
{
  /// <summary>
  /// JsonSerializer Extension
  /// </summary>
  public static string JsonSerialize<TValue>(TValue value, bool UnsafeRelaxedJsonEscaping = true, bool WriteIndented = true)
  {
    var options = new JsonSerializerOptions()
    {
      WriteIndented = WriteIndented
    };

    if (UnsafeRelaxedJsonEscaping)
      options.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;

    return JsonSerializer.Serialize(value, options);
  }
}
