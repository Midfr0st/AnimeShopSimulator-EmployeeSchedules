using System.Reflection;
using HarmonyLib;
using Il2CppProject.Code.Gameplay.Configs;
using Il2CppProject.Code.Gameplay.Controllers;
using Il2CppProject.Code.Gameplay.UI.Menu;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(
    typeof(WolfEmployeeSchedules.WolfEmployeeSchedulesMod),
    "Anime Shop: Employee Schedules",
    "0.1.2",
    "WolfMods")]

namespace WolfEmployeeSchedules;

public sealed class WolfEmployeeSchedulesMod : MelonMod
{
    private const string WolfModId = "wolfmod.employee_schedules";
    private const int MaxRegistrationAttempts = 4;
    private const float RetryDelay = 1.5f;
    private const float DependencyErrorDisplaySeconds = 30f;

    private float _nextRegistrationAttempt;
    private float _dependencyErrorUntil;
    private int _registrationAttempts;
    private bool _runtimeInitialized;
    private bool _dependencyFailed;
    private bool _dependencyNotificationStarted;
    private GUIStyle? _dependencyErrorStyle;
    private MenuView? _mainMenuView;
    private float _nextMainMenuLookup;

    internal static bool FeatureEnabled { get; private set; }

    public override void OnInitializeMelon()
    {
        FeatureEnabled = false;
        TryInitialize();
    }

    public override void OnUpdate()
    {
        if (!_runtimeInitialized && !_dependencyFailed && Time.unscaledTime >= _nextRegistrationAttempt)
            TryInitialize();

        if (_runtimeInitialized)
            ScheduleRuntime.Tick();
    }

    public override void OnSceneWasLoaded(int buildIndex, string sceneName)
    {
        _mainMenuView = null;
        _nextMainMenuLookup = 0f;
        ScheduleRuntime.ResetSceneState();
    }

    public override void OnGUI()
    {
        if (!_dependencyFailed || !IsMainMenuVisible())
            return;

        if (!_dependencyNotificationStarted)
        {
            _dependencyNotificationStarted = true;
            _dependencyErrorUntil = Time.unscaledTime + DependencyErrorDisplaySeconds;
        }

        if (Time.unscaledTime < _dependencyErrorUntil)
            DrawDependencyError();
    }

    public override void OnDeinitializeMelon()
    {
        FeatureEnabled = false;
        ScheduleRuntime.RequestRefresh();
        ScheduleRuntime.Tick(force: true);
        SchedulePatches.Uninstall();
        WolfModBridge.Unregister(WolfModId);
    }

    private void TryInitialize()
    {
        _registrationAttempts++;
        if (!WolfModBridge.TryRegister(
                WolfModId,
                "Расписание работников",
                "0.1.2",
                "Отдельное расписание до открытия и после закрытия для каждой профессии.",
                SetFeatureEnabled,
                DrawWolfModSettings,
                () => false))
        {
            if (_registrationAttempts < MaxRegistrationAttempts)
            {
                _nextRegistrationAttempt = Time.unscaledTime + RetryDelay;
                return;
            }

            _dependencyFailed = true;
            FeatureEnabled = false;
            LoggerInstance.Error(
                "Employee Schedules отключён: WolfCore не найден или несовместим. " +
                $"Установите WolfCore.dll и перезапустите игру. {WolfModBridge.LastError}");
            return;
        }

        try
        {
            ScheduleSettings.Initialize();
            SchedulePatches.Install();
            ScheduleRuntime.RequestRefresh();
            _runtimeInitialized = true;
            LoggerInstance.Msg(
                "Employee Schedules 0.1.2 загружен. Настройки доступны через WolfCore.");
        }
        catch
        {
            FeatureEnabled = false;
            SchedulePatches.Uninstall();
            WolfModBridge.Unregister(WolfModId);
            throw;
        }
    }

    private static void SetFeatureEnabled(bool enabled)
    {
        FeatureEnabled = enabled;
        ScheduleRuntime.RequestRefresh();
    }

    private static void DrawWolfModSettings()
    {
        GUILayout.Label("Для каждой профессии можно отдельно расширить рабочий день.");
        GUILayout.Label("Обычное расписание игры не меняется, если обе галочки выключены.");
        GUILayout.Space(10f);

        var changed = false;
        changed |= DrawRole("Кассир", ScheduleSettings.Value.Cashier);
        changed |= DrawRole("Выкладчик", ScheduleSettings.Value.Store);
        changed |= DrawRole("Кладовщик", ScheduleSettings.Value.Storage);
        changed |= DrawRole("Уборщик", ScheduleSettings.Value.Cleaner);
        changed |= DrawRole("Распаковщик", ScheduleSettings.Value.AutoOpener);

        if (!changed)
            return;

        ScheduleSettings.Save();
        ScheduleRuntime.RequestRefresh();
    }

    private static bool DrawRole(string title, RoleSchedule role)
    {
        var changed = false;
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(title);

        var before = GUILayout.Toggle(role.StartBeforeOpening, "Начать работу до открытия магазина");
        if (before != role.StartBeforeOpening)
        {
            role.StartBeforeOpening = before;
            changed = true;
        }

        var after = GUILayout.Toggle(role.ContinueAfterClosing, "Продолжать работу после закрытия магазина");
        if (after != role.ContinueAfterClosing)
        {
            role.ContinueAfterClosing = after;
            changed = true;
        }

        GUILayout.EndVertical();
        GUILayout.Space(5f);
        return changed;
    }

    private void DrawDependencyError()
    {
        _dependencyErrorStyle ??= new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = 18,
            fontStyle = FontStyle.Bold,
            wordWrap = true,
            padding = new RectOffset(18, 18, 12, 12)
        };
        _dependencyErrorStyle.normal.textColor = new Color(1f, 0.86f, 0.86f, 1f);

        var width = Math.Min(760f, Screen.width - 48f);
        const float height = 112f;
        var bottomClearance = Math.Max(130f, Screen.height * 0.12f);
        GUI.depth = -20000;
        GUI.Box(
            new Rect(24f, Screen.height - height - bottomClearance, width, height),
            "Employee Schedules отключён\nWolfCore не найден или несовместим. " +
            "Установите WolfCore.dll и перезапустите игру.",
            _dependencyErrorStyle);
    }

    private bool IsMainMenuVisible()
    {
        try
        {
            if ((_mainMenuView == null || !_mainMenuView) && Time.unscaledTime >= _nextMainMenuLookup)
            {
                _nextMainMenuLookup = Time.unscaledTime + 2f;
                _mainMenuView = UnityEngine.Object.FindObjectOfType<MenuView>();
            }

            return _mainMenuView != null && _mainMenuView.isActiveAndEnabled &&
                   _mainMenuView.gameObject.activeInHierarchy;
        }
        catch
        {
            return false;
        }
    }
}

internal enum WorkPeriod
{
    BeforeOpening,
    Open,
    AfterClosing
}

internal static class ScheduleRuntime
{
    private const float ControllerLookupInterval = 1f;
    private const int FallbackClosingHour = 21;

    private static EmployeesController? _controller;
    private static float _nextControllerLookup;
    private static bool _refreshRequested = true;

    public static void ResetSceneState()
    {
        _controller = null;
        _nextControllerLookup = 0f;
        _refreshRequested = true;
        SchedulePatches.ClearForcedPeriod();
    }

    public static void RequestRefresh()
    {
        _refreshRequested = true;
    }

    public static void Tick(bool force = false)
    {
        // Once the controller is cached there is nothing to poll. Wake only after
        // a scene/setting/feature event explicitly requests a state refresh.
        if (!force && !_refreshRequested)
            return;

        if (_controller == null || !_controller)
        {
            if (!force && Time.unscaledTime < _nextControllerLookup)
                return;
            _nextControllerLookup = Time.unscaledTime + ControllerLookupInterval;
            _controller = UnityEngine.Object.FindObjectOfType<EmployeesController>();
            if (_controller == null)
                return;
            _refreshRequested = true;
        }

        _refreshRequested = false;
        try
        {
            _controller.RefreshAllWorkStates();
        }
        catch (Exception exception)
        {
            MelonLogger.Warning($"Employee Schedules: не удалось обновить состояния работников: {exception.Message}");
        }
    }

    public static bool ShouldExtend(EmployeeInfo info, EmployeesController controller)
    {
        if (!WolfEmployeeSchedulesMod.FeatureEnabled || info == null)
            return false;
        if (info.CurrentType == EmployeeType.None || !info.HasAssignedTier || info.HasDebt)
            return false;

        var role = ScheduleSettings.Value.Get(info.CurrentType);
        if (role == null)
            return false;

        return GetPeriod(controller) switch
        {
            WorkPeriod.BeforeOpening => role.StartBeforeOpening,
            WorkPeriod.AfterClosing => role.ContinueAfterClosing,
            _ => false
        };
    }

    public static WorkPeriod GetPeriod(EmployeesController controller)
    {
        if (SchedulePatches.ForcedPeriod is { } forced)
            return forced;
        if (controller._isDayStarted)
            return WorkPeriod.Open;

        try
        {
            var timeService = controller._timeService;
            if (timeService != null)
            {
                if (timeService.IsTimeUpdating)
                    return WorkPeriod.Open;

                var closingHour = timeService._timeController?._timeConfig?.EndHour ?? FallbackClosingHour;
                return timeService.CurrentTime.Hour >= closingHour
                    ? WorkPeriod.AfterClosing
                    : WorkPeriod.BeforeOpening;
            }
        }
        catch
        {
            // A controller may be only partially initialized during scene loading.
        }

        return WorkPeriod.BeforeOpening;
    }
}

internal static class SchedulePatches
{
    private const string HarmonyId = "wolfmod.employee_schedules";
    private static HarmonyLib.Harmony? _harmony;

    public static WorkPeriod? ForcedPeriod { get; private set; }

    public static void Install()
    {
        if (_harmony != null)
            return;

        _harmony = new HarmonyLib.Harmony(HarmonyId);
        Patch(
            "UpdateWorkState",
            nameof(UpdateWorkStatePrefix),
            nameof(UpdateWorkStatePostfix),
            nameof(UpdateWorkStateFinalizer));
        Patch(
            "HandleDayEnd",
            nameof(BeginAfterClosing),
            nameof(EndForcedPeriod),
            nameof(EndForcedPeriodFinalizer));
        Patch(
            "HandleNextDayStart",
            nameof(BeginBeforeOpening),
            nameof(EndForcedPeriod),
            nameof(EndForcedPeriodFinalizer));
    }

    public static void Uninstall()
    {
        _harmony?.UnpatchSelf();
        _harmony = null;
        ClearForcedPeriod();
    }

    public static void ClearForcedPeriod()
    {
        ForcedPeriod = null;
    }

    private static void Patch(
        string originalName,
        string? prefix = null,
        string? postfix = null,
        string? finalizer = null)
    {
        var original = AccessTools.Method(typeof(EmployeesController), originalName);
        if (original == null)
            throw new MissingMethodException(typeof(EmployeesController).FullName, originalName);

        _harmony!.Patch(
            original,
            prefix == null ? null : new HarmonyMethod(typeof(SchedulePatches), prefix),
            postfix == null ? null : new HarmonyMethod(typeof(SchedulePatches), postfix),
            finalizer: finalizer == null ? null : new HarmonyMethod(typeof(SchedulePatches), finalizer));
    }

    private static void UpdateWorkStatePrefix(
        EmployeesController __instance,
        EmployeeInfo info,
        out bool __state)
    {
        __state = false;
        if (!ScheduleRuntime.ShouldExtend(info, __instance))
            return;

        info.IsWorking = true;
        if (__instance._isDayStarted)
            return;

        __instance._isDayStarted = true;
        __state = true;
    }

    private static void UpdateWorkStatePostfix(EmployeesController __instance, bool __state)
    {
        if (__state)
            __instance._isDayStarted = false;
    }

    private static Exception? UpdateWorkStateFinalizer(
        Exception? __exception,
        EmployeesController __instance,
        bool __state)
    {
        if (__state)
            __instance._isDayStarted = false;
        return __exception;
    }

    private static void BeginAfterClosing()
    {
        ForcedPeriod = WorkPeriod.AfterClosing;
    }

    private static void BeginBeforeOpening()
    {
        ForcedPeriod = WorkPeriod.BeforeOpening;
    }

    private static void EndForcedPeriod()
    {
        ForcedPeriod = null;
    }

    private static Exception? EndForcedPeriodFinalizer(Exception? __exception)
    {
        ForcedPeriod = null;
        return __exception;
    }
}

internal static class WolfModBridge
{
    private const string RegistryTypeName = "WolfCore.WolfModRegistry";

    public static bool Registered { get; private set; }
    public static string LastError { get; private set; } = string.Empty;

    public static bool TryRegister(
        string id,
        string displayName,
        string version,
        string description,
        Action<bool> onEnabledChanged,
        Action drawSettings,
        Func<bool> isCapturingInput)
    {
        if (Registered)
            return true;

        var registry = FindRegistryType();
        if (registry == null)
        {
            LastError = "Тип WolfCore.WolfModRegistry пока не найден.";
            return false;
        }

        try
        {
            var register = registry.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .FirstOrDefault(method =>
                    method.Name == "Register" &&
                    method.GetParameters().Length == 8 &&
                    method.GetParameters()[0].ParameterType == typeof(string));
            if (register == null)
            {
                LastError = "Установленная версия WolfCore не поддерживает регистрацию модов.";
                return false;
            }

            register.Invoke(null, new object?[]
            {
                id,
                displayName,
                version,
                description,
                onEnabledChanged,
                drawSettings,
                isCapturingInput,
                true
            });

            Registered = true;
            LastError = string.Empty;
            MelonLogger.Msg("Employee Schedules: подключён к WolfCore.");
            return true;
        }
        catch (Exception exception)
        {
            LastError = exception.GetBaseException().Message;
            return false;
        }
    }

    public static void Unregister(string id)
    {
        if (!Registered)
            return;

        try
        {
            FindRegistryType()?.GetMethod(
                "Unregister",
                BindingFlags.Public | BindingFlags.Static)?.Invoke(null, new object?[] { id });
        }
        catch
        {
            // WolfCore may already be shutting down.
        }

        Registered = false;
    }

    private static Type? FindRegistryType()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (string.Equals(assembly.GetName().Name, "WolfCore", StringComparison.Ordinal))
                return assembly.GetType(RegistryTypeName, false);
        }

        return null;
    }
}
