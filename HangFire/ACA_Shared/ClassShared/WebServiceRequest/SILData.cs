using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shared.ClassShared.WebServiceRequest
{
  public class SILData: WebService
  {
    private string webServiceBaseUrl { get; set; }  
    public override string GetWebSerive { get { return webServiceBaseUrl; } set { } }

    public SILData(string webServiceUrl)
        : base()
    {
      webServiceBaseUrl = webServiceUrl;
    }
  }
}
