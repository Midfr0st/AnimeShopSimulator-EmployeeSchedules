using Il2CppProject.Code.Gameplay.Configs;
using MelonLoader;
using MelonLoader.Utils;
using Newtonsoft.Json;

namespace WolfEmployeeSchedules;

internal sealed class RoleSchedule
{
    public bool StartBeforeOpening { get; set; }
    public bool ContinueAfterClosing { get; set; }
}

internal sealed class ScheduleSettingsData
{
    public RoleSchedule Cashier { get; set; } = new() { ContinueAfterClosing = true };
    public RoleSchedule Store { get; set; } = new();
    public RoleSchedule Storage { get; set; } = new();
    public RoleSchedule Cleaner { get; set; } = new();
    public RoleSchedule AutoOpener { get; set; } = new();

    public RoleSchedule? Get(EmployeeType type)
    {
        return type switch
        {
            EmployeeType.Cashier => Cashier,
            EmployeeType.Store => Store,
            EmployeeType.Storage => Storage,
            EmployeeType.Cleaner => Cleaner,
            EmployeeType.AutoOpener => AutoOpener,
            _ => null
        };
    }
}

internal static class ScheduleSettings
{
    private static string _path = string.Empty;

    public static ScheduleSettingsData Value { get; private set; } = new();

    public static void Initialize()
    {
        _path = Path.Combine(MelonEnvironment.UserDataDirectory, "WolfEmployeeSchedules.settings.json");
        if (!File.Exists(_path))
        {
            Save();
            return;
        }

        try
        {
            Value = JsonConvert.DeserializeObject<ScheduleSettingsData>(File.ReadAllText(_path)) ?? new();
            Normalize();
        }
        catch (Exception exception)
        {
            MelonLogger.Warning(
                $"Employee Schedules: настройки не прочитаны, используются стандартные: {exception.Message}");
            Value = new ScheduleSettingsData();
        }
    }

    public static void Save()
    {
        try
        {
            Normalize();
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var temporaryPath = _path + ".tmp";
            File.WriteAllText(temporaryPath, JsonConvert.SerializeObject(Value, Formatting.Indented));
            File.Move(temporaryPath, _path, true);
        }
        catch (Exception exception)
        {
            MelonLogger.Error($"Employee Schedules: настройки не сохранены: {exception}");
        }
    }

    private static void Normalize()
    {
        Value.Cashier ??= new RoleSchedule { ContinueAfterClosing = true };
        Value.Store ??= new RoleSchedule();
        Value.Storage ??= new RoleSchedule();
        Value.Cleaner ??= new RoleSchedule();
        Value.AutoOpener ??= new RoleSchedule();
    }
}
