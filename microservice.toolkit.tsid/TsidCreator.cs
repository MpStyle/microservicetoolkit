namespace microservice.toolkit.tsid;

public static class TsidCreator
{
    private TsidCreator()
    {
    }

    public static Tsid Tsid256()
    {
        return Factory256Holder.instance.Create();
    }

    public static Tsid Tsid1024()
    {
        return Factory1024Holder.instance.Create();
    }

    public static Tsid Tsid4096()
    {
        return Factory4096Holder.instance.Create();
    }

    private static class Factory256Holder
    {
        private static readonly TsidFactory instance = TsidFactory.NewInstance256();
    }

    private static class Factory1024Holder
    {
        private static readonly TsidFactory instance = TsidFactory.NewInstance1024();
    }

    private static class Factory4096Holder
    {
        private static readonly TsidFactory instance = TsidFactory.NewInstance4096();
    }
}
