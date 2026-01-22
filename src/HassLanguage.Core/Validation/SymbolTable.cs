using HassLanguage.Core.Ast;

namespace HassLanguage.Core.Validation;

public class SymbolTable
{
    private readonly Dictionary<string, HomeDeclaration> _homes = new();
    private readonly Dictionary<string, RoomDeclaration> _rooms = new();
    private readonly Dictionary<string, DeviceDeclaration> _devices = new();
    private readonly Dictionary<string, EntityDeclaration> _entities = new();

    public bool AddHome(string alias, HomeDeclaration home)
    {
        if (_homes.ContainsKey(alias))
        {
            return false;
        }
        _homes[alias] = home;
        return true;
    }

    public bool AddRoom(string fullPath, RoomDeclaration room)
    {
        if (_rooms.ContainsKey(fullPath))
        {
            return false;
        }
        _rooms[fullPath] = room;
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
        _homes.Clear();
        _rooms.Clear();
        _devices.Clear();
        _entities.Clear();
    }
}
