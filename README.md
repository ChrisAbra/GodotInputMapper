# Godot Input Mapper

A source generator based package designed to make nicer C# API for Godot's Input system.

Goals:
- Ergonimc API with sensible defaults
- Easy support for multiple Players across multiple devices
- Elimiation/Minimisation of string references and runtime errors

⚠️ Work in progress

## Usage

```csharp
[MapToInput("Look", MapToInputAttribute.InputLevel.Getter, true)]
public Vector2 LookDirection { get => _lookDirection; }
```

The Attribute `[MapToInput]` can be used to decorate Properties and Delegates on any class that inherits from Node.

- The first parameter is required, it's the first stub of the Action Name defined in the InputMap/Project settings. It's modified in source generation based on 
