# Godot Input Mapper

A source generator based package designed to make nicer C# API for Godot's Input system.

Goals:
- Ergonimc API with sensible defaults
- Easy support for multiple Players across multiple devices
- Elimiation/Minimisation of string references and runtime errors

⚠️ Work in progress

## Examples

```csharp
//Example Decoration on an Analog Stick
[MapToInput("Look", MapToInputAttribute.InputLevel.Getter, true)]
public Vector2 LookDirection => _lookDirection;

//Generates:
private Vector2 _lookDirection => Input.GetVector( "Look_-X", "Look_+X",  "Look_-Y",  "Look_+Y");
```

```csharp
//Example Decoration on an Trigger
[InputMapPlayerIndex(nameof(PlayerIndex))]
public partial class PlayerController : Node {

    [Export]
    public int PlayerIndex = 0;

    [MapToInput("Shoot", MapToInputAttribute.InputLevel.Getter)]
    public float Shooting => _shooting;

}
//Generates:
public partial class PlayerController : Node {
    private float _shooting  => Input.GetActionStrength("Shoot_" + PlayerIndex);
}
```


## Usage


The Attribute `[MapToInput]` can be used to decorate Properties and Delegates on any class that inherits from Node.

- The first parameter, `ActionName`, is required, it's the first stub of the Action Name defined in the InputMap/Project settings. It can be modified based on the property type such as `Vector2` or use of `[InputMapPlayerIndex]`.
- The second parameter, `Level`, is also required, it's an enum with the options:
    - `MapToInputAttribute.InputLevel.Getter` : Places the check in a Get method. Useful for checking input in a _Process. Uses InputMap.
    - `MapToInputAttribute.InputLevel._UnhandledInput` : Places the check in a an _UnhandledInput callback. Useful for delegates.
    - `MapToInputAttribute.InputLevel._Input`: Places the check in a an _Input callback useful for GUI checks.
- The third parameter, `HandleInput`, defaults to false, and decides whether to call Viewport.SetInputAsHandled();
- The fourth parameter, `FireOnRelease`, defaults to true, relates only to delegates and expects the delegate to have a single bool property `True` when IsActionPressed and `False` when IsActionReleased

Acceptable Property types are:
- `Vector2`
- `float`
- `bool`

Acceptable Delegate return types are `void` and with either no parameters or a single bool 
