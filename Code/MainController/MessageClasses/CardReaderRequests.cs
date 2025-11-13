using System;
using SASComms;
using BitbossInterface;
using System.IO.Pipes;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Newtonsoft.Json;


namespace MainController
{
    // Card Reader Requests

    // Meters Response
    public class SetEGMIdRequest
    {
        public byte[] AssetNumber;
        public byte[] Denom;
        public byte[] SerialNumber;
        public byte[] Location;
    }

  

}
