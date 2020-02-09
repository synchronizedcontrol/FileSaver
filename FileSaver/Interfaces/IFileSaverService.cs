using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Xml;
using System.Xml.Serialization;
using System.Web;
using System.Web.Hosting;
using System.Security.Policy;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace FileSaver
{
    [ServiceContract]
    public interface IFileSaverService
    {
        [OperationContract]
        string InsertDeclaration(Declaration dec);

        [OperationContract]
        string SerializeObject(Message request);

        [OperationContract]
        SystemCheck checkSystem();
    }
}
