using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Wordsmith.Data;
using Wordsmith.Enums;

namespace Wordsmith.Gui;

public sealed class SettingsUI : Window, IReflected
{
    private const int FOOTERHEIGHT = 100;

    // Thesaurus settings.
    private int _searchHistoryCountChange = Wordsmith.Configuration.SearchHistoryCount;
    private bool _researchToTopChange = Wordsmith.Configuration.ResearchToTop;

    // Scratch Pad settings.
    private bool _contextMenu = Wordsmith.Configuration.AddContextMenuOption;
    private bool _deleteClosed = Wordsmith.Configuration.DeleteClosedScratchPads;
    private bool _ignoreHypen = Wordsmith.Configuration.IgnoreWordsEndingInHyphen;
    private bool _showChunks = Wordsmith.Configuration.ShowTextInChunks;
    private bool _onSentence = Wordsmith.Configuration.SplitTextOnSentence;
    private bool _detectHeader = Wordsmith.Configuration.ParseHeaderInput;
    private string _oocOpening = Wordsmith.Configuration.OocOpeningTag;
    private string _oocClosing = Wordsmith.Configuration.OocClosingTag;
    private string _sentenceTerminators = Wordsmith.Configuration.SentenceTerminators;
    private string _encapTerminators = Wordsmith.Configuration.EncapsulationTerminators;
    private string _continueMarker = Wordsmith.Configuration.ContinuationMarker;
    private bool _markLastChunk = Wordsmith.Configuration.ContinuationMarkerOnLast;
    private bool _autoClear = Wordsmith.Configuration.AutomaticallyClearAfterLastCopy;
    private int _scratchMaxTextLen = Wordsmith.Configuration.ScratchPadMaximumTextLength;
    private int _scratchEnter = (int)Wordsmith.Configuration.ScratchPadTextEnterBehavior;

    // Alias Settings
    private int _newAliasSelection = 0;
    private string _newAlias = "";
    private string _newAliasTarget = "";
    private List<(int ChatType, string Alias, object? data)> _headerAliases = new(Wordsmith.Configuration.HeaderAliases);

    // Spellcheck Settings
    private bool _fixDoubleSpace = Wordsmith.Configuration.ReplaceDoubleSpaces;
    private int _maxSuggestions = Wordsmith.Configuration.MaximumSuggestions;
    private string _dictionaryFilename = Wordsmith.Configuration.DictionaryFile;

    // Linkshell Settings
    private string[] _cwlinkshells = Wordsmith.Configuration.CrossWorldLinkshellNames;
    private string[] _linkshells = Wordsmith.Configuration.LinkshellNames;

    // Colors Settings
    private Vector4 _backupColor = new();
    private bool _enableTextColor = Wordsmith.Configuration.EnableTextHighlighting;
    private Vector4 _spellingErrorColor = Wordsmith.Configuration.SpellingErrorHighlightColor;
    private Dictionary<int, Vector4> _headerColors = Wordsmith.Configuration.HeaderColors;

    internal string GetDebugString()
    {
        string result = "Settings UI:";
        result += $"Thesaurus Settings\n";
        result += $"\tSearch History Limit: {this._searchHistoryCountChange}\n";
        result += $"\tMove Re-searches to top: {this._researchToTopChange}\n\n";

        result += $"Scratch Pad Settings:\n";
        result += $"\tContext Menu Enabled: {this._contextMenu}\n";
        result += $"\tDelete Closed Pads: {this._deleteClosed}\n";
        result += $"\tIgnore Hyphen Terminated: {this._ignoreHypen}\n";
        result += $"\tShow Text in Chunks: {this._showChunks}\n";
        result += $"\tBreak on Sentence: {this._onSentence}\n";
        result += $"\tParse Header: {this._detectHeader}\n";
        result += $"\tOOC Opening: {this._oocOpening}\n";
        result += $"\tOOC Closing: {this._oocClosing}\n";
        result += $"\tSentence Terminators: {this._sentenceTerminators}\n";
        result += $"\tEncap Terminators: {this._encapTerminators}\n";
        result += $"\tContinue Marker: {this._continueMarker}\n";
        result += $"\tMark Last Chunk: {this._markLastChunk}\n";
        result += $"\tAuto Clear Pads: {this._autoClear}\n";
        result += $"\tMax Text Length: {this._scratchMaxTextLen}\n";
        result += $"\tCtrl+Enter Action: {this._scratchEnter}\n\n";

        result += $"Alias Settings:\n";
        result += $"\tNew Alias: {this._newAlias}\n";
        result += $"\tNew Alias Selection: {this._newAliasSelection}\n";
        result += $"\tAliases:\n";
        result += $"\t]\n";
        foreach ( (int i, string s, object? o) in this._headerAliases )
            result += $"\t\t{i}: {s}, {(o is null ? "NULL" : o)}\n";

        result += $"\t]\n\n";

        result += $"Spellcheck Settings:\n";
        result += $"\tFix Double Spaces: {this._fixDoubleSpace}\n";
        result += $"\tMax Suggestions: {this._maxSuggestions}\n";
        result += $"\tDictionary Filename: {this._dictionaryFilename}\n\n";

        result += $"Linkshell Settings:\n";
        result += $"\tLinkshell Names:\n\t[\n";
        result += $"\t\t{string.Join( "\n\t\t", this._linkshells )}";
        result += $"\t]\n";
        result += $"\tCross-World Linkshell Names:\n[\n";
        result += $"\t\t{string.Join( "\n\t\t", this._cwlinkshells )}";
        result += $"\t]\n\n";

        result += $"Color Settings:\n";
        result += $"\tBackup Color: {this._backupColor}";
        result += $"\tEnable Text Color: {this._enableTextColor}";
        result += $"\tSpelling Error Color: {this._spellingErrorColor}";
        result += $"\tHeader Colors:\n\t[\n";
        foreach ( KeyValuePair<int, Vector4> kv in this._headerColors )
            result += $"\t\t<{kv.Value.X}, {kv.Value.Y}, {kv.Value.Z}, {kv.Value.W}>";
        result += $"\t]";
        return result;
    }

    public SettingsUI() : base($"{Wordsmith.AppName} - Settings")
    {
        this._searchHistoryCountChange = Wordsmith.Configuration.SearchHistoryCount;
        this._researchToTopChange = Wordsmith.Configuration.ResearchToTop;
        //Size = new(375, 350);
        this.SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = ImGuiHelpers.ScaledVector2(800, 450),
            MaximumSize = ImGuiHelpers.ScaledVector2(9999, 9999)
        };

        this.Flags |= ImGuiWindowFlags.NoScrollbar;
        this.Flags |= ImGuiWindowFlags.NoScrollWithMouse;
    }

    public override void Update()
    {
        base.Update();

        if (!this.IsOpen)
            WordsmithUI.RemoveWindow(this);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("SettingsUITabBar"))
        {
            DrawThesaurusTab();
            DrawScratchPadTab();
            DrawAliasesTab();
            DrawSpellCheckTab();
            DrawLinkshellTab();
            DrawColorsTab();
            ImGui.EndTabBar();
        }

        ImGui.Separator();
        DrawFooter();
    }

    private void DrawThesaurusTab()
    {
        if (ImGui.BeginTabItem("Thesaurus##SettingsUITabItem"))
        {
            if (ImGui.BeginChild("ThesaurusSettingsChildFrame", new(-1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale)))
            {
                //Search history count
                //ImGui.DragInt("Search History Size", ref _searchHistoryCountChange, 0.1f, 1, 50);
                ImGui.InputInt("History Size", ref this._searchHistoryCountChange, 1, 5);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("This is the number of searches to keep in memory at one time.\nNote: The more you keep, them more memory used.");
                if (this._searchHistoryCountChange < 1)
                    this._searchHistoryCountChange = 1;
                if (this._searchHistoryCountChange > 50)
                    this._searchHistoryCountChange = 50;

                //Re-search to top
                ImGui.Checkbox("Move repeated search to top of history.", ref this._researchToTopChange);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("If enabled, when searching for a word you've searched\nalready, it will move it to the top of the list.");

                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawScratchPadTab()
    {
        if (ImGui.BeginTabItem("Scratch Pad##SettingsUITabItem"))
        {
            if (ImGui.BeginChild("SettingsUIScratchPadChildFrame", new(-1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale)))
            {
                if (ImGui.BeginTable("SettingsUiScratchPadChildTable", 2, ImGuiTableFlags.BordersH | ImGuiTableFlags.BordersInnerV))
                {
                    ImGui.TableSetupColumn("SettingsUiScratchPadChildTableLeftColumn");
                    ImGui.TableSetupColumn("SettingsUiScratchPadChildTableRightColumn");

                    ImGui.TableNextColumn();
                    // Add to context menu.
                    ImGui.Checkbox("Add to context menu.", ref this._contextMenu);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("If enabled the option \"Tell in Scratch\" will be added to\ncontext menus that have \"Send Tell\".");

                    ImGui.TableNextColumn();
                    // Auto-Clear Scratch Pad
                    ImGui.Checkbox("Auto-clear Scratch Pad", ref this._autoClear);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Automatically clears text from scratch pad after copying last chunk.");

                    ImGui.TableNextColumn();
                    // Auto Delete Scratch Pads
                    ImGui.Checkbox("Auto-Delete Scratch Pads On Close##SettingsUICheckbox", ref this._deleteClosed);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("When enabled it will delete the scratch pad on close.\nWhen disabled you will have a delete button at the bottom.");

                    ImGui.TableNextColumn();
                    // Show text in chunks.
                    ImGui.Checkbox("Show Text In Chunks##SettingsUICheckbox", ref this._showChunks);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("When enabled it will display a large box with text above your entry form.\nThis box will show you how the text will be broken into chunks.\nIn single-line input mode this text will always show but without chunking.");

                    ImGui.TableNextColumn();
                    // Split on sentence
                    ImGui.Checkbox("Split Text On Sentence##SettingsUICheckbox", ref this._onSentence);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("When enabled, Scratch Pad attempts to do chunk breaks at the end of sentences instead\nof between any words.");

                    // TODO Add user control to disable automatic header parsing.
                    ImGui.TableNextColumn();
                    ImGui.Checkbox("Parse Header From Text##SettingsUICheckbox", ref this._detectHeader);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("When enabled, typing a header into the input text of a scratch pad will cause\nthe scratchpad to try to parse the desired header automatically.");

                    ImGui.TableNextColumn();
                    // OOC Tags.
                    ImGui.SetNextItemWidth(50 * ImGuiHelpers.GlobalScale);
                    ImGui.InputText("##OocOpeningTagInputText", ref this._oocOpening, 5);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("The opening tag for your OOC text.");

                    ImGui.SameLine(0, 2);
                    ImGui.Text("OOC Tags");

                    ImGui.SameLine(0, 2);
                    ImGui.SetNextItemWidth(50 * ImGuiHelpers.GlobalScale);
                    ImGui.InputText("##OocClosingTagInputText", ref this._oocClosing, 5);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("The closing tag for your OOC text.");

                    ImGui.TableNextColumn();
                    // Enter Key Behavior
                    ImGui.SetNextItemWidth(150 * ImGuiHelpers.GlobalScale);

                    // Get all of the enum options.
                    string[] enterKeyActions = Enum.GetNames(typeof(Enums.EnterKeyAction));

                    // Add a space in front of all capital letters.
                    for (int i = 0; i < enterKeyActions.Length; ++i)
                        enterKeyActions[i] = enterKeyActions[i].SpaceByCaps();

                    ImGui.Combo("Ctrl+Enter Key Behavior##SettingsUI", ref this._scratchEnter, enterKeyActions, enterKeyActions.Length);//new string[] { "Do nothing", "Spell Check", "Copy" }, 3);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Defines what action to take when the user hits enter in the text entry.");

                    float width = ImGui.GetColumnWidth() - (175*ImGuiHelpers.GlobalScale);

                    ImGui.TableNextColumn();
                    // Sentence terminators
                    ImGui.SetNextItemWidth(width);
                    ImGui.InputText("Sentence Terminators##ScratchPadSplitCharsText", ref this._sentenceTerminators, 32);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip("Each of these characters can mark the end of a sentence when followed by a space of encapsulator.\ni.e. \"A.B\" is not a sentence terminator but \"A. B\" is.");


                    ImGui.TableNextColumn();
                    // Encapsulation terminators
                    ImGui.SetNextItemWidth(width);
                    ImGui.InputText($"Encpasulation Terminators##ScratchPadEncapCharsText", ref this._encapTerminators, 32);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"Each of these characters ends an encapsulator.\nThis is used with sentence terminators in case of encapsulation for chunk breaks\ni.e. \"A) B\" will not count but \"A.) B\" will.");


                    ImGui.TableNextColumn();
                    // Continuation marker
                    ImGui.SetNextItemWidth(width);
                    ImGui.InputText($"Continuation Marker##ScratchPadEncapCharsText", ref this._continueMarker, 32);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"This is what is appended to the end of your text chunks to notify\nreaders that it isn't finished yet. #c will be replaced with current number and #m will be max number\nSo if you put: (#c/#m) it will say something like.(1/3)");


                    ImGui.TableNextColumn();
                    // Mark last chunk
                    ImGui.SetNextItemWidth(width);
                    ImGui.Checkbox($"Continuation Mark On Last Chunk##ScratchPadEncapCharsText", ref this._markLastChunk);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"This is useful if your continuation marker uses the #c and/or #m\ni.e. (#c/#m) will put (3/3) on last chunk.");


                    ImGui.EndTable();
                }
                // Max Text Length
                float dragWidth = ImGui.GetWindowContentRegionWidth() - (125 * ImGuiHelpers.GlobalScale);
                ImGui.SetNextItemWidth(dragWidth);
                ImGui.SliderInt("Max Text Length##CratchPadSettingsSlider", ref this._scratchMaxTextLen, 512, 8192);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("This is the buffer size for text input. The higher this value is the more\nmemory consumed up to a maximum of 8MB per Scratch Pad.");

                ImGui.Separator();
                ImGui.Text("Open Scratch Pads.");
                if (ImGui.BeginTable($"SettingsPadListTable", 3))
                {
                    ImGui.TableSetupColumn("SettingsUIPadListIDColumn", ImGuiTableColumnFlags.WidthFixed, 25 * ImGuiHelpers.GlobalScale);
                    ImGui.TableSetupColumn("SettingsUIPadListDescColumn", ImGuiTableColumnFlags.WidthStretch, 1);
                    ImGui.TableSetupColumn("SettingsUIPadListCloseColumn", ImGuiTableColumnFlags.WidthFixed, 75 * ImGuiHelpers.GlobalScale);

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("ID##SettingsUIPadListColumnHeader");
                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Chat Header##SettingsUIPadListColumnHeader.");
                    ImGui.TableNextColumn();
                    ImGui.TableHeader("##SettingsUIPadListColumnHeader");


                    foreach(Window w in WordsmithUI.Windows)
                    {
                        if (w is ScratchPadUI pad)
                        {
                            ImGui.TableNextColumn();
                            ImGui.Text(pad.ID.ToString());

                            ImGui.TableNextColumn();
                            ImGui.Text(pad.GetFullChatHeader().Length > 0 ? pad.GetFullChatHeader() : "None");

                            ImGui.TableNextColumn();
                            if (ImGui.Button($"Close##SettingsUIPadListClose{pad.ID}", new(-1, 25 * ImGuiHelpers.GlobalScale)))
                                pad.OnClose();
                        }
                    }
                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawAliasesTab()
    {
        if ( ImGui.BeginTabItem( "Aliases##SettingsUITabItem" ) )
        {
            if ( ImGui.BeginChild( "AliasSettingsChild", new( -1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale ) ) )
            {
                //
                // Aliases
                //
                ImGui.TextWrapped( $"Aliases are like nicknames for chat commands. For example if you set the alias \"me\" for Emote you can type /me instead of /em and the correct chat channel will be parsed." );
                int spacing = 10;
                int tellBarWidth = 200;

                if ( ImGui.BeginTable( "AliasesTabTable", 3, ImGuiTableFlags.BordersH ) )
                {
                    ImGui.TableSetupColumn( "AliasChatTypeHeaderColumn", ImGuiTableColumnFlags.WidthFixed, 150*ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "AliasTextInputColumn", ImGuiTableColumnFlags.WidthStretch );
                    ImGui.TableSetupColumn( "AliasAddDeleteColumn", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize, 30*ImGuiHelpers.GlobalScale );

                    // Get the chat type options.
                    List<string> options = new(Enum.GetNames(typeof(Enums.ChatType)));

                    options[0] = "Please Choose";
                    options.Remove( "Linkshell" );
                    options.Remove( "CrossWorldLinkshell" );

                    for ( int toggle = 0; toggle < 2; ++toggle )
                        for ( int i = 1; i <= 8; ++i )
                            options.Add( $"{(toggle == 1 ? "CW-" : "")}Linkshell{i}" );

                    for (int i = 0; i < this._headerAliases.Count; ++i)
                    {
                        (int ChatType, string Alias, object? Data) alias = this._headerAliases[i];

                        string chatTypeName = options[alias.ChatType];

                        // Draw the chat type.
                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth( -1 );
                        ImGui.Spacing();
                        ImGui.Text( $"{chatTypeName}" );
                        if ( ImGui.IsItemHovered() )
                            ImGui.SetTooltip( "Chat header to use alias on." );

                        string output = alias.Alias;
                        // Add the input field.
                        ImGui.TableNextColumn();

                        bool update = false;
                        ImGui.Spacing();
                        if ( chatTypeName == "Tell" && alias.Data is not null)
                        {
                            // Get the data as a string.
                            string data = alias.Data as string ?? "";

                            // Set the width of the target entry.
                            ImGui.SetNextItemWidth( tellBarWidth * ImGuiHelpers.GlobalScale );
                            if (ImGui.InputTextWithHint( $"##{chatTypeName}Alias{alias}TargetTextEdit", $"User Name@World", ref data, 128, ImGuiInputTextFlags.EnterReturnsTrue))
                                update = data != (alias.Data as string) && data.GetTarget() is not null;
                            if ( ImGui.IsItemHovered() )
                                ImGui.SetTooltip( "Edt the target of your tell. Hit the enter key after maing changes to update.\nYou must still click Apply to save the changes." );

                            ImGui.SameLine( 0, 0 );
                            ImGui.Spacing();
                            // Same line.
                            ImGui.SameLine( 0, spacing * ImGuiHelpers.GlobalScale );

                            // Draw the alias input. If the user hits enter, set update flag if alias has changed.
                            // Get the size of the column.
                            float size = ImGui.GetColumnWidth();
                            ImGui.SetNextItemWidth( size );
                        }
                        else
                            ImGui.SetNextItemWidth( -1 );

                        update |= ImGui.InputText( $"##{chatTypeName}Alias{alias}Input", ref output, 128, ImGuiInputTextFlags.EnterReturnsTrue );
                        if ( ImGui.IsItemHovered() )
                            ImGui.SetTooltip( "Edit the alias here. Hit the enter key after making changes to update.\nYou must still click Apply to save the changes." );

                        // If update flag is enabled then update the item.
                        if ( update )
                            this._headerAliases[i] = new( alias.ChatType, output.ToLower().Trim( '\'', '/', ' ', '\r', '\n' ), alias.Data );

                        ImGui.TableNextColumn();
                        if ( ImGui.Button( $"X##{chatTypeName}Alias{alias}", ImGuiHelpers.ScaledVector2( 25, 25 ) ))
                            this._headerAliases.RemoveAt( i-- );
                    }


                    // Display the chat type selection.
                    ImGui.TableNextColumn();
                    ImGui.Spacing();
                    ImGui.SetNextItemWidth( -1 );
                    ImGui.Combo( "##NewAliasChatTypeSelection", ref this._newAliasSelection, options.ToArray(), options.Count );
                    if ( ImGui.IsItemHovered() )
                        ImGui.SetTooltip( "Chat header to use alias on." );
                    // Show an input field for the new alias.
                    ImGui.TableNextColumn();
                    ImGui.Spacing();

                    bool add = false;
                    if ( options[this._newAliasSelection] == "Tell" )
                    {
                        // Set the size of the tell target.
                        ImGui.SetNextItemWidth( tellBarWidth * ImGuiHelpers.GlobalScale );

                        // Display the target entry.
                        ImGui.InputTextWithHint( $"##NewAliasTargetTextInput", $"User Name@World", ref this._newAliasTarget, 128 );
                        if ( ImGui.IsItemHovered() )
                            ImGui.SetTooltip( "This is the target of your tell. i.e. \"<t>\" or \"User Name@World\"" );

                        // Insert the spacing.
                        ImGui.SameLine( 0, spacing * ImGuiHelpers.GlobalScale );

                        // Get the size of alias text area.
                        float size = ImGui.GetColumnWidth();
                        ImGui.SetNextItemWidth( size );
                    }
                    else
                        ImGui.SetNextItemWidth( -1 );

                    add = ImGui.InputTextWithHint( $"##NewAliasTextInput", $"Enter alias here without /.", ref this._newAlias, 128, ImGuiInputTextFlags.EnterReturnsTrue );
                    if ( ImGui.IsItemHovered() )
                        ImGui.SetTooltip( "Enter the desired alias here without the \"/\" character" );

                    // Put the + button.
                    ImGui.TableNextColumn();
                    ImGui.Spacing();
                    add |= ImGui.Button( "+##NewAliasAddButton", ImGuiHelpers.ScaledVector2( 25, 25 ) );

                    if ( add && this._newAliasSelection > 0 )
                    {
                        // Create a flag to check validity.
                        bool valid = true;

                        // Check if the alias is already in use.
                        foreach ( (int i, string s, object? o) in this._headerAliases )
                        {
                            // If it is, flag it as invalid
                            if ( this._newAlias == s )
                            {
                                valid = false;
                                break;
                            }
                        }

                        // Check if the alias is used by a default option.
                        for( int i = 0; i < Enum.GetNames(typeof(ChatType)).Length; ++i )
                        {
                            ChatType ct = (ChatType)i;
                            if ( $"/{this._newAlias.ToLower()}" == ct.GetShortHeader() || $"/{this._newAlias.ToLower()}" == ct.GetLongHeader() )
                            {
                                valid = false;
                                break;
                            }
                        }

                        // If the user did not enter a valid target then flag as invalid.
                        if ( options[this._newAliasSelection] == "Tell" && !this._newAliasTarget.isTarget() )
                            valid = false;

                        // If the alias doesn't exist then add it.
                        if ( valid )
                        {
                            this._headerAliases.Add( new( this._newAliasSelection, this._newAlias.ToLower().Trim( '\'', '/', ' ', '\r', '\n' ), this._newAliasTarget.Length > 0 ? this._newAliasTarget : null ) );
                            this._newAliasSelection = 0;
                            this._newAlias = "";
                        }
                    }
                    ImGui.Spacing();

                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawSpellCheckTab()
    {
        if (ImGui.BeginTabItem("Spell Check##SettingsUITabItem"))
        {
            if (ImGui.BeginChild("DictionarySettingsChild", new(-1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale)))
            {
                // Ignore Hyphen terminated words.
                ImGui.Checkbox("Ignore Hyphen-Terminated Words##SettingsUICheckbox", ref this._ignoreHypen);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("This is useful in roleplay for emulating cut speech.\ni.e. \"How dare yo-,\" she was cut off but the rude man.");
                ImGui.SameLine();

                // Auto-Fix Spaces
                ImGui.Checkbox("Automatically Fix Multiple Spaces In Text.", ref this._fixDoubleSpace);
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip("When enabled, Scratch Pads will programmatically remove extra\nspaces from your text for you.");
                ImGui.Separator();

                ImGui.DragInt( "Maximum Suggestions", ref this._maxSuggestions, 0.1f, 0, 100 );
                if ( ImGui.IsItemHovered() )
                    ImGui.SetTooltip( "The number of spelling suggestions to return with spell checking. 0 is unlimited results." );
                ImGui.Separator();

                // Dictionary File
                // Start by getting all of the available dictionary files.
                string[] files = Directory.GetFiles(Path.Combine(Wordsmith.PluginInterface.AssemblyLocation.Directory?.FullName!, "Dictionaries"))
                    .Select(f => Path.GetFileName(f)).ToArray();

                // If no files are returned
                if (files.Length == 0)
                {
                    // Alert the user to the missing dictionaries.
                    ImGui.TextColored(new(255, 0, 0, 255), "ERROR.");
                    ImGui.TextWrapped($"There are no dictionary files in the dictionary folder {{{Path.Combine(Wordsmith.PluginInterface.AssemblyLocation.Directory?.FullName!, "Dictionaries")}}}.");
                }
                else
                {
                    // Get the index of the current dictionary file if it exists.
                    int selection = files.IndexOf(this._dictionaryFilename);

                    // If the file isn't found, default to option 0.
                    if (selection < 0)
                        selection = 0;

                    ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 160*ImGuiHelpers.GlobalScale);
                    // Display a combo with all of the available dictionaries.
                    ImGui.Combo("Dictionary Selection", ref selection, files, files.Length);
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"This is the file to be used for the dictionary. To use a custom spell check\ndictionary it must be inside the plug-in's Dictionary folder.");

                    // If the selection is different from the previous dictionary then
                    // update the filename.
                    if (selection != files.IndexOf(this._dictionaryFilename))
                        this._dictionaryFilename = files[selection];
                }
                ImGui.Separator();

                // Custom Dictionary Table
                if (ImGui.BeginTable($"CustomDictionaryEntriesTable", 2, ImGuiTableFlags.BordersH))
                {
                    ImGui.TableSetupColumn("CustomDictionaryWordColumn", ImGuiTableColumnFlags.WidthStretch, 2);
                    ImGui.TableSetupColumn("CustomDictionaryDeleteColumn", ImGuiTableColumnFlags.WidthFixed, 65 * ImGuiHelpers.GlobalScale);

                    ImGui.TableNextColumn();
                    ImGui.Text("Custom Dictionary Entries");

                    // Delete all
                    ImGui.TableNextColumn();
                    if (ImGui.Button("Delete All##DeleteAllDictionaryEntriesButton", ImGuiHelpers.ScaledVector2(-1, 25)))
                        WordsmithUI.ShowResetDictionary();
                    if (ImGui.IsItemHovered())
                        ImGui.SetTooltip($"Deletes all dictionary entries. This action cannot be undone.");

                    // Individual entries
                    for (int i = 0; i < Wordsmith.Configuration.CustomDictionaryEntries.Count; ++i)
                    {
                        ImGui.TableNextColumn();
                        ImGui.Text(Wordsmith.Configuration.CustomDictionaryEntries[i]);

                        ImGui.TableNextColumn();
                        if ( ImGui.Button( $"Delete##CustomDictionaryDelete{i}Buttom", ImGuiHelpers.ScaledVector2( -1, 25 ) ) )
                            Lang.RemoveDictionaryEntry( Wordsmith.Configuration.CustomDictionaryEntries[i--] );

                        else if (ImGui.IsItemHovered())
                            ImGui.SetTooltip($"Permanently deletes {Wordsmith.Configuration.CustomDictionaryEntries[i]} from your custom dictionary.");
                    }
                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawLinkshellTab()
    {
        if (ImGui.BeginTabItem("Linkshells##SettingsUITabItem"))
        {
            if (ImGui.BeginChild("LinkshellsSettingsChildFrame", new(-1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale)))
            {
                ImGui.Text("Linkshell Names");
                
                if (ImGui.BeginTable("LinkshellsNamesTable", 3, ImGuiTableFlags.BordersH))
                {
                    ImGui.TableSetupColumn("LinkshellRowHeaderColumn", ImGuiTableColumnFlags.WidthFixed, 10 * ImGuiHelpers.GlobalScale);
                    ImGui.TableSetupColumn("LinkshellNameColumn", ImGuiTableColumnFlags.WidthStretch, 1);
                    ImGui.TableSetupColumn("CrossworldLinkshellNameColumn", ImGuiTableColumnFlags.WidthStretch, 1);

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("#");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Linkshell");

                    ImGui.TableNextColumn();
                    ImGui.TableHeader("Cross-World");

                    // For each linkshell, create an id | custom name row.
                    for(int i = 0; i<8; ++i)
                    {
                        ImGui.TableNextColumn();
                        ImGui.Text($"{i + 1}");

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(-1);
                        ImGui.InputText($"##SettingsLinkshellName{i}", ref this._linkshells[i], 32);

                        ImGui.TableNextColumn();
                        ImGui.SetNextItemWidth(-1);
                        ImGui.InputText($"##SettingsCWLinkshellName{i}", ref this._cwlinkshells[i], 32);
                    }

                    ImGui.EndTable();
                }

                ImGui.EndChild();
            }

            ImGui.EndTabItem();
        }
    }

    private void DrawColorsTab()
    {
        if ( ImGui.BeginTabItem( "Colors##SettingsUITabItem" ) )
        {
            if ( ImGui.BeginChild( "ColorsSettingsChildFrame", new( -1, ImGui.GetWindowSize().Y - FOOTERHEIGHT * ImGuiHelpers.GlobalScale ) ) )
            {
                ImGui.Checkbox( "Enable Text Colorization##SettingsUICheckbox", ref this._enableTextColor );
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                bool spellingErrorColorPopup = false;
                if ( this._enableTextColor )
                    spellingErrorColorPopup = ImGui.ColorButton( "##ColorPreviewButton", this._spellingErrorColor );
                else
                    ImGui.ColorButton( "##ColorPreviewButtonDisabled", new( 0.2f, 0.2f, 0.2f, 0.5f ), ImGuiColorEditFlags.NoTooltip );
                if ( spellingErrorColorPopup )
                {
                    ImGui.OpenPopup( "SettingsUIErrorHighlightingColorPickerPopup" );
                    this._backupColor = this._spellingErrorColor;
                }
                if ( ImGui.BeginPopup( "SettingsUIErrorHighlightingColorPickerPopup" ) )
                {

                    if ( ImGui.ColorPicker4( "##SettingsUIErrorHighlightingPicker", ref this._backupColor ) )
                        this._spellingErrorColor = this._backupColor;

                    ImGui.EndPopup();
                }
                ImGui.SameLine( 0, 5 * ImGuiHelpers.GlobalScale );
                ImGui.Text( $"Spelling Error Color" );
                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();
                ImGui.Text( "Chat Header Colors" );

                if (ImGui.BeginTable("ColorSettingsTable", 10))
                {
                    ImGui.TableSetupColumn( "LeftColorOuterColorColumn", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "LeftColorOuterChatColumn" );
                    ImGui.TableSetupColumn( "LeftColorInnerColorColumn", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "LeftColorInnerChatColumn" );
                    ImGui.TableSetupColumn( "CenterColorColorColumn", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "CenterColorChatColumn" );
                    ImGui.TableSetupColumn( "RightColorInnerColorColumn", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "RightColorInnerChatColumn" );
                    ImGui.TableSetupColumn( "RightColorOuterColorColumn", ImGuiTableColumnFlags.WidthFixed, 30 * ImGuiHelpers.GlobalScale );
                    ImGui.TableSetupColumn( "RightColorOuterChatColumn" );

                    string[] options = Enum.GetNames(typeof(Enums.ChatType));
                    for ( int i = 0; i < options.Length-1; ++i )
                    {
                        if ( !this._headerColors.ContainsKey( i ) )
                            continue;

                        ImGui.TableNextColumn();

                        bool highlightColorPopup = false;
                        if ( this._enableTextColor )
                            highlightColorPopup = ImGui.ColorButton( $"##ColorPreviewButton{options[i]}", this._headerColors[i] );
                        else
                            ImGui.ColorButton( $"##ColorPreviewButtonDisabled{options[i]}", new( 0.2f, 0.2f, 0.2f, 0.5f ), ImGuiColorEditFlags.NoTooltip );

                        if ( highlightColorPopup )
                        {
                            ImGui.OpenPopup( $"SettingsUIHighlightingColorPickerPopup{options[i]}" );
                            this._backupColor = this._headerColors[i];
                        }
                        if ( ImGui.BeginPopup( $"SettingsUIHighlightingColorPickerPopup{options[i]}" ) )
                        {

                            if ( ImGui.ColorPicker4( "##SettingsUIErrorHighlightingPicker", ref this._backupColor ) )
                                this._headerColors[i] = this._backupColor;

                            ImGui.EndPopup();
                        }

                        ImGui.TableNextColumn();
                        ImGui.Text( $"{options[i]}" );
                    }
                    ImGui.EndTable();
                }
                ImGui.EndChild();
            }
            ImGui.EndTabItem();
        }
    }

    private void DrawFooter()
    {
        if (ImGui.BeginTable("SettingsUISaveCloseCancelButtonTable", 5))
        {
            ImGui.TableSetupColumn("SettingsUIKoFiButtonColumn", ImGuiTableColumnFlags.WidthFixed, 105 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("SettingsUITableSpacerColumn", ImGuiTableColumnFlags.WidthStretch, 1);
            ImGui.TableSetupColumn("SettingsUISaveAndCloseButtonColumn", ImGuiTableColumnFlags.WidthFixed, 70 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("SettingsUIDefaultsButtonColumn", ImGuiTableColumnFlags.WidthFixed, 70 * ImGuiHelpers.GlobalScale);
            ImGui.TableSetupColumn("SettingsUICancelButtonColumn", ImGuiTableColumnFlags.WidthFixed, 70 * ImGuiHelpers.GlobalScale);



            // Leave the first column blank for spacing.
            ImGui.TableNextColumn();
            try
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.5f, 0, 0, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.5f, 0.3f, 0.3f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.3f, 0.1f, 0.1f, 1f));
                // TODO Ko-Fi button.
                if (ImGui.Button("Buy Me A Ko-Fi##SettingsUIBuyAKoFiButton", ImGuiHelpers.ScaledVector2(-1, 25)))
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://ko-fi.com/ladydefile") { UseShellExecute = true });
            }
            finally
            {
                ImGui.PopStyleColor(3);
            }

            //Skip the next column.
            ImGui.TableNextColumn();
            ImGui.TableNextColumn();
            // Save and close buttons
            if (ImGui.Button("Apply", ImGuiHelpers.ScaledVector2(-1, 25)))
                Save();

            ImGui.TableNextColumn();
            // Reset settings to default.
            if (ImGui.Button("Defaults", ImGuiHelpers.ScaledVector2(-1, 25)))
            {
                WordsmithUI.ShowRestoreSettings();
                this.IsOpen = false;
            }

            ImGui.TableNextColumn();
            // Cancel button
            if (ImGui.Button("Close", ImGuiHelpers.ScaledVector2(-1, 25)))
                this.IsOpen = false;

            ImGui.EndTable();
        }
    }

    public override void OnOpen()
    {
        base.OnOpen();
        ResetValues();
    }

    private void ResetValues()
    {
        // Thesaurus settings.
        this._searchHistoryCountChange = Wordsmith.Configuration.SearchHistoryCount;
        this._researchToTopChange = Wordsmith.Configuration.ResearchToTop;

        // Scratch Pad settings.
        this._contextMenu = Wordsmith.Configuration.AddContextMenuOption;
        this._autoClear = Wordsmith.Configuration.AutomaticallyClearAfterLastCopy;
        this._deleteClosed = Wordsmith.Configuration.DeleteClosedScratchPads;
        this._ignoreHypen = Wordsmith.Configuration.IgnoreWordsEndingInHyphen;
        this._showChunks = Wordsmith.Configuration.ShowTextInChunks;
        this._onSentence = Wordsmith.Configuration.SplitTextOnSentence;
        this._detectHeader = Wordsmith.Configuration.ParseHeaderInput;
        this._oocOpening = Wordsmith.Configuration.OocOpeningTag;
        this._oocClosing = Wordsmith.Configuration.OocClosingTag;
        this._sentenceTerminators = Wordsmith.Configuration.SentenceTerminators;
        this._encapTerminators = Wordsmith.Configuration.EncapsulationTerminators;
        this._markLastChunk = Wordsmith.Configuration.ContinuationMarkerOnLast;
        this._scratchMaxTextLen = Wordsmith.Configuration.ScratchPadMaximumTextLength;
        this._scratchEnter = (int)Wordsmith.Configuration.ScratchPadTextEnterBehavior;

        // Alias Settings
        this._headerAliases = new(Wordsmith.Configuration.HeaderAliases);

        // Spell Check Settings
        this._fixDoubleSpace = Wordsmith.Configuration.ReplaceDoubleSpaces;
        this._dictionaryFilename = Wordsmith.Configuration.DictionaryFile;
        this._maxSuggestions = Wordsmith.Configuration.MaximumSuggestions;

        // Linkshell Settings
        this._linkshells = Wordsmith.Configuration.LinkshellNames;
        this._cwlinkshells = Wordsmith.Configuration.CrossWorldLinkshellNames;

        // Color Settings
        this._enableTextColor = Wordsmith.Configuration.EnableTextHighlighting;
        this._spellingErrorColor = Wordsmith.Configuration.SpellingErrorHighlightColor;
        this._headerColors = Wordsmith.Configuration.HeaderColors;
}

    private void Save()
    {
        // Thesaurus Settings.
        if (this._searchHistoryCountChange != Wordsmith.Configuration.SearchHistoryCount)
            Wordsmith.Configuration.SearchHistoryCount = this._searchHistoryCountChange;

        if (this._researchToTopChange != Wordsmith.Configuration.ResearchToTop)
            Wordsmith.Configuration.ResearchToTop = this._researchToTopChange;

        // Scratch Pad settings.
        if (this._contextMenu != Wordsmith.Configuration.AddContextMenuOption)
            Wordsmith.Configuration.AddContextMenuOption = this._contextMenu;

        if (this._autoClear != Wordsmith.Configuration.AutomaticallyClearAfterLastCopy)
            Wordsmith.Configuration.AutomaticallyClearAfterLastCopy = _autoClear;

        if (this._deleteClosed != Wordsmith.Configuration.DeleteClosedScratchPads)
            Wordsmith.Configuration.DeleteClosedScratchPads = this._deleteClosed;

        if (this._ignoreHypen != Wordsmith.Configuration.IgnoreWordsEndingInHyphen)
            Wordsmith.Configuration.IgnoreWordsEndingInHyphen = this._ignoreHypen;

        if (this._showChunks != Wordsmith.Configuration.ShowTextInChunks)
            Wordsmith.Configuration.ShowTextInChunks = this._showChunks;

        if (this._onSentence != Wordsmith.Configuration.SplitTextOnSentence)
            Wordsmith.Configuration.SplitTextOnSentence = this._onSentence;

        if (this._detectHeader != Wordsmith.Configuration.ParseHeaderInput)
            Wordsmith.Configuration.ParseHeaderInput = this._detectHeader;

        if (this._oocOpening != Wordsmith.Configuration.OocOpeningTag)
            Wordsmith.Configuration.OocOpeningTag = this._oocOpening;

        if (this._oocClosing != Wordsmith.Configuration.OocClosingTag)
            Wordsmith.Configuration.OocClosingTag = this._oocClosing;

        if (this._sentenceTerminators != Wordsmith.Configuration.SentenceTerminators)
            Wordsmith.Configuration.SentenceTerminators = this._sentenceTerminators;

        if (this._encapTerminators != Wordsmith.Configuration.EncapsulationTerminators)
            Wordsmith.Configuration.EncapsulationTerminators = this._encapTerminators;

        if (this._continueMarker != Wordsmith.Configuration.ContinuationMarker)
            Wordsmith.Configuration.ContinuationMarker = this._continueMarker;

        if (this._markLastChunk != Wordsmith.Configuration.ContinuationMarkerOnLast)
            Wordsmith.Configuration.ContinuationMarkerOnLast = this._markLastChunk;

        if (this._scratchMaxTextLen != Wordsmith.Configuration.ScratchPadMaximumTextLength)
            Wordsmith.Configuration.ScratchPadMaximumTextLength = this._scratchMaxTextLen;

        if ((Enums.EnterKeyAction)this._scratchEnter != Wordsmith.Configuration.ScratchPadTextEnterBehavior)
            Wordsmith.Configuration.ScratchPadTextEnterBehavior = (Enums.EnterKeyAction)this._scratchEnter;

        // Alias Settings
        if (this._headerAliases != Wordsmith.Configuration.HeaderAliases)
            Wordsmith.Configuration.HeaderAliases = this._headerAliases;

        // Spell Check settings.
        if (this._fixDoubleSpace != Wordsmith.Configuration.ReplaceDoubleSpaces)
            Wordsmith.Configuration.ReplaceDoubleSpaces = this._fixDoubleSpace;

        if (this._dictionaryFilename != Wordsmith.Configuration.DictionaryFile)
        {
            Wordsmith.Configuration.DictionaryFile = this._dictionaryFilename;
            Lang.Reinit();
        }

        if (this._maxSuggestions != Wordsmith.Configuration.MaximumSuggestions)
            Wordsmith.Configuration.MaximumSuggestions = this._maxSuggestions;

        // Linkshell settings
        if (this._linkshells != Wordsmith.Configuration.LinkshellNames)
            Wordsmith.Configuration.LinkshellNames = this._linkshells;

        if (this._cwlinkshells != Wordsmith.Configuration.CrossWorldLinkshellNames)
            Wordsmith.Configuration.CrossWorldLinkshellNames = this._cwlinkshells;

        // Color settings
        if ( this._enableTextColor != Wordsmith.Configuration.EnableTextHighlighting )
            Wordsmith.Configuration.EnableTextHighlighting = this._enableTextColor;

        if ( this._spellingErrorColor != Wordsmith.Configuration.SpellingErrorHighlightColor )
            Wordsmith.Configuration.SpellingErrorHighlightColor = this._spellingErrorColor;

        if (this._headerColors != Wordsmith.Configuration.HeaderColors)
            Wordsmith.Configuration.HeaderColors = this._headerColors;

        // Save the configuration
        Wordsmith.Configuration.Save();
    }
}
