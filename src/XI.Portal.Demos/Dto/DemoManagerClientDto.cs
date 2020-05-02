using System;

namespace XI.Portal.Demos.Dto
{
    public class DemoManagerClientDto
    {
        public string Version { get; set; }
        public string Description { get; set; }
        public Uri Url { get; set; }
        public string Changelog { get; set; }
    }
}