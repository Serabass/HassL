using HassLanguage.Core.Ast;

namespace HassLanguage.Core.Validation;

public class SymbolTable
{
  private readonly Dictionary<string, ZoneDeclaration> _zones = new();
  private readonly Dictionary<string, AreaDeclaration> _areas = new();
  private readonly Dictionary<string, DeviceDeclaration> _devices = new();
  private readonly Dictionary<string, EntityDeclaration> _entities = new();

  public bool AddZone(string alias, ZoneDeclaration zone)
  {
    if (_zones.ContainsKey(alias))
    {
      return false;
    }
    _zones[alias] = zone;
    return true;
  }

  public bool AddArea(string fullPath, AreaDeclaration area)
  {
    if (_areas.ContainsKey(fullPath))
    {
      return false;
    }
    _areas[fullPath] = area;
    return true;
  }

  public bool AddDevice(string fullPath, DeviceDeclaration device)
  {
    if (_devices.ContainsKey(fullPath))
    {
      return false;
    }
    _devices[fullPath] = device;
    return true;
  }

  public bool AddEntity(string fullPath, EntityDeclaration entity)
  {
    if (_entities.ContainsKey(fullPath))
    {
      return false;
    }
    _entities[fullPath] = entity;
    return true;
  }

  public bool EntityExists(string path)
  {
    // Check direct entity path
    if (_entities.ContainsKey(path))
    {
      return true;
    }

    // Check if path matches any entity (e.g., "kitchen.sensors.motion")
    var parts = path.Split('.');
    if (parts.Length >= 3)
    {
      var entityPath = string.Join(".", parts);
      return _entities.ContainsKey(entityPath);
    }

    return false;
  }

  public EntityDeclaration? GetEntity(string path)
  {
    return _entities.TryGetValue(path, out var entity) ? entity : null;
  }

  public void Clear()
  {
    _zones.Clear();
    _areas.Clear();
    _devices.Clear();
    _entities.Clear();
  }
}
