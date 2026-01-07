using System;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace TextScrambler.Models
{
    public class AppSettings
    {
        public string TriggerModifier { get; set; } = "Shift"; // Alt, Ctrl, Shift
        public string Theme { get; set; } = "Auto"; // Auto, Dark, Light
        public string DefaultPin { get; set; } = "1234";
        public bool RunOnStartup { get; set; } = true;
    }
}
