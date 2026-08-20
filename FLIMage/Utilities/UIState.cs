using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace Utilites
{
    public static class UiState
    {
        //Save location: %AppData%\YourApp\FormName.ui.json
        private static string GetPath(Form f)
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Application.ProductName);
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, $"{f.Name}.ui.json");
        }

        public static void Save(Form form)
        {
            var bag = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

            //Form size
            if (form.WindowState == FormWindowState.Normal)
            {
                bag[$"{form.Name}.Bounds"] = new[] { form.Left, form.Top, form.Width, form.Height };
            }
            bag[$"{form.Name}.WindowState"] = (int)form.WindowState;

            //Scan controls
            WalkControls(form, bag, form.Name);

            //ToolStrip and menus
            foreach (var ts in form.Controls.OfType<MenuStrip>())
                WalkToolStrip(ts, bag, form.Name);
            foreach (var ts in form.Controls.OfType<ToolStrip>())
                WalkToolStrip(ts, bag, form.Name);

            File.WriteAllText(GetPath(form),
                JsonSerializer.Serialize(bag, new JsonSerializerOptions { WriteIndented = true }));
        }

        public static void Load(Form form)
        {
            var path = GetPath(form);
            if (!File.Exists(path)) return;

            var bag = JsonSerializer.Deserialize<Dictionary<string, object>>(File.ReadAllText(path));

            // フォーム
            if (bag.TryGetValue($"{form.Name}.Bounds", out var bObj) && bObj is JsonElement je && je.ValueKind == JsonValueKind.Array)
            {
                try
                {
                    var arr = je.EnumerateArray().Select(x => x.GetInt32()).ToArray();
                    form.StartPosition = FormStartPosition.Manual;
                    form.SetBounds(arr[0], arr[1], arr[2], arr[3]);
                }
                catch { /* Bprlem */ }
            }
            if (bag.TryGetValue($"{form.Name}.WindowState", out var wsObj))
            {
                try { form.WindowState = (FormWindowState)Convert.ToInt32(wsObj.ToString()); } catch { }
            }

            //Controls
            ApplyControls(form, bag, form.Name);

            //Menu and Strips
            foreach (var ts in form.Controls.OfType<MenuStrip>())
                ApplyToolStrip(ts, bag, form.Name);
            foreach (var ts in form.Controls.OfType<ToolStrip>())
                ApplyToolStrip(ts, bag, form.Name);
        }

        private static void WalkControls(Control root, IDictionary<string, object> bag, string prefix)
        {
            foreach (Control c in root.Controls)
            {
                if (string.IsNullOrEmpty(c.Name)) { WalkControls(c, bag, prefix); continue; }
                var key = $"{prefix}.{c.Name}";

                switch (c)
                {
                    case TextBox tb:
                        bag[$"{key}.Text"] = tb.Text;
                        break;
                    case CheckBox cb:
                        bag[$"{key}.Checked"] = cb.Checked;
                        break;
                    case RadioButton rb:
                        bag[$"{key}.Checked"] = rb.Checked;
                        break;
                    case ComboBox combo:
                        bag[$"{key}.SelectedIndex"] = combo.SelectedIndex;
                        bag[$"{key}.Text"] = combo.Text;
                        break;
                    case NumericUpDown nud:
                        bag[$"{key}.Value"] = nud.Value;
                        break;
                    case TrackBar tr:
                        bag[$"{key}.Value"] = tr.Value;
                        break;
                    case DateTimePicker dtp:
                        bag[$"{key}.Value"] = dtp.Value;
                        break;
                    case SplitContainer sc:
                        bag[$"{key}.SplitterDistance"] = sc.SplitterDistance;
                        break;
                        // Add more controls if necessary
                }
                // Further down to children.
                WalkControls(c, bag, $"{prefix}.{c.Name}");
            }
        }

        private static void ApplyControls(Control root, IDictionary<string, object> bag, string prefix)
        {
            foreach (Control c in root.Controls)
            {
                if (string.IsNullOrEmpty(c.Name)) { ApplyControls(c, bag, prefix); continue; }
                var key = $"{prefix}.{c.Name}";

                void Set<T>(string suffix, Action<T> apply, Func<object, T> conv)
                {
                    if (bag.TryGetValue($"{key}.{suffix}", out var v))
                    {
                        try
                        {
                            if (v is JsonElement je) apply(conv(je.Deserialize<T>()));
                            else apply(conv(v));
                        }
                        catch { /* Broken */ }
                    }
                }

                switch (c)
                {
                    case TextBox tb:
                        Set<string>("Text", x => tb.Text = x, x => x is string s ? s : x.ToString());
                        break;
                    case CheckBox cb:
                        Set<bool>("Checked", x => cb.Checked = x, Convert.ToBoolean);
                        break;
                    case RadioButton rb:
                        Set<bool>("Checked", x => rb.Checked = x, Convert.ToBoolean);
                        break;
                    case ComboBox combo:
                        Set<int>("SelectedIndex", x => combo.SelectedIndex = x, Convert.ToInt32);
                        Set<string>("Text", x => combo.Text = x, x => x is string s ? s : x.ToString());
                        break;
                    case NumericUpDown nud:
                        Set<decimal>("Value", x => nud.Value = x, Convert.ToDecimal);
                        break;
                    case TrackBar tr:
                        Set<int>("Value", x => tr.Value = x, Convert.ToInt32);
                        break;
                    case DateTimePicker dtp:
                        Set<DateTime>("Value", x => dtp.Value = x, y => (DateTime)(object)y);
                        break;
                    case SplitContainer sc:
                        Set<int>("SplitterDistance", x => sc.SplitterDistance = x, Convert.ToInt32);
                        break;
                }
                ApplyControls(c, bag, $"{prefix}.{c.Name}");
            }
        }

        private static void WalkToolStrip(ToolStrip ts, IDictionary<string, object> bag, string prefix)
        {
            foreach (ToolStripItem item in ts.Items)
                WalkToolStripItem(item, bag, $"{prefix}.{ts.Name}");
        }

        private static void WalkToolStripItem(ToolStripItem item, IDictionary<string, object> bag, string prefix)
        {
            if (item == null || string.IsNullOrEmpty(item.Name)) return;
            var key = $"{prefix}.{item.Name}";
            if (item is ToolStripMenuItem mi)
            {
                bag[$"{key}.Checked"] = mi.Checked;
                foreach (ToolStripItem child in mi.DropDownItems)
                    WalkToolStripItem(child, bag, $"{prefix}.{mi.Name}");
            }
        }

        private static void ApplyToolStrip(ToolStrip ts, IDictionary<string, object> bag, string prefix)
        {
            foreach (ToolStripItem item in ts.Items)
                ApplyToolStripItem(item, bag, $"{prefix}.{ts.Name}");
        }

        private static void ApplyToolStripItem(ToolStripItem item, IDictionary<string, object> bag, string prefix)
        {
            if (item == null || string.IsNullOrEmpty(item.Name)) return;
            var key = $"{prefix}.{item.Name}";
            if (item is ToolStripMenuItem mi)
            {
                if (bag.TryGetValue($"{key}.Checked", out var v))
                {
                    try
                    {
                        mi.Checked = v is JsonElement je ? je.GetBoolean() : Convert.ToBoolean(v);
                    }
                    catch { }
                }
                foreach (ToolStripItem child in mi.DropDownItems)
                    ApplyToolStripItem(child, bag, $"{prefix}.{mi.Name}");
            }
        }
    }
}