using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ElementModel
{
    public string ElementName { get; set; }
    public string ElementType { get; set; }
    public List<string> ChildElements { get; set; }
}
