using System;
using System.Collections.Generic;

namespace TtWork.Project.Applications.Sessions.Dto
{
    public class ApplicationInfoDto
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public DateTime ReleaseDate { get; set; }

        public Dictionary<string, bool> Features { get; set; }
    }
}
