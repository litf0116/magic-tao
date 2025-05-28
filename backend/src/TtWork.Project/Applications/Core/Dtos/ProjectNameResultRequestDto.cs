using System;
using System.Collections.Generic;
using TtWork.Abp.Applications.Dtos;

namespace TtWork.Project.Applications.Dtos
{
    public class CommentRequestDto : AppResultRequestDto
    {
        public int? ActivityId { get; set; }
    }

    public class CmsRequestDto : AppResultRequestDto
    {
        public int? CategoryId { get; set; }
    }

    public class Filter
    {
        public string key { get; set; }
        public List<String> values { get; set; }
    }
}