using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using ChapterTool.Avalonia.ViewModels;
using ChapterTool.Core.Services;
using ChapterTool.Infrastructure.Configuration;

namespace ChapterTool.Avalonia.Services;

public sealed class AvaloniaWindowService(ISettingsStore<ThemeColorSettings>? themeSettingsStore = null) : IWindowService
{
    private readonly Dictionary<string, Window> windows = new(StringComparer.OrdinalIgnoreCase);

    public ValueTask ShowAsync(string windowId, object? parameter, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (windows.TryGetValue(windowId, out var existing))
        {
            Refresh(existing, windowId, parameter);
            existing.Activate();
            return ValueTask.CompletedTask;
        }

        var window = new Window
        {
            Title = Title(windowId),
            Width = 620,
            Height = 460,
            MinWidth = 420,
            MinHeight = 280
        };
        Refresh(window, windowId, parameter);
        window.Closed += (_, _) => windows.Remove(windowId);
        windows[windowId] = window;
        window.Show();
        return ValueTask.CompletedTask;
    }

    public ValueTask HideAsync(string windowId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (windows.Remove(windowId, out var window))
        {
            window.Close();
        }

        return ValueTask.CompletedTask;
    }

    private void Refresh(Window window, string id, object? parameter)
    {
        window.Title = Title(id);
        window.Content = parameter is MainWindowViewModel viewModel
            ? CreateContent(window, id, viewModel)
            : Placeholder(Title(id));
    }

    private Control CreateContent(Window window, string id, MainWindowViewModel viewModel) =>
        id switch
        {
            "preview" => PreviewContent(window, viewModel),
            "log" => LogContent(window, viewModel),
            "color-settings" => ColorContent(),
            "language" => LanguageContent(viewModel),
            "expression" => ExpressionContent(viewModel),
            "template-names" => TemplateContent(viewModel),
            "file-association" => Placeholder("File association registration is platform-gated and not enabled in this build."),
            "zones" => ZonesContent(window, viewModel),
            "forward-shift" => ForwardShiftContent(viewModel),
            _ => Placeholder(Title(id))
        };

    private static Control PreviewContent(Window window, MainWindowViewModel viewModel)
    {
        var textBox = ReadOnlyTextBox(viewModel.BuildPreview());
        var refresh = Button("Refresh");
        var copy = Button("Copy");
        refresh.Click += (_, _) => textBox.Text = viewModel.BuildPreview();
        copy.Click += async (_, _) =>
        {
            if (window.Clipboard is not null)
            {
                await window.Clipboard.SetTextAsync(textBox.Text ?? string.Empty);
            }
        };

        return WithToolbar(textBox, refresh, copy);
    }

    private static Control LogContent(Window window, MainWindowViewModel viewModel)
    {
        var textBox = ReadOnlyTextBox(viewModel.LogText());
        var refresh = Button("Refresh");
        var copy = Button("Copy");
        var clear = Button("Clear");
        refresh.Click += (_, _) => textBox.Text = viewModel.LogText();
        copy.Click += async (_, _) =>
        {
            if (window.Clipboard is not null)
            {
                await window.Clipboard.SetTextAsync(textBox.Text ?? string.Empty);
            }
        };
        clear.Click += (_, _) =>
        {
            viewModel.ClearLog();
            textBox.Text = string.Empty;
        };

        return WithToolbar(textBox, refresh, copy, clear);
    }

    private Control ColorContent()
    {
        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = "Legacy color slots", FontSize = 18 });
        var defaults = ThemeColorSettings.Default.OrderedSlots.ToArray();
        var boxes = new List<TextBox>(defaults.Length);
        foreach (var slot in defaults)
        {
            var box = new TextBox { PlaceholderText = slot.Name, Text = slot.Value };
            boxes.Add(box);
            panel.Children.Add(box);
        }

        if (themeSettingsStore is not null)
        {
            _ = LoadThemeIntoBoxesAsync(themeSettingsStore, boxes);
        }

        var save = Button("Save");
        save.Click += async (_, _) =>
        {
            if (themeSettingsStore is null || boxes.Count < 6)
            {
                return;
            }

            await themeSettingsStore.SaveAsync(
                new ThemeColorSettings(
                    NormalizeColor(boxes[0].Text, defaults[0].Value),
                    NormalizeColor(boxes[1].Text, defaults[1].Value),
                    NormalizeColor(boxes[2].Text, defaults[2].Value),
                    NormalizeColor(boxes[3].Text, defaults[3].Value),
                    NormalizeColor(boxes[4].Text, defaults[4].Value),
                    NormalizeColor(boxes[5].Text, defaults[5].Value)),
                CancellationToken.None);
        };
        panel.Children.Add(save);
        return panel;
    }

    private Control LanguageContent(MainWindowViewModel viewModel)
    {
        var combo = new ComboBox
        {
            ItemsSource = new[] { "zh-Hant", "en-US" },
            SelectedIndex = string.Equals(viewModel.UiLanguage, "en-US", StringComparison.OrdinalIgnoreCase) ? 1 : 0
        };
        var apply = Button("Apply");
        apply.Click += async (_, _) =>
        {
            var language = combo.SelectedItem?.ToString() == "en-US" ? "en-US" : "";
            await viewModel.SaveUiLanguageAsync(language, CancellationToken.None);
        };
        return LabeledPanel("Language", combo, apply);
    }

    private static Control ExpressionContent(MainWindowViewModel viewModel)
    {
        var box = new TextBox { Text = viewModel.Expression, PlaceholderText = "Expression, e.g. t + 1" };
        var enable = new CheckBox { Content = "Apply expression", IsChecked = viewModel.ApplyExpression };
        var apply = Button("Apply");
        apply.Click += (_, _) =>
        {
            viewModel.Expression = string.IsNullOrWhiteSpace(box.Text) ? "t" : box.Text;
            viewModel.ApplyExpression = enable.IsChecked == true;
        };
        return LabeledPanel("Expression", box, enable, apply);
    }

    private static Control TemplateContent(MainWindowViewModel viewModel)
    {
        var auto = new CheckBox { Content = "Auto-generate missing names", IsChecked = viewModel.AutoGenerateNames };
        var template = new CheckBox { Content = "Use template names", IsChecked = viewModel.UseTemplateNames };
        var apply = Button("Apply");
        apply.Click += (_, _) =>
        {
            viewModel.AutoGenerateNames = auto.IsChecked == true;
            viewModel.UseTemplateNames = template.IsChecked == true;
        };
        return LabeledPanel("Template Names", auto, template, apply);
    }

    private static Control ZonesContent(Window window, MainWindowViewModel viewModel)
    {
        var textBox = ReadOnlyTextBox(viewModel.CreateZonesText());
        var refresh = Button("Refresh");
        var copy = Button("Copy");
        refresh.Click += (_, _) => textBox.Text = viewModel.CreateZonesText();
        copy.Click += async (_, _) =>
        {
            if (window.Clipboard is not null)
            {
                await window.Clipboard.SetTextAsync(textBox.Text ?? string.Empty);
            }
        };

        return WithToolbar(textBox, refresh, copy);
    }

    private static Control ForwardShiftContent(MainWindowViewModel viewModel)
    {
        var frames = new NumericUpDown
        {
            Minimum = -1_000_000,
            Maximum = 1_000_000,
            Value = 0,
            Increment = 1
        };
        var apply = Button("Apply");
        apply.Click += (_, _) => viewModel.ShiftFramesForward((int)(frames.Value ?? 0));
        return LabeledPanel("Forward Translation", frames, apply);
    }

    private static Control WithToolbar(Control content, params Button[] buttons)
    {
        var root = new DockPanel { Margin = new Thickness(12) };
        var toolbar = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 8)
        };
        foreach (var button in buttons)
        {
            toolbar.Children.Add(button);
        }

        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);
        root.Children.Add(content is TextBox textBox
            ? new ScrollViewer { Content = textBox, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
            : content);
        return root;
    }

    private static Control LabeledPanel(string title, params Control[] controls)
    {
        var panel = new StackPanel { Margin = new Thickness(16), Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = title, FontSize = 18 });
        foreach (var control in controls)
        {
            panel.Children.Add(control);
        }

        return panel;
    }

    private static TextBox ReadOnlyTextBox(string text) =>
        new()
        {
            Text = text,
            AcceptsReturn = true,
            TextWrapping = TextWrapping.NoWrap,
            IsReadOnly = true
        };

    private static Button Button(string text) => new() { Content = text, MinWidth = 80 };

    private static string NormalizeColor(string? value, string fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var text = value.Trim();
        if (text.Length == 7 && text[0] == '#' && text.Skip(1).All(Uri.IsHexDigit))
        {
            return text.ToUpperInvariant();
        }

        return fallback;
    }

    private static async Task LoadThemeIntoBoxesAsync(ISettingsStore<ThemeColorSettings> store, IReadOnlyList<TextBox> boxes)
    {
        var settings = await store.LoadAsync(CancellationToken.None);
        var values = settings.OrderedSlots.Select(static slot => slot.Value).ToArray();
        for (var index = 0; index < boxes.Count && index < values.Length; index++)
        {
            boxes[index].Text = values[index];
        }
    }

    private static Control Placeholder(string text) =>
        new TextBlock
        {
            Margin = new Thickness(20),
            Text = text,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 16
        };

    private static string Title(string id) => id switch
    {
        "preview" => "Preview",
        "log" => "Log",
        "color-settings" => "Color Settings",
        "language" => "Language",
        "expression" => "Expression",
        "template-names" => "Template Names",
        "file-association" => "File Association",
        "zones" => "Zones",
        "forward-shift" => "Forward Shift",
        _ => id
    };
}
