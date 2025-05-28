using System;
using System.ComponentModel.DataAnnotations;

namespace TtWork.HttpClient.Weixin.Models;

[Serializable]
public class GetJsSdkWeChatPayParametersInput {
    [Required] public string MchId { get; set; }

    [Required] public string AppId { get; set; }

    [Required] public string PrepayId { get; set; }
}