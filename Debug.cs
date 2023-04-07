using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Fjord.Graphics;
using Fjord.Input;
using Fjord.Scenes;
using Fjord.Ui;
using static Fjord.Helpers;
using static SDL2.SDL;

namespace Fjord.Scenes;

public class FieldExport : Attribute
{

}

public struct DebugLog
{
    public string time;
    public string sender;
    public string message;
    public LogLevel level;
    public int repeat;
    public bool hideInfo;

    public override bool Equals(object? obj)
    {
        return obj is DebugLog log &&
               sender == log.sender &&
               message == log.message &&
               level == log.level &&
               hideInfo == log.hideInfo;
    }
}

public enum LogLevel
{
    User,
    Message,
    Warning,
    Error,
}

public static class Debug {

    public static List<DebugLog> Logs = new List<DebugLog>();
    internal static DebugLog lastTopMessage;

    public static Dictionary<string, Action<object[]>> commands = new Dictionary<string, Action<object[]>>();

    public static void RegisterCommand(string id, Action<object[]> callback) {
        if (!commands.ContainsKey(id))
        {
            commands.Add(id, callback);
        } else
        {
            Debug.Log(LogLevel.Error, $"Couldn't register command '{id}'! Command already exists;");
        }
    }
    public static void RegisterCommand(string[] ids, Action<object[]> callback)
    {
        foreach (string id in ids)
            RegisterCommand(id, callback);
    }

    public static void Initialize()
    {
        SceneHandler.Register(new InspectorScene((int)(Game.Window.Width * 0.20), 1080, "inspector")
            .SetAllowWindowResize(false)
            .SetAlwaysRebuildTexture(true)
            .SetRelativeWindowSize(0.8f, 0f, 1.01f, 1f));

        SceneHandler.Register(new ConsoleScene((int)(Game.Window.Width * 0.2), (int)(Game.Window.Height * 0.4), "console")
            .SetAllowWindowResize(true)
            .SetRelativeWindowSize(0.1f, 0.1f, 0.3f, 0.5f)
            .SetAlwaysRebuildTexture(true));

        RegisterCommand("clear", (args) =>
        {
            Logs = new();
        });

        RegisterCommand(new string[] { "q", "quit" }, (args) =>
        {
            Game.Stop();
        });
    }

    public static SDL_FRect DebugWindowOffset = new()
    {
        x = 0f,
        y = 0f,
        w = 0.2f,
        h = 0f
    };

    public static void Log(string message)
    {
        Log(LogLevel.Message, message);
    }

    public static void Log(LogLevel level, string message)
    {
        var words = message.Split();
        // List<string> messageSplit = message.ToString().SplitInParts(60).ToList();

        var lines = new List<string> { words[0] };
        var lineNum = 0;
        for(int i = 1; i < words.Length; i++)
        {
            if(lines[lineNum].Length + words[i].Length + 1 <= 120)
                lines[lineNum] += " " + words[i];
            else
            {
                lines.Add(words[i]);
                lineNum++;
            }
        }

        StackTrace stackTrace = new StackTrace(); 
        StackFrame? stackFrame = stackTrace.GetFrame(1);

        int idx = -1;
        foreach(string i in lines) {
            idx++;
            if(stackFrame is not null) {
                System.Reflection.MethodBase? methodBase = stackFrame.GetMethod();
                if(methodBase is not null) {
                    var names = methodBase.DeclaringType;
                    if (names is not null) {
                        var logtmp = new DebugLog() {
                            level = level,
                            time = DateTime.Now.ToString("hh:mm:ss"),
                            sender = names.Namespace + "." + names.Name,
                            message = i,
                            hideInfo = idx != 0
                        };

                        Logs.Add(logtmp);
                        //lastTopMessage = logtmp;

                        //if (level != LogLevel.User)
                        //{
                        //    if (idx == 0)
                        //    {
                        //        Console.WriteLine(Logs.Count.ToString());
                        //        if (Logs.Count > 0)
                        //        {
                        //            Console.WriteLine("Help");
                        //            if (!lastTopMessage.Equals(logtmp))
                        //            {
                        //                Console.WriteLine("Help2");
                        //                Logs.Add(logtmp);
                        //                lastTopMessage = logtmp;
                        //            }
                        //            else
                        //            {
                        //                logtmp.repeat = Logs[Logs.Count - 1].repeat + 1;
                        //                Logs[Logs.Count - 1] = logtmp;
                        //                return;
                        //            }
                        //        }
                        //        else
                        //        {
                        //            Logs.Add(logtmp);
                        //            lastTopMessage = logtmp;
                        //        }
                        //    }
                        //    else
                        //    {
                        //        Logs.Add(logtmp);
                        //    }
                        //} else
                        //{
                        //    Logs.Add(logtmp);
                        //}
                    }
                }
            }
        }
    }

    public static void PerformCommand(string command, object[] args) {
        if(commands.ContainsKey(command)) {
            try {
                commands[command](args);
            } catch(Exception e) {
                Debug.Log(LogLevel.Error, e.ToString());
            }
        } else {
            Debug.Log(LogLevel.Error, "Invalid Command");
        }
    }
}

public class InspectorScene : Scene
{
    [FieldExport]
    bool ShowLoadedScenes = true;

    public InspectorScene(int width, int height, string id) : base(width, height, id)
    {
        SetClearColor(UiColors.Background);
    }

    public override void Render()
    {
        new UiBuilder(new Vector4(0, 0, (int)(Game.Window.Width * 0.2), (int)Game.Window.Height), LocalMousePosition)
            .Title("Inspector")
            .Container(
                new UiBuilder()
                    .Title("Scenes")
                    .ForEach(SceneHandler.Scenes.ToList(), (val, idx) =>
                    {
                        var list = new List<object>() {
                                new UiTitle(val.Key),
                                new UiButton("Load", () => SceneHandler.Load(val.Key)),
                                new UiButton("Unload", () => SceneHandler.Unload(val.Key)),
                                new UiButton("Remake", () => SceneHandler.Remake(val.Key)),
                                new UiButton("Apply Aspect Ratio", () => val.Value.ApplyOriginalAspectRatio()),
                                new UiCheckbox("Allow window resize", val.Value.AllowWindowResize, () => val.Value.SetAllowWindowResize(!val.Value.AllowWindowResize)),
                                new UiCheckbox("Always at back", val.Value.AlwaysAtBack, () => val.Value.SetAlwaysAtBack(!val.Value.AlwaysAtBack)),
                                new UiCheckbox("Always rebuild texture", val.Value.AlwaysRebuildTexture, () => val.Value.SetAlwaysRebuildTexture(!val.Value.AlwaysRebuildTexture))
                        };

                        FieldInfo[] infos = val.Value.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        List<object> exports = new() {
                            
                        };

                        foreach(var fi in infos) {
                            if (fi.IsDefined(typeof(FieldExport), true))
                            {
                                exports.Add(new UiText(fi.Name));
                                if (fi.GetValue(val.Value).GetType() == typeof(string))
                                {
                                    exports.Add(new UiTextField(fi.Name, fi.GetValue(val.Value).ToString(), (result) =>
                                    {
                                        fi.SetValue(val.Value, result);
                                    }, (result) => { }));
                                } else if(fi.GetValue(val.Value).GetType() == typeof(bool)) {
                                    exports.Add(new UiCheckbox(fi.Name, (bool)fi.GetValue(val.Value), () =>
                                    {
                                        fi.SetValue(val.Value, !(bool)fi.GetValue(val.Value));
                                    }));
                                } else
                                {
                                    exports.Add(new UiText($"{fi.Name} has an unsupported type: {fi.GetValue(val.Value).GetType()}!"));
                                }
                            }
                        }

                        if (exports.Count > 0)
                        {
                            list.Add(new UiTitle($"Exports"));
                            list.Add(exports);
                        }

                        if (idx != SceneHandler.Scenes.ToList().Count - 1)
                        {
                            list.Add(new UiSpacer());
                        }
                        
                        return list;
                    })
                    .If(ShowLoadedScenes, new UiBuilder()
                        .Title("Loaded Scenes")
                        .ForEach(SceneHandler.LoadedScenes, (scene, idx) =>
                        {
                            var list = new List<object>()
                            {
                                new UiTitle(scene),
                                new UiButton("Unload", () => SceneHandler.Unload(scene))
                            };

                            if (idx != SceneHandler.LoadedScenes.Count - 1)
                            {
                                list.Add(new UiSpacer());
                            }

                            return list;
                        })
                        .Build()
                    )
                    .Build()
            )
            .Render();
    }
}


public class ConsoleScene : Scene
{
    string consoleInput = "";
    float yOffset = 0;

    public ConsoleScene(int width, int height, string id) : base(width, height, id)
    {
        SetClearColor(UiColors.Background);
    }

    public override void Render()
    {
        if(Mouse.ScrollDown) {
            yOffset -= 10;
        }
        if(Mouse.ScrollUp) {
            yOffset += 10;
        }

        new UiBuilder(new Vector4(0, yOffset, 0, 0), LocalMousePosition)
            .Title("Console")
            .ForEach(Debug.Logs, (val, idx) =>
            {
                switch(val.level) {
                    case LogLevel.User: {
                        return new UiText(val.message);
                    }
                    default: {
                        return new UiDebugLog(val.level, val.time, val.sender, val.message, val.hideInfo, val.repeat);
                    } 
                }
            })
            .Render(out int uiHeight);

        // Math.Clamp(yOffset, 0, uiHeight);
        if(uiHeight > LocalWindowSize.h) {
            if(-yOffset < 0) {
                yOffset = 0;
            }
            if(-yOffset > uiHeight - LocalWindowSize.h + 50) {
                yOffset = -uiHeight + LocalWindowSize.h - 50;
            }
        } else {
            yOffset = 0;
        }

        var submitCommand = () => {
            Debug.Log(LogLevel.User, consoleInput);
            string command = consoleInput.Split(" ")[0];
            List<object> args = new List<object>();

            string currentWord = "";
            bool isString = false;
            string[] boolValues = {"true", "false"}; 

            void HandleCurrentWord()
            {
                if (currentWord != String.Empty)
                {
                    float value = 0f;
                    if (float.TryParse(currentWord, out value))
                    {
                        args.Add(value);
                    }
                    if(boolValues.Contains(currentWord.ToLower())) {
                        args.Add(currentWord.ToLower() == "true");
                    }
                    else
                    {
                        args.Add(currentWord);
                    }
                }
                currentWord = "";
            }

            foreach (char c in String.Join(" ", consoleInput.Split(" ").ToList().Skip(1)))
            {
                if (c == '"')
                {
                    isString = !isString;
                    if (!isString)
                    {
                        HandleCurrentWord();
                    }
                    continue;
                }
                if (isString)
                {
                    currentWord += c;
                }
                else if (c != ' ')
                {
                    currentWord += c;
                }
                else
                {
                    HandleCurrentWord();
                }
            }
            if (currentWord != String.Empty)
            {
                HandleCurrentWord();
            }

            Debug.PerformCommand(command, args.ToArray());
            consoleInput = "";
        };

        FUI.OverrideMousePosition(LocalMousePosition);
        FUI.TextFieldExt(new(10, LocalWindowSize.h - 40), "consolein", consoleInput, (val) => {consoleInput = val;}, (val) => submitCommand(), null, out Vector2 size);
        FUI.Button(new(Math.Min(size.X + 20, LocalWindowSize.w - 88), LocalWindowSize.h - 40), "Send", submitCommand);
        FUI.ResetMousePosition();
    }
}
