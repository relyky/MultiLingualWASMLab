using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;

namespace MultiLingualWASMLab.Shared;

[AttributeUsage(
   AttributeTargets.Field |
   AttributeTargets.Property,
   AllowMultiple = true)]
public class PropertyNameAttribute : Attribute
{
  public string Name { get; set; }
  public string Culture { get; set; }

  public PropertyNameAttribute(string name)
  {
    Name = name;
    Culture = string.Empty;
  }

  public PropertyNameAttribute(string name, string culture)
  {
    Name = name;
    Culture = culture;
  }
}
