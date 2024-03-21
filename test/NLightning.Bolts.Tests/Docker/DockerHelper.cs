// using Docker.DotNet;
// using Docker.DotNet.Models;
//
// namespace NLightning.Bolts.Tests.Docker;
//
// using SharpCompress.Common;
// using SharpCompress.Writers;
//  
// public static class DockerHelper
// {
//     private static Random s_random = new();
//
//     /// <summary>
//     ///     Pull image (needs event to know when done), blocking until completed.
//     /// </summary>
//     /// <param name="client"></param>
//     /// <param name="imageName"></param>
//     /// <param name="tag"></param>
//     public static async Task PullImageAndWaitForCompleted(this DockerClient client, string imageName, string tag)
//     {
//         var done = false;
//         var p = new Progress<JSONMessage>();
//         p.ProgressChanged += (sender, message) =>
//         {
//             if (message.Status.StartsWith("Digest: sha256:") || message.Status.EndsWith("Already exists"))
//                 done = true;
//         };
//         await client.Images.CreateImageAsync(
//             new ImagesCreateParameters
//             {
//                 FromImage = imageName,
//                 Tag = tag
//             },
//             // new AuthConfig
//             // {
//             //     Email = "test@example.com",
//             //     Username = "test",
//             //     Password = "pa$$w0rd"
//             // },
//             null,
//             p);
//
//         while (!done)
//             await Task.Delay(1);
//     }
//
//     private static string GetRandomHexString(int size = 8)
//     {
//         var b = new byte[size];
//         s_random.NextBytes(b);
//         return Convert.ToHexString(b);
//     }
//
//     /// <summary>
//     ///     Build Random {baseName}_{random 8 CHAR HEX} network
//     /// </summary>
//     /// <param name="client"></param>
//     /// <returns>Network Id</returns>
//     public static async Task<string> BuildTestingNetwork(this DockerClient client, string baseName)
//     {
//         var randomString = GetRandomHexString();
//         var networksCreateResponse = await client.Networks.CreateNetworkAsync(new NetworksCreateParameters
//         {
//             Name = $"{baseName}_{randomString}",
//             Driver = "bridge",
//             CheckDuplicate = true
//         });
//         return networksCreateResponse.ID;
//     }
//
//     public static async Task CreateDockerImageFromPath(this DockerClient client, string path, List<string> tags)
//     {
//         var tarStream = path.MakeDockerTarFromFolder();
//         var p = new Progress<JSONMessage>();
//         await client.Images.BuildImageFromDockerfileAsync(new ImageBuildParameters
//         {
//             Tags = tags
//         }, tarStream, null, null, p);
//     }
//
//     public static MemoryStream MakeDockerTarFromFolder(this string path)
//     {
//         var stream = new MemoryStream();
//         using (var writer = WriterFactory.Open(stream, ArchiveType.Tar, new WriterOptions(CompressionType.None)))
//         {
//             foreach (var f in Directory.GetFiles(path))
//             {
//                 var source = new FileInfo(f);
//                 var tarPath = Path.GetFileName(f);
//                 writer.Write(tarPath, source);
//             }
//         }
//
//         stream.Position = 0;
//         return stream;
//     }
// }