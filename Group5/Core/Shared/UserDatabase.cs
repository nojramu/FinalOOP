using System.Collections.Generic;
using System.Linq;
using Group5.Models;

namespace Group5.Shared
{
    public static class UserDatabase
    {
        public static List<BorrowForm> PendingForms { get; } = new();
        public static HashSet<string> DismissedNotificationKeys { get; } = new();

        // Notice/Announcements
        public static string Notice { get; set; } = "No new notices at this time.";

        // Subjects available
        public static readonly string[] AllSubjects = new[]
        {
            "NCP_2100", "NCP_2101", "NCP_2102",
            "NEC_1200", "NEC_1201", "NEC_1202"
        };

        // Toolroom inventory with stock quantities
        public static Dictionary<string, int> ToolRoomStock { get; set; } = new()
        {
            // Electronics Engineering Toolroom
            ["Multimeters"] = 10,
            ["Extension Cords"] = 15,
            ["Remote Control"] = 5,
            ["UniTrain Learning Module"] = 3,
            ["Generator"] = 2,
            ["Soldering Iron"] = 8,
            ["Oscilloscope"] = 4,
            
            // Electronics and Communication Engineering
            ["Capacitor"] = 50,
            ["Resistors"] = 100,
            ["LEDs"] = 200,
            ["IC"] = 30,
            ["Multimeter"] = 8,
            ["DC Supply"] = 6,
            ["AC Supply"] = 4,
            ["VGA Cables"] = 12,
            ["HDMI Cables"] = 10,
            
            // Physics Toolroom
            ["Vernier Caliper"] = 20,
            ["Meter stick"] = 25,
            ["Clamps"] = 30,
            ["Timer"] = 15,
            ["Newton's hook"] = 10,
            ["Force Table"] = 5,
            ["Pulley"] = 8,
            
            // Chemistry Toolroom
            ["Flask"] = 40,
            ["Graduated Cylinder"] = 25,
            ["Beaker"] = 50,
            ["Test Tube"] = 200,
            ["Test Tube holder"] = 30,
            ["Test tube Rack"] = 15,
            ["Wire Gauze"] = 20,
            ["Litmus Paper"] = 100,
            ["Balance Scale"] = 6,
            ["Stirring Rods"] = 40,
            ["Alcohol Lamp"] = 12,
            
            // Civil Engineering Toolroom
            ["Pins"] = 100,
            ["Tripod"] = 8,
            ["50m Tape"] = 5,
            ["Total Station"] = 2,
            ["Prism"] = 4,
            ["Leveling Staff"] = 6,
            ["Slump Cone"] = 3,
            ["Inclinometer"] = 2,
            ["Metal Float"] = 5,
            ["Spirit Level"] = 10
        };

        // Toolrooms and their items
        public static readonly Dictionary<string, string[]> ToolRooms = new()
        {
            ["ELECTRONICS ENGINEERING TOOLROOM"] = new[]
            {
                "MULTIMETER", "OSCILLOSCOPE", "BREADBOARD", "FUNCTION GENERATOR",
                "POWER SUPPLY", "SOLDERING IRON", "RESISTOR", "CAPACITOR",
                "INDUCTOR", "DIODE", "TRANSISTOR", "RELAY", "IC CHIP",
                "LED", "SWITCH", "JUMPER WIRES", "PROTOBOARD"
            },
            ["ELECTRICAL ENGINEERING TOOLROOM"] = new[]
            {
                "MULTIMETER", "OSCILLOSCOPE", "POWER SUPPLY", "FUNCTION GENERATOR",
                "CLAMP METER", "INSULATION TESTER", "WIRE STRIPPER", "CRIMPING TOOL"
            },
            ["CIVIL ENGINEERING TOOLROOM"] = new[]
            {
                "Pins", "Tripod", "50m Tape", "Total Station", "Prism",
                "Leveling Staff", "Slump Cone", "Inclinometer", "Metal Float", "Spirit Level"
            },
            ["CHEMISTRY LABORATORY"] = new[]
            {
                "Flask", "Graduated Cylinder", "Beaker", "Test Tube",
                "Test Tube holder", "Test tube Rack", "Wire Gauze",
                "Litmus Paper", "Balance Scale", "Stirring Rods", "Alcohol Lamp"
            },
            ["PHYSICS LABORATORY"] = new[]
            {
                "Vernier Caliper", "Meter stick", "Clamps", "Timer",
                "Newton's hook", "Force Table", "Pulley"
            }
        };

        // Notification tracking
        public static string GetDismissKey(string userEmail, int formId)
            => $"{userEmail}:{formId}";

        public static bool IsDismissed(string userEmail, int formId)
            => DismissedNotificationKeys.Contains(GetDismissKey(userEmail, formId));

        public static void Dismiss(string userEmail, int formId)
            => DismissedNotificationKeys.Add(GetDismissKey(userEmail, formId));
    }
}

