namespace GodotInputMapper;

[AttributeUsage((AttributeTargets.Delegate | AttributeTargets.Field| AttributeTargets.Property))]
public class MapToInputAttribute : Attribute
{
    public enum InputLevel
    {
        Getter,
        _UnhandledInput,
        _Input
    }
    public string ActionName;
    public InputLevel Level;
    public bool FireOnRelease = false;

    public bool HandleInput = false;

    //To allow the source generator to map the syntax to this property it needs a matching constructor.
    //The source generator reads the Level enum as an int and uses Activator.CreateInstance needs it to be match
    //Necessarily public but use is discouraged in favour of more typed InputLevel enum;
    public MapToInputAttribute(string actionName, int level, bool handleInput = false, bool fireOnRelease = true)
    {
        ActionName = actionName;
        Level = (InputLevel)level;
        HandleInput = handleInput;
        FireOnRelease = fireOnRelease;
    }

    public MapToInputAttribute(string actionName, InputLevel level, bool handleInput = false, bool fireOnRelease = true)
    {
        ActionName = actionName;
        Level = level;
        HandleInput = handleInput;
        FireOnRelease = fireOnRelease;
    }

}
