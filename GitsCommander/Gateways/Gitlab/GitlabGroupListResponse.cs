#nullable disable
namespace GitsCommander.Gateways.Gitlab;

public class GitlabGroupListResponse
{
    public int id { get; set; }
    public string web_url { get; set; }
    public string name { get; set; }
    public string path { get; set; }
    public string description { get; set; }
    public string visibility { get; set; }
    public string project_creation_level { get; set; }
    public string subgroup_creation_level { get; set; }
    public string full_name { get; set; }
    public string full_path { get; set; }
    public DateTime created_at { get; set; }
    public int? parent_id { get; set; }
}
