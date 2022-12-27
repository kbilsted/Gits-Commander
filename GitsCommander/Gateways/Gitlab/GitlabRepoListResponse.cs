#nullable disable

namespace GitsCommander.Gateways.Gitlab;

class GitlabRepoListResponse
{
    public int id { get; set; }
    public string description { get; set; }
    public string name { get; set; }
    public string name_with_namespace { get; set; }
    public string path { get; set; }
    public string path_with_namespace { get; set; }
    public DateTime created_at { get; set; }
    public string default_branch { get; set; }
    public object[] tag_list { get; set; }
    public object[] topics { get; set; }
    public string ssh_url_to_repo { get; set; }
    public string http_url_to_repo { get; set; }
    public string web_url { get; set; }
    public string readme_url { get; set; }
    public object avatar_url { get; set; }
    public string container_registry_image_prefix { get; set; }
    public bool packages_enabled { get; set; }
    public bool empty_repo { get; set; }
    public bool archived { get; set; }
    public string visibility { get; set; }
    public bool container_registry_enabled { get; set; }
    public bool service_desk_enabled { get; set; }
}

