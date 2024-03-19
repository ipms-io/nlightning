namespace NLightning.Bolts.Tests.BOLT8.Utils;

using static Utils.TestUtils;

internal sealed class ValidMessagesUtil
{
    public byte[] InitiatorCk { get; }
    public byte[] InitiatorSk { get; }
    public byte[] InitiatorRk { get; }
    public byte[] Message0 { get; }
    public byte[] Message1 { get; }
    public byte[] Message500 { get; }
    public byte[] Message501 { get; }
    public byte[] Message1000 { get; }
    public byte[] Message1001 { get; }

    public ValidMessagesUtil()
    {
        InitiatorCk = GetBytes("0x919219dbb2920afa8db80f9a51787a840bcf111ed8d588caf9ab4be716e42b01");
        InitiatorSk = GetBytes("0x969ab31b4d288cedf6218839b27a3e2140827047f2c0f01bf5c04435d43511a9");
        InitiatorRk = GetBytes("0xbb9020b8965f4df047e07f955f3c4b88418984aadc5cdb35096b9ea8fa5c3442");
        Message0 = GetBytes("0xcf2b30ddf0cf3f80e7c35a6e6730b59fe802473180f396d88a8fb0db8cbcf25d2f214cf9ea1d95");
        Message1 = GetBytes("0x72887022101f0b6753e0c7de21657d35a4cb2a1f5cde2650528bbc8f837d0f0d7ad833b1a256a1");
        Message500 = GetBytes("0x178cb9d7387190fa34db9c2d50027d21793c9bc2d40b1e14dcf30ebeeeb220f48364f7a4c68bf8");
        Message501 = GetBytes("0x1b186c57d44eb6de4c057c49940d79bb838a145cb528d6e8fd26dbe50a60ca2c104b56b60e45bd");
        Message1000 = GetBytes("0x4a2f3cc3b5e78ddb83dcb426d9863d9d9a723b0337c89dd0b005d89f8d3c05c52b76b29b740f09");
        Message1001 = GetBytes("0x2ecd8c8a5629d0d02ab457a0fdd0f7b90a192cd46be5ecb6ca570bfc5e268338b1a16cf4ef2d36");
    }
}