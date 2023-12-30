namespace ObjectDumper.Output;

public interface IDumpOutput
{
    void Write(string format, string expression, string content, bool isFileName);
}