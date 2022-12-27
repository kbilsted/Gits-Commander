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
    public bool share_with_group_lock { get; set; }
    public bool require_two_factor_authentication { get; set; }
    public int two_factor_grace_period { get; set; }
    public string project_creation_level { get; set; }
    public object auto_devops_enabled { get; set; }
    public string subgroup_creation_level { get; set; }
    public bool? emails_disabled { get; set; }
    public bool? mentions_disabled { get; set; }
    public bool lfs_enabled { get; set; }
    public int default_branch_protection { get; set; }
    public string avatar_url { get; set; }
    public bool request_access_enabled { get; set; }
    public string full_name { get; set; }
    public string full_path { get; set; }
    public DateTime created_at { get; set; }
    public int? parent_id { get; set; }
    public object ldap_cn { get; set; }
    public int? ldap_access { get; set; }
    public string marked_for_deletion_on { get; set; }
    public GitlabRepoLdap_Group_Links[] ldap_group_links { get; set; }
}

public class GitlabRepoLdap_Group_Links
{
    public object cn { get; set; }
    public int group_access { get; set; }
    public string provider { get; set; }
    public string filter { get; set; }
}