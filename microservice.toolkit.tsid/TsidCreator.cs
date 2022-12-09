namespace microservice.toolkit.tsid;

public class TsidCreator
{
    private static readonly TsidFactory instance256 = TsidFactory.NewInstance256();
    private static readonly TsidFactory instance1024 = TsidFactory.NewInstance1024();
    private static readonly TsidFactory instance4096 = TsidFactory.NewInstance4096();

    private TsidCreator()
    {
    }

    public static Tsid Tsid256()
    {
        return instance256.Create();
    }

    public static Tsid Tsid1024()
    {
        return instance1024.Create();
    }

    public static Tsid Tsid4096()
    {
        return instance4096.Create();
    }
}
