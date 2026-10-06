namespace Dashboard.Health;

/// <summary>The system the dashboard shows: the content of <c>topology.json</c>.</summary>
public sealed record Topology(SystemInfo System, DateTimeOffset? Generated, IReadOnlyList<EnvironmentInfo> Environments)
{
    /// <summary>
    /// True when a deployable of any environment has a Front Door endpoint: the page's help then speaks of Front Door.
    /// </summary>
    public bool HasFrontDoor => Environments.Any(environment => environment.Deployables.Any(deployable => deployable.FrontDoor is not null));
}

/// <param name="Slug">The system's short name.</param>
/// <param name="Name">The name the header shows.</param>
/// <param name="Repository">The system repository on GitHub, where the deployments pin the versions.</param>
/// <param name="DeliveryUrl">Where the browser reads the system's delivery facts (<c>delivery.json</c>); null without them.</param>
public sealed record SystemInfo(string Slug, string Name, Uri? Repository = null, Uri? DeliveryUrl = null);

/// <param name="Name">The environment's name.</param>
/// <param name="Tier">The tier, for example <c>nonprod</c>.</param>
/// <param name="Deployables">The deployables the dashboard checks in this environment.</param>
/// <param name="VersionsUrl">
/// Where the browser reads the environment's <c>versions.json</c>: the versions the deployments pinned in Git.
/// </param>
/// <param name="VersionsHistoryUrl">The page with the history of that file.</param>
/// <param name="Links">Where the environment's resources are (<see cref="LinkSet"/>); null without links.</param>
public sealed record EnvironmentInfo(
    string Name,
    string? Tier,
    IReadOnlyList<DeployableInfo> Deployables,
    Uri? VersionsUrl = null,
    Uri? VersionsHistoryUrl = null,
    LinkSet? Links = null);

/// <summary>
/// One deployable of an environment: its public address (Front Door), the nodes behind it and the page of the project
/// that deploys it (<c>ProjectUrl</c>, in Octopus Deploy).
/// </summary>
/// <param name="TelemetryPath">
/// Where a node reports its own calls of the last minute (<c>/_telemetry</c>); null when the app has no such endpoint.
/// </param>
/// <param name="TrafficPaths">The paths the traffic button calls: representative requests, the start page first.</param>
/// <param name="BuildPath">
/// Where the primary node reports the build it runs (<c>/_build</c>, see <see cref="BuildInfo"/>); null when the app
/// has no such endpoint.
/// </param>
/// <param name="Links">Where the deployable's resources are (<see cref="LinkSet"/>); null without links.</param>
/// <param name="PinUrl">
/// Where the browser reads the deployable's own pin: a Kustomize file whose first <c>newTag</c> is the version pinned
/// in Git. Null when the environment's <c>versions.json</c> holds the pin.
/// </param>
/// <param name="PinHistoryUrl">The page with the history of that file; null for the history of <c>versions.json</c>.</param>
public sealed record DeployableInfo(
    string Name,
    Uri? FrontDoor,
    string HealthPath,
    string AlivePath,
    string VersionPath,
    IReadOnlyList<NodeInfo> Nodes,
    Uri? ProjectUrl = null,
    string? TelemetryPath = null,
    IReadOnlyList<string>? TrafficPaths = null,
    string? BuildPath = null,
    LinkSet? Links = null,
    Uri? PinUrl = null,
    Uri? PinHistoryUrl = null)
{
    public const string DefaultHealthPath = "/_healthcheck";
    public const string DefaultAlivePath = "/alive";
    public const string DefaultVersionPath = "/_version";

    /// <summary>The name of the file that holds the deployable's pin: its kustomization's, or <c>versions.json</c>.</summary>
    public string PinFile => PinUrl is null ? PinnedVersions.FileName : PinnedVersions.FileOf(PinUrl);

    /// <summary>The path the chosen probe calls on every node of this deployable.</summary>
    public string PathFor(ProbeKind probe) => probe == ProbeKind.Liveness ? AlivePath : HealthPath;
}

/// <summary>One regional node (web app) of a deployable.</summary>
/// <param name="Links">Where the node's numbers lead (<see cref="LinkSet"/>); null without links.</param>
public sealed record NodeInfo(string Name, string? Region, string Role, Uri Url, LinkSet? Links = null)
{
    public const string PrimaryRole = "primary";
    public const string StandbyRole = "standby";

    public bool IsPrimary => string.Equals(Role, PrimaryRole, StringComparison.OrdinalIgnoreCase);
}
