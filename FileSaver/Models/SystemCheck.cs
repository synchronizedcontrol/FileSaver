using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace FileSaver
{
    public class SystemCheck
    {
        public bool SystemOnline;
        public string FolderPath;
        public bool CorrectFolderRights;
        public string Errors;
    }
}