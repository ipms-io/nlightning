namespace NLightning.Bolts.Tests.BOLT8.Utils;

using Tests.Utils;

internal static class ValidMessagesUtil
{
    public static byte[] InitiatorCk => "919219dbb2920afa8db80f9a51787a840bcf111ed8d588caf9ab4be716e42b01".ToByteArray();
    public static byte[] InitiatorSk => "969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9".ToByteArray();
    public static byte[] InitiatorRk => "bb9020b8965f4df047e07f955f3c4b88418984aadc5cdb35096b9ea8fa5c3442".ToByteArray();
    public static byte[] Message0 => "cf2b30ddf0cf3f80e7c35a6e6730b59fe802473180f396d88a8fb0db8cbcf25d2f214cf9ea1d95".ToByteArray();
    public static byte[] Message1 => "72887022101f0b6753e0c7de21657d35a4cb2a1f5cde2650528bbc8f837d0f0d7ad833b1a256a1".ToByteArray();
    public static byte[] Message500 => "178cb9d7387190fa34db9c2d50027d21793c9bc2d40b1e14dcf30ebeeeb220f48364f7a4c68bf8".ToByteArray();
    public static byte[] Message501 => "1b186c57d44eb6de4c057c49940d79bb838a145cb528d6e8fd26dbe50a60ca2c104b56b60e45bd".ToByteArray();
    public static byte[] Message1000 => "4a2f3cc3b5e78ddb83dcb426d9863d9d9a723b0337c89dd0b005d89f8d3c05c52b76b29b740f09".ToByteArray();
    public static byte[] Message1001 => "2ecd8c8a5629d0d02ab457a0fdd0f7b90a192cd46be5ecb6ca570bfc5e268338b1a16cf4ef2d36".ToByteArray();
}