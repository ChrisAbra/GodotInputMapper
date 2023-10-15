namespace GodotInputMapper;

[AttributeUsage(AttributeTargets.Class)]
public class InputMapPlayerIndexAttribute : Attribute
{
    public string PlayerIndexMemberName;

    public InputMapPlayerIndexAttribute(string memberName){
        PlayerIndexMemberName = memberName;
    }
}
