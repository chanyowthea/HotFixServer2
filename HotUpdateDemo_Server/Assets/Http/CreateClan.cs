using ProtoBuf;

[ProtoContract]
public class CreateClanRes
{
    [ProtoMember(1)]
    public int[] _ids;
}

[ProtoContract]
public class CreateClanReq
{
    [ProtoMember(1)]
    public string _name;
}
