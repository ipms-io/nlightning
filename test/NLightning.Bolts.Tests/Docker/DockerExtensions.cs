using Docker.DotNet;
using Docker.DotNet.Models;

namespace NLightning.Bolts.Tests.Docker;
//
// public static class DockerExtensions
// {
//     public static async Task<string?> GetGitlabRunnerNetworkId(this DockerClient _client)
//     {
//         var listContainers = await _client.Containers.ListContainersAsync(new ContainersListParameters());
//         var x = listContainers.FirstOrDefault(x =>
//             x.Labels.Any(y => y.Key == "com.gitlab.gitlab-runner.type" && y.Value == "build") && x.State == "running");
//         if (x != null)
//             return x.NetworkSettings.Networks["bridge"].NetworkID;
//         return null;
//     }
// }