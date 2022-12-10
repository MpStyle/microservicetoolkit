namespace microservice.toolkit.tsid;

public class Tsid
{
    public long Number { get; }

    internal Tsid(long number)
    {
        this.Number = number;
    }

    public override string ToString()
    {
        return this.ToString(TsidProps.ALPHABET_UPPERCASE);
    }
}
