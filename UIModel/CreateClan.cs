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

[ProtoContract]
public class HttpVerInfo
{
    [ProtoMember(1)]
    public uint code;
    [ProtoMember(2)]
    public bool is_server_open;
    [ProtoMember(3)]
    public string billboard_msg;
    [ProtoMember(4)]
    public string remote_version;
    [ProtoMember(5)]
    public string cdn_url;
    [ProtoMember(6)]
    public string server_url;
    [ProtoMember(7)]
    public bool is_review_server;
    [ProtoMember(8)]
    public string appstore_url;
    [ProtoMember(9)]
    public bool force_to_restart_app;
}

public enum HttpErrorCode
{
    OK,
    InvalidArgs,
    Exception,
    Timeout,
    DataTypeError,
}
