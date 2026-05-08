// =====================================================================================
// VSTMC.Wizard - FULL UPDATED IWizardImplementation.cs (buffer-first, EnvDTE)
// Updates implemented exactly as requested:
//
// KeyinCommands.vb insertion
//   - Marker is the matching "#End Region" for:
//     #Region "VSTMC GENERATED KEYINS (Do Not remove this region)"
//   - Code is inserted immediately ABOVE that matching #End Region.
//
// Commands.xml insertion
//   - KeyinHandler insertion marker is "</KeyinHandler>"
//     (insert the <KeyinHandler .../> line ABOVE that marker line)
//   - Keywords insertion marker is "<!-- VSTMC:KEYWORDS_INSERT -->"
//     (marker selects the correct KeyinTable when multiple exist)
//     Insert the <Keyword ...> line ABOVE the </KeyinTable> that closes that table.
//
// IMPORTANT NOTE:
//   Your original file contains long blocks not shown here (AddAddinAttribute, AddReferences,
//   BuildReferenceXml, etc.). I left them as stubs at the bottom with clear "KEEP YOUR EXISTING"
//   markers. Paste your existing implementations into those stub methods.
//
// =====================================================================================

using EnvDTE;
using Microsoft.IO;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TemplateWizard;
using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml.Linq;

[assembly: CLSCompliant(true)]
namespace VSTMC.Wizard
{
    public class IWizardImplementation : IWizard
    {
        #region IWizard Methods

        [CLSCompliant(false)]
        public void BeforeOpeningFile(ProjectItem projectItem) { }

        [CLSCompliant(false)]
        public void ProjectFinishedGenerating(Project project)
        {
//            string userFilePath = project.FullName + ".user";

//            string userFileContent =
//        @"<?xml version=""1.0"" encoding=""utf-8""?>
//<Project ToolsVersion=""Current"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
//  <PropertyGroup Condition=""'$(Configuration)|$(Platform)'=='Debug|x64'"">
//    <LocalDebuggerDebuggerType>NativeOnly</LocalDebuggerDebuggerType>
//    <DebuggerFlavor>WindowsLocalDebugger</DebuggerFlavor>
//  </PropertyGroup>
//</Project>";

//            File.WriteAllText(userFilePath, userFileContent);

//            project.Save();
        }

        [CLSCompliant(false)]
        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            Cursor.Current = Cursors.Default;
        }

        public void RunFinished()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (IsNewItem)
            {
                ClassName = ReplacementsDictionary["$safeitemname$"];
                ClassName_Upper = ClassName.ToUpperInvariant();
                SafeProjectName_Upper = ReplacementsDictionary["$safeprojectname_UPPER$"];

                FunctionName = "Start" + ClassName;                

                if (IsItemAddedToKeyins)
                {
                    AddCommand();
                }

                if (IsCPPProject)
                {
                    switch (GetItemTypeFromTemplate)
                    {
                        case "MicroStation Placement Tool":
                            UpdatePlacementToolCppFiles();
                            break;
                        case "MicroStation Placement Tool Settings":
                            UpdatePlacementToolSettingsCppFiles();
                            break;
                        case "Selection Tool Settings":
                            UpdateSelectionToolCppFiles();
                            break;
                        case "Selection Set Tool Settings":
                            UpdateSelectionSetToolCppFiles();
                            break;
                        default:
                            break;
                    }
                }
                                
                try { ActiveProject.Save(); } catch { }
            }

            Cursor.Current = Cursors.Default;            
        }        

        [CLSCompliant(false)]
        public void RunStarted(
            object automationObject,
            Dictionary<string, string> replacementsDictionary,
            WizardRunKind runKind,
            object[] customParams)
        {
            ReplacementsDictionary = replacementsDictionary;

            if (ReplacementsDictionary.ContainsKey("$safeprojectname$"))
            {
                ReplacementsDictionary["$safeprojectname$"] =
                    ReplacementsDictionary["$safeprojectname$"]
                        .Replace(" ", "_")
                        .Replace("-", "_")
                        .Replace("+", "_");

                ReplacementsDictionary["$safeprojectname_UPPER$"] =
                    ReplacementsDictionary["$safeprojectname$"].ToUpperInvariant();
            }

            ProjectFileInfo = new System.IO.FileInfo(customParams[0].ToString());
            IsNewItem = (runKind.ToString() == "AsNewItem");
            
            ThreadHelper.ThrowIfNotOnUIThread();
            Dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));           

            IsCommandsXmlAllowed = true;
            IsItemAddedToKeyins = false;

            Dte.Windows.Item(EnvDTE.Constants.vsext_wk_SProjectWindow).Activate();
            Dte.Windows.Item(EnvDTE.Constants.vsext_wk_SProjectWindow).Visible = true;

            if (IsNewItem)
            {
                ReplacementProjectItem(replacementsDictionary);
                ProcessProjectItem();
            }
            else
                ProcessProject();

        }

        #region Common CPP Editor

        private void InsertIntoKeyinHandlersIncludeCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string includeLine = $"#include \"{ClassName}.h\"";
            string anchorLine = "#include \"KeyinHandlers.h\"";

            // Already present anywhere? Stop.
            if (text.IndexOf(includeLine, StringComparison.Ordinal) >= 0)
                return;

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int anchorIndex = lines.FindIndex(line => line.Trim() == anchorLine);
            if (anchorIndex < 0)
                throw new InvalidOperationException("KeyinHandlers.h include not found.");

            // Start right after KeyinHandlers.h
            int insertIndex = anchorIndex + 1;

            // Move through the contiguous local include block only
            while (insertIndex < lines.Count)
            {
                string trimmed = lines[insertIndex].Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                    break;

                if (!trimmed.StartsWith("#include \"", StringComparison.Ordinal))
                    break;

                insertIndex++;
            }

            lines.Insert(insertIndex, includeLine);

            string newText = string.Join(nl, lines);
            ReplaceAllText(td, newText);
        }

        private void InsertIntoKeyinHandlersHeader(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string declaration = $"static void {ClassName}(Bentley::WCharCP unparsed);";

            // Prevent duplicate
            if (text.IndexOf(declaration, StringComparison.Ordinal) >= 0)
                return;

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int publicIndex = lines.FindIndex(line => line.Trim() == "public:");
            if (publicIndex < 0)
                throw new InvalidOperationException("'public:' not found in KeyinHandlers.h.");

            // Start just after public:
            int insertIndex = publicIndex + 1;

            // Move past existing contiguous method declarations in the public section
            while (insertIndex < lines.Count)
            {
                string trimmed = lines[insertIndex].Trim();

                if (string.IsNullOrWhiteSpace(trimmed))
                    break;

                if (!trimmed.StartsWith("static ", StringComparison.Ordinal))
                    break;

                insertIndex++;
            }

            // Use the same indentation as the first declaration after public:, if present
            string indent = "        "; // fallback
            if (publicIndex + 1 < lines.Count)
            {
                string nextLine = lines[publicIndex + 1];
                int i = 0;
                while (i < nextLine.Length && (nextLine[i] == ' ' || nextLine[i] == '\t'))
                    i++;

                if (i > 0)
                    indent = nextLine.Substring(0, i);
            }

            lines.Insert(insertIndex, indent + declaration);

            string newText = string.Join(nl, lines);
            ReplaceAllText(td, newText);
        }

        #endregion

        #region MicroStation Placement Tool Settings        

        private void InsertIntoKeyinHandlersCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string methodSignature = $"void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed)";

            // Prevent duplicate
            if (text.IndexOf(methodSignature, StringComparison.Ordinal) >= 0)
                return;

            string methodText =
                $"{nl}    void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed){nl}" +
                $"    {{{nl}" +
                $"        (void)unparsed;                 // Reserved for future key-in arguments{nl}" +
                $"        Tools::Start{ClassName}(unparsed);{nl}" +
                $"    }};{nl}";

            string namespaceAnchor = "namespace Commands";
            int nsPos = text.IndexOf(namespaceAnchor, StringComparison.Ordinal);
            if (nsPos < 0)
                throw new InvalidOperationException("namespace Commands not found.");

            int openBrace = text.IndexOf("{", nsPos, StringComparison.Ordinal);
            if (openBrace < 0)
                throw new InvalidOperationException("Opening brace for namespace not found.");

            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                throw new InvalidOperationException("Closing brace for namespace not found.");

            // Insert BEFORE namespace closing brace
            int insertPos = text.LastIndexOf(nl, closeBrace, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? closeBrace : insertPos + nl.Length;

            text = text.Insert(insertPos, methodText);

            ReplaceAllText(td, text);
        }

        private void InsertIntoBentleyApiCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string methodSignature = $"extern \"C\" void cmd_{ClassName}(WCharCP unparsed)";
            string arrayEntry = $"{{ (CmdHandler)cmd_{ClassName}, CMD_{SafeProjectName_Upper}_{ClassName_Upper} }},";

            // ---------- 1) Insert extern "C" method before static array ----------
            if (text.IndexOf(methodSignature, StringComparison.Ordinal) < 0)
            {
                string staticAnchor = "static MdlCommandNumber s_commandNumbers[]";
                int staticPos = text.IndexOf(staticAnchor, StringComparison.Ordinal);
                if (staticPos < 0)
                    throw new InvalidOperationException("Command array declaration not found in BentleyApi.cpp.");

                int insertMethodPos = text.LastIndexOf(nl, staticPos, StringComparison.Ordinal);
                insertMethodPos = (insertMethodPos < 0) ? staticPos : insertMethodPos + nl.Length;

                string methodText =
                    $"    extern \"C\" void cmd_{ClassName}(WCharCP unparsed){nl}" +
                    $"    {{{nl}" +
                    $"        Commands::KeyinHandlers::{ClassName}(unparsed);{nl}" +
                    $"    }}{nl}{nl}";

                text = text.Insert(insertMethodPos, methodText);
            }

            // ---------- 2) Insert array entry before terminating 0 ----------
            if (text.IndexOf(arrayEntry, StringComparison.Ordinal) < 0)
            {
                string staticAnchor = "static MdlCommandNumber s_commandNumbers[]";
                int staticPos = text.IndexOf(staticAnchor, StringComparison.Ordinal);
                if (staticPos < 0)
                    throw new InvalidOperationException("Command array declaration not found in BentleyApi.cpp.");

                int openBrace = text.IndexOf("{", staticPos, StringComparison.Ordinal);
                if (openBrace < 0)
                    throw new InvalidOperationException("Opening brace for s_commandNumbers[] not found.");

                int closeBrace = -1;
                int depth = 0;

                for (int i = openBrace; i < text.Length; i++)
                {
                    if (text[i] == '{') depth++;
                    else if (text[i] == '}')
                    {
                        depth--;
                        if (depth == 0)
                        {
                            closeBrace = i;
                            break;
                        }
                    }
                }

                if (closeBrace < 0)
                    throw new InvalidOperationException("Closing brace for s_commandNumbers[] not found.");

                // Find the terminating 0 inside the array block
                string arrayBlock = text.Substring(openBrace, closeBrace - openBrace);
                int zeroPosInBlock = arrayBlock.LastIndexOf("0", StringComparison.Ordinal);
                if (zeroPosInBlock < 0)
                    throw new InvalidOperationException("Terminating 0 not found in s_commandNumbers[] array.");

                int zeroPos = openBrace + zeroPosInBlock;

                // Find indentation from the first existing entry line
                int firstEntryPos = text.IndexOf("{ (CmdHandler)", openBrace, StringComparison.Ordinal);
                string indent = "        "; // fallback
                if (firstEntryPos >= 0 && firstEntryPos < closeBrace)
                {
                    int lineStart = text.LastIndexOf(nl, firstEntryPos, StringComparison.Ordinal);
                    lineStart = (lineStart < 0) ? 0 : lineStart + nl.Length;

                    int indentEnd = lineStart;
                    while (indentEnd < text.Length && (text[indentEnd] == ' ' || text[indentEnd] == '\t'))
                        indentEnd++;

                    indent = text.Substring(lineStart, indentEnd - lineStart);
                }

                int insertEntryPos = text.LastIndexOf(nl, zeroPos, StringComparison.Ordinal);
                insertEntryPos = (insertEntryPos < 0) ? zeroPos : insertEntryPos + nl.Length;

                string entryText = indent + arrayEntry + nl;
                text = text.Insert(insertEntryPos, entryText);
            }

            ReplaceAllText(td, text);
        }        

        private void InsertIntoIdsFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string sharedBlock =
                $"#define STRINGLISTID_Commands           1{nl}" +
                $"#define STRINGLISTID_Prompts            2{nl}{nl}" +
                $"#define ITEMLISTID_PlacementSettings    1{nl}{nl}" +
                $"#define COLORPICKERID_LineColor         5{nl}" +
                $"#define HOOKITEMID_LineColorCombo       1{nl}{nl}" +
                $"#define COMBOBOXID_LineWeight           2{nl}" +
                $"#define HOOKITEMID_LineWeightCombo      2{nl}{nl}" +
                $"#define COMBOBOXID_LineStyle            3{nl}" +
                $"#define HOOKITEMID_LineStyleCombo       3{nl}{nl}" +
                $"#define COMBOBOXID_LineLevel            4{nl}" +
                $"#define HOOKITEMID_LineLevelCombo       4{nl}{nl}" +
                $"#define TOGGLEID_UseByLevel             6{nl}" +
                $"#define HOOKITEMID_UseByLevelToggle     7{nl}";

            string toolBlock =
                $"{nl}#define CMDNAME_{ClassName}         1{nl}" +
                $"#define PROMPT_{ClassName}          1{nl}{nl}";

            if (text.IndexOf("#define STRINGLISTID_Commands", StringComparison.Ordinal) < 0)
            {
                int dllAppIdPos = text.IndexOf("#define DLLAPPID", StringComparison.Ordinal);
                if (dllAppIdPos >= 0)
                {
                    int dllLineEnd = text.IndexOf(nl, dllAppIdPos, StringComparison.Ordinal);
                    dllLineEnd = (dllLineEnd < 0) ? text.Length : dllLineEnd + nl.Length;
                    text = text.Insert(dllLineEnd, nl + sharedBlock);
                }
                else
                {
                    text = text.TrimEnd('\r', '\n') + nl + nl + sharedBlock;
                }
            }

            if (text.IndexOf($"#define CMDNAME_{ClassName}", StringComparison.Ordinal) >= 0)
            {
                ReplaceAllText(td, text);
                return;
            }

            int lastPromptPos = text.LastIndexOf("#define PROMPT_", StringComparison.Ordinal);
            if (lastPromptPos >= 0)
            {
                int lastPromptLineEnd = text.IndexOf(nl, lastPromptPos, StringComparison.Ordinal);
                lastPromptLineEnd = (lastPromptLineEnd < 0) ? text.Length : lastPromptLineEnd + nl.Length;
                text = text.Insert(lastPromptLineEnd, toolBlock);
            }
            else
            {
                int sharedAnchor = text.IndexOf("#define HOOKITEMID_UseByLevelToggle", StringComparison.Ordinal);
                if (sharedAnchor >= 0)
                {
                    int sharedLineEnd = text.IndexOf(nl, sharedAnchor, StringComparison.Ordinal);
                    sharedLineEnd = (sharedLineEnd < 0) ? text.Length : sharedLineEnd + nl.Length;
                    text = text.Insert(sharedLineEnd, toolBlock);
                }
                else
                {
                    text = text.TrimEnd('\r', '\n') + nl + toolBlock;
                }
            }

            ReplaceAllText(td, text);
        }

        private void InsertIntoMsgFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string commandEntry = $"{{CMDNAME_{ClassName}, \"{ClassName}\" }},{nl}";
            string promptEntry = $"{{CMDNAME_{ClassName}, \"Identify start point\" }},{nl}";

            bool hasCommandsList = text.IndexOf("MessageList STRINGLISTID_Commands", StringComparison.Ordinal) >= 0;
            bool hasPromptsList = text.IndexOf("MessageList STRINGLISTID_Prompts", StringComparison.Ordinal) >= 0;

            bool hasCommandEntry = text.IndexOf($"{{CMDNAME_{ClassName}, \"{ClassName}\" }}", StringComparison.Ordinal) >= 0;
            bool hasPromptEntry = text.IndexOf($"{{CMDNAME_{ClassName}, \"Identify start point\" }}", StringComparison.Ordinal) >= 0;

            if (!hasCommandsList || !hasPromptsList)
            {
                string block =
                    $"MessageList STRINGLISTID_Commands ={nl}" +
                    $"{{{nl}" +
                    $"    {{{nl}" +
                    $"        {{CMDNAME_{ClassName}, \"{ClassName}\" }},{nl}" +
                    $"    }}{nl}" +
                    $"}}{nl}{nl}" +
                    $"MessageList STRINGLISTID_Prompts ={nl}" +
                    $"{{{nl}" +
                    $"    {{{nl}" +
                    $"        {{CMDNAME_{ClassName}, \"Identify start point\" }},{nl}" +
                    $"    }}{nl}" +
                    $"}}{nl}";

                text = AppendBlockToEnd(text, block, nl);
                ReplaceAllText(td, text);
                return;
            }

            if (!hasCommandEntry)
                text = InsertIntoMessageList(text, "MessageList STRINGLISTID_Commands", commandEntry, nl);

            if (!hasPromptEntry)
                text = InsertIntoMessageList(text, "MessageList STRINGLISTID_Prompts", promptEntry, nl);

            ReplaceAllText(td, text);
        }

        private string AppendBlockToEnd(string text, string block, string nl)
        {
            string trimmed = text.TrimEnd('\r', '\n');

            if (trimmed.Length == 0)
                return block;

            return trimmed + nl + nl + block;
        }

        private string InsertIntoMessageList(string text, string listHeader, string entryCore, string nl)
        {
            int listPos = text.IndexOf(listHeader, StringComparison.Ordinal);
            if (listPos < 0)
                return text;

            int outerOpen = text.IndexOf("{", listPos, StringComparison.Ordinal);
            if (outerOpen < 0)
                return text;

            int innerOpen = text.IndexOf("{", outerOpen + 1, StringComparison.Ordinal);
            if (innerOpen < 0)
                return text;

            int depth = 0;
            int innerClose = -1;

            for (int i = innerOpen; i < text.Length; i++)
            {
                if (text[i] == '{')
                    depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        innerClose = i;
                        break;
                    }
                }
            }

            if (innerClose < 0)
                return text;

            string innerBlock = text.Substring(innerOpen, innerClose - innerOpen);
            if (innerBlock.IndexOf(entryCore.Trim(), StringComparison.Ordinal) >= 0)
                return text;

            int firstEntryPos = text.IndexOf("{CMDNAME_", innerOpen, StringComparison.Ordinal);
            if (firstEntryPos < 0 || firstEntryPos > innerClose)
                return text;

            int lineStart = text.LastIndexOf(nl, firstEntryPos, StringComparison.Ordinal);
            lineStart = (lineStart < 0) ? 0 : lineStart + nl.Length;

            int indentEnd = lineStart;
            while (indentEnd < text.Length && (text[indentEnd] == ' ' || text[indentEnd] == '\t'))
                indentEnd++;

            string indent = text.Substring(lineStart, indentEnd - lineStart);

            int insertPos = text.LastIndexOf(nl, innerClose, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? innerClose : insertPos + nl.Length;

            string newLine = indent + entryCore.TrimStart();

            return text.Insert(insertPos, newLine);
        }

        private void InsertIntoResourceFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            // Duplicate guard: one stable line from the block
            if (text.IndexOf("DItem_ColorPickerRsc COLORPICKERID_LineColor", StringComparison.Ordinal) >= 0)
                return;

            string anchor = "DllMdlApp DLLAPPID =";
            int anchorPos = text.IndexOf(anchor, StringComparison.Ordinal);
            if (anchorPos < 0)
                throw new InvalidOperationException("DllMdlApp DLLAPPID = not found in cmd.r.");

            int insertPos = text.LastIndexOf(nl, anchorPos, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? anchorPos : insertPos + nl.Length;

            string block =
                $"DItem_ColorPickerRsc COLORPICKERID_LineColor ={nl}" +
                $"{{{nl}" +
                $"    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKITEMID_LineColorCombo, NOARG,{nl}" +
                $"    0, NOMASK,{nl}" +
                $"    \"Line color:(~C)\", \"\"{nl}" +
                $"}};{nl}{nl}" +

                $"DItem_ComboBoxRsc COMBOBOXID_LineWeight ={nl}" +
                $"{{{nl}" +
                $"    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKITEMID_LineWeightCombo, NOARG,{nl}" +
                $"    256, \"\", \"\", \"\", \"\", NOMASK,{nl}" +
                $"    0, 18, 4, 0, 0,{nl}" +
                $"    COMBOATTR_READONLY | COMBOATTR_DISPLAYALLCOLUMNS | COMBOATTR_USEMODELVALUE |{nl}" +
                $"    COMBOATTR_DRAWPREFIXICON | COMBOATTR_NODISABLEICON | COMBOATTR_FULLWIDTH,{nl}" +
                $"    \"Line weight:(~W)\",{nl}" +
                $"    \"\",{nl}" +
                $"    {{{nl}" +
                $"        {{18*XC, 256, ALIGN_LEFT, \"\"}},{nl}" +
                $"    }}{nl}" +
                $"}};{nl}{nl}" +

                $"DItem_ComboBoxRsc COMBOBOXID_LineStyle ={nl}" +
                $"{{{nl}" +
                $"    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKITEMID_LineStyleCombo, NOARG,{nl}" +
                $"    256, \"\", \"\", \"\", \"\", NOMASK,{nl}" +
                $"    0, 18, 4, 0, 0,{nl}" +
                $"    COMBOATTR_READONLY | COMBOATTR_DISPLAYALLCOLUMNS | COMBOATTR_USEMODELVALUE |{nl}" +
                $"    COMBOATTR_DRAWPREFIXICON | COMBOATTR_NODISABLEICON | COMBOATTR_FULLWIDTH,{nl}" +
                $"    \"Line style:(~S)\",{nl}" +
                $"    \"\",{nl}" +
                $"    {{{nl}" +
                $"        {{18*XC, 256, ALIGN_LEFT, \"\"}},{nl}" +
                $"    }}{nl}" +
                $"}};{nl}{nl}" +

                $"DItem_ComboBoxRsc COMBOBOXID_LineLevel ={nl}" +
                $"{{{nl}" +
                $"    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKITEMID_LineLevelCombo, NOARG,{nl}" +
                $"    256, \"%s\", \"%s\", \"\", \"\", NOMASK,{nl}" +
                $"    0, 18, 4, 0, 0, COMBOATTR_READONLY | COMBOATTR_FULLWIDTH,{nl}" +
                $"    \"Line level:(~L)\", \"\",{nl}" +
                $"    {{{nl}" +
                $"        {{0, 256, ALIGN_LEFT, \"\"}},{nl}" +
                $"    }}{nl}" +
                $"}};{nl}{nl}" +

                $"DItem_ToggleButtonRsc TOGGLEID_UseByLevel ={nl}" +
                $"{{{nl}" +
                $"    NOCMD, LCMD, NOSYNONYM, NOHELP, MHELP, HOOKITEMID_UseByLevelToggle, NOARG,{nl}" +
                $"    ON, NOMASK,{nl}" +
                $"    \"Use ByLevel symbology\", \"\"{nl}" +
                $"}};{nl}{nl}" +

                $"CmdItemListRsc ITEMLISTID_PlacementSettings ={nl}" +
                $"{{{{{nl}" +
                $"    {{{{10*XC, YC,   3*XC, 0}}, ColorPicker, COLORPICKERID_LineColor, ON, 0, \"\", \"\"}},{nl}" +
                $"    {{{{10*XC, YC*3, 20*XC, 0}}, ComboBox, COMBOBOXID_LineWeight, ON, 0, \"\", \"\"}},{nl}" +
                $"    {{{{10*XC, YC*5, 20*XC, 0}}, ComboBox, COMBOBOXID_LineStyle,  ON, 0, \"\", \"\"}},{nl}" +
                $"    {{{{10*XC, YC*7, 20*XC, 0}}, ComboBox, COMBOBOXID_LineLevel,  ON, 0, \"\", \"\"}},{nl}" +
                $"    {{{{10*XC, YC*9, 20*XC, 0}}, ToggleButton, TOGGLEID_UseByLevel, ON, 0, \"\", \"\"}},{nl}" +
                $"}}}};{nl}{nl}";

            text = text.Insert(insertPos, block);
            ReplaceAllText(td, text);
        }

        private void InsertIntoMainCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            // Shared block should only exist once
            bool alreadyInserted =
                text.IndexOf("UInt32 g_lineColor = COLOR_BYLEVEL;", StringComparison.Ordinal) >= 0 ||
                text.IndexOf("static void SyncToolSettingsFromActive()", StringComparison.Ordinal) >= 0 ||
                text.IndexOf("DialogHookInfo uHooks[] =", StringComparison.Ordinal) >= 0;

            if (alreadyInserted)
                return;

            string anchor = "extern \"C\" DLLEXPORT void MdlMain";
            int anchorPos = text.IndexOf(anchor, StringComparison.Ordinal);
            if (anchorPos < 0)
                throw new InvalidOperationException("MdlMain entry point not found in main cpp.");

            int insertPos = text.LastIndexOf(nl, anchorPos, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? anchorPos : insertPos + nl.Length;

            string block = @"
USING_NAMESPACE_BENTLEY_DGNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM
USING_NAMESPACE_BENTLEY_MSTNPLATFORM_ELEMENT

UInt32 g_lineColor = COLOR_BYLEVEL;
MSDialogP g_lineColorDialog = nullptr;
int g_lineColorItemIndex = -1;

MSDialogP g_lineWeightDialog = nullptr;
int g_lineWeightItemIndex = -1;

Int32 g_lineWeight = (Int32)WEIGHT_BYLEVEL;

Int32 g_lineStyle = (Int32)STYLE_BYLEVEL;

MSDialogP g_lineStyleDialog = nullptr;
int g_lineStyleItemIndex = -1;

MSDialogP g_lineLevelDialog = nullptr;
int g_lineLevelItemIndex = -1;

bool g_useByLevel = false;

UInt32 GetSelectedLineColor()
{
    return g_lineColor;
}

int GetSelectedLineWeight()
{
    return (int)g_lineWeight;
}

int GetSelectedLineStyle()
{
    return (int)g_lineStyle;
}

static void SyncLineColorFromActive()
{
    UInt32 activeColor = 0;
    ActiveParams::GetValue(activeColor, ACTIVEPARAM_COLOR);

    if (activeColor == COLOR_BYLEVEL)
    {
        UInt32 activeLevelId = 0;
        ActiveParams::GetValue(activeLevelId, ACTIVEPARAM_LEVEL);

        DgnFileP pDgnFile = ISessionMgr::GetActiveDgnFile();
        if (nullptr != pDgnFile)
        {
            FileLevelCacheR levelCache = pDgnFile->GetLevelCacheR();
            LevelHandle lh = levelCache.GetLevel(activeLevelId);

            if (lh.IsValid())
            {
                auto levelColor = lh.GetByLevelColor();
                activeColor = (UInt32)levelColor.GetColor();
            }
        }
    }

    g_lineColor = activeColor;

    if (nullptr != g_lineColorDialog && g_lineColorItemIndex >= 0)
    {
        bool changed = false;
        MSValueDescr value((UInt32)g_lineColor);
        mdlDialog_itemSetValue(&changed, value, g_lineColorDialog, g_lineColorItemIndex);
        mdlDialog_itemSynch(g_lineColorDialog, g_lineColorItemIndex);
    }
}

static void SyncLineWeightFromActive()
{
    UInt32 activeWeight = 0;
    ActiveParams::GetValue(activeWeight, ACTIVEPARAM_LINEWEIGHT);

    g_lineWeight = (Int32)activeWeight;

    if (nullptr != g_lineWeightDialog && g_lineWeightItemIndex >= 0)
    {
        bool changed = false;
        MSValueDescr value((UInt32)g_lineWeight);
        mdlDialog_itemSetValue(&changed, value, g_lineWeightDialog, g_lineWeightItemIndex);
    }
}

static void SyncLineStyleFromActive()
{
    Int32 activeStyle = 0;
    ActiveParams::GetValue(activeStyle, ACTIVEPARAM_LINESTYLE);

    g_lineStyle = activeStyle;

    if (nullptr != g_lineStyleDialog && g_lineStyleItemIndex >= 0)
    {
        bool changed = false;
        MSValueDescr value((Int32)g_lineStyle);
        mdlDialog_itemSetValue(&changed, value, g_lineStyleDialog, g_lineStyleItemIndex);
    }
}

static void SyncLineLevelFromActive()
{
    UInt32 activeLevelId = 0;
    ActiveParams::GetValue(activeLevelId, ACTIVEPARAM_LEVEL);

    DgnFileP pDgnFile = ISessionMgr::GetActiveDgnFile();
    if (nullptr == pDgnFile)
        return;

    FileLevelCacheR levelCache = pDgnFile->GetLevelCacheR();
    LevelHandle lh = levelCache.GetLevel(activeLevelId);
    if (!lh.IsValid())
        return;

    WString activeLevelName;
    lh.GetDisplayName(activeLevelName);

    if (nullptr != g_lineLevelDialog && g_lineLevelItemIndex >= 0)
    {
        bool changed = false;
        mdlDialog_itemSetStringValue(&changed, activeLevelName.GetWCharCP(), g_lineLevelDialog, g_lineLevelItemIndex);
    }

    if (nullptr != g_lineWeightDialog && g_lineWeightItemIndex >= 0)
    {
        mdlDialog_itemSynch(g_lineWeightDialog, g_lineWeightItemIndex);
    }

    if (nullptr != g_lineStyleDialog && g_lineStyleItemIndex >= 0)
    {
        mdlDialog_itemSynch(g_lineStyleDialog, g_lineStyleItemIndex);
    }
}

static void SyncToolSettingsFromActive()
{
    SyncLineLevelFromActive();
    SyncLineColorFromActive();
    SyncLineWeightFromActive();
    SyncLineStyleFromActive();
}

static bool TryGetSelectedLevelId(LevelId& levelId)
{
    if (nullptr != g_lineLevelDialog && g_lineLevelItemIndex >= 0)
    {
        WString levelName;
        mdlDialog_itemGetStringValue(levelName, g_lineLevelDialog, g_lineLevelItemIndex);

        if (!levelName.empty())
        {
            DgnFileP pDgnFile = ISessionMgr::GetActiveDgnFile();
            if (nullptr != pDgnFile)
            {
                FileLevelCacheR levelCache = pDgnFile->GetLevelCacheR();
                LevelHandle lh = levelCache.GetLevelByName(levelName.GetWCharCP());
                if (lh.IsValid())
                {
                    levelId = lh.GetLevelId();
                    return true;
                }
            }
        }
    }

    return false;
}

void lineColor_comboHook(DialogItemMessage* dimP)
{
    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType)
    {
    case DITEM_MESSAGE_CREATE:
    {
        g_lineColorDialog = dimP->db;
        g_lineColorItemIndex = dimP->itemIndex;

        SyncLineColorFromActive();
        break;
    }

    case DITEM_MESSAGE_INIT:
    {
        bool changed = false;
        MSValueDescr value((UInt32)g_lineColor);
        mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
        break;
    }

    case DITEM_MESSAGE_STATECHANGED:
    {
        MSValueDescr value;
        mdlDialog_itemGetValue(value, dimP->db, dimP->itemIndex);

        g_lineColor = (UInt32)value.GetUShort();

        break;
    }

    case DITEM_MESSAGE_SYNCHRONIZE:
    {
        bool changed = false;
        MSValueDescr value((UInt32)g_lineColor);
        mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
        break;
    }

    case DITEM_MESSAGE_DESTROY:
    {
        g_lineColorDialog = nullptr;
        g_lineColorItemIndex = -1;
        break;
    }

    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}

void lineWeight_comboHook(DialogItemMessage* dimP)
{
    RawItemHdr* riP = dimP->dialogItemP->rawItemP;
    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType)
    {
    case DITEM_MESSAGE_CREATE:
    {
        g_lineWeightDialog = dimP->db;
        g_lineWeightItemIndex = dimP->itemIndex;

        ListModelP pListModel = mdlListModel_create(1);
        if (nullptr == pListModel)
        {
            dimP->u.create.createFailed = TRUE;
            break;
        }

        LevelId levelId;
        LevelId* levelIdP = nullptr;

        if (TryGetSelectedLevelId(levelId))
            levelIdP = &levelId;

        UInt32 color = 0;
        UInt32 lmOptions = 0;

        if (SUCCESS == mdlLevelList_getWeightListModel(pListModel, MASTERFILE, levelIdP, color, lmOptions))
        {
            mdlDialog_comboBoxSetListModelP(riP, pListModel);
        }
        else
        {
            mdlListModel_destroy(pListModel, TRUE);
        }

        break;
    }

    case DITEM_MESSAGE_INIT:
    {
        bool changed = false;
        MSValueDescr value((UInt32)g_lineWeight);
        mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);

        break;
    }

    case DITEM_MESSAGE_STATECHANGED:
    {
        MSValueDescr value;
        mdlDialog_itemGetValue(value, dimP->db, dimP->itemIndex);

        Int32 selected = (Int32)value.GetLong();
        g_lineWeight = selected;

        break;
    }

    case DITEM_MESSAGE_SYNCHRONIZE:
    {
        ListModelP pListModel = mdlDialog_comboBoxGetListModelP(riP);
        if (nullptr != pListModel)
        {
            LevelId levelId;
            LevelId* levelIdP = nullptr;

            if (TryGetSelectedLevelId(levelId))
                levelIdP = &levelId;

            mdlLevelList_synchronizeWeightListModel(pListModel, MASTERFILE, levelIdP);

            bool changed = false;
            MSValueDescr value((UInt32)g_lineWeight);
            mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
        }
        break;
    }

    case DITEM_MESSAGE_DESTROY:
    {
        g_lineWeightDialog = nullptr;
        g_lineWeightItemIndex = -1;

        ListModelP pListModel = mdlDialog_comboBoxGetListModelP(riP);
        if (nullptr != pListModel)
            mdlListModel_destroy(pListModel, TRUE);

        break;
    }

    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}

void lineStyle_comboHook(DialogItemMessage* dimP)
{
    RawItemHdr* riP = dimP->dialogItemP->rawItemP;
    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType)
    {
    case DITEM_MESSAGE_CREATE:
    {
        g_lineStyleDialog = dimP->db;
        g_lineStyleItemIndex = dimP->itemIndex;

        ListModelP pListModel = mdlListModel_create(1);
        if (nullptr == pListModel)
        {
            dimP->u.create.createFailed = TRUE;
            break;
        }

        LevelId levelId;
        LevelId* levelIdP = nullptr;

        if (TryGetSelectedLevelId(levelId))
            levelIdP = &levelId;

        UInt32 color = 0;
        UInt32 lmOptions = 0;

        if (SUCCESS == mdlLevelList_getStyleListModel(pListModel, MASTERFILE, levelIdP, color, lmOptions))
        {
            mdlDialog_comboBoxSetListModelP(riP, pListModel);

            bool changed = false;
            MSValueDescr value((Int32)g_lineStyle);
            mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
        }
        else
        {
            mdlListModel_destroy(pListModel, TRUE);
        }

        break;
    }

    case DITEM_MESSAGE_INIT:
    {
        bool changed = false;
        MSValueDescr value((Int32)g_lineStyle);
        mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
        mdlDialog_itemDraw(dimP->db, dimP->itemIndex);

        break;
    }

    case DITEM_MESSAGE_STATECHANGED:
    {
        MSValueDescr value;
        mdlDialog_itemGetValue(value, dimP->db, dimP->itemIndex);

        g_lineStyle = (Int32)value.GetLong();
        break;
    }

    case DITEM_MESSAGE_SYNCHRONIZE:
    {
        ListModelP pListModel = mdlDialog_comboBoxGetListModelP(riP);
        if (nullptr != pListModel)
        {
            LevelId levelId;
            LevelId* levelIdP = nullptr;

            if (TryGetSelectedLevelId(levelId))
                levelIdP = &levelId;

            mdlLevelList_synchronizeStyleListModel(pListModel, MASTERFILE, levelIdP);

            bool changed = false;
            MSValueDescr value((Int32)g_lineStyle);
            mdlDialog_itemSetValue(&changed, value, dimP->db, dimP->itemIndex);
            mdlDialog_itemDraw(dimP->db, dimP->itemIndex);
        }
        break;
    }

    case DITEM_MESSAGE_DESTROY:
    {
        g_lineStyleDialog = nullptr;
        g_lineStyleItemIndex = -1;

        ListModelP pListModel = mdlDialog_comboBoxGetListModelP(riP);
        if (nullptr != pListModel)
            mdlListModel_destroy(pListModel, TRUE);

        break;
    }

    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}

void lineLevel_comboHook(DialogItemMessage* dimP)
{
    RawItemHdr* riP = dimP->dialogItemP->rawItemP;

    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType)
    {
    case DITEM_MESSAGE_CREATE:
    {
        g_lineLevelDialog = dimP->db;
        g_lineLevelItemIndex = dimP->itemIndex;

        ListModelP pListModel = mdlListModel_create(1);

        DgnFileP pDgnFile = ISessionMgr::GetActiveDgnFile();
        if (nullptr != pDgnFile)
        {
            bvector<WString> levelNames;

            FileLevelCacheR levelCache = pDgnFile->GetLevelCacheR();
            for (const auto& lh : levelCache)
            {
                WString lvlName;
                lh.GetDisplayName(lvlName);
                levelNames.push_back(lvlName);
            }

            std::sort(levelNames.begin(), levelNames.end(),
                [](WString const& a, WString const& b)
                {
                    return wcscmp(a.GetWCharCP(), b.GetWCharCP()) < 0;
                });

            for (WString const& lvlName : levelNames)
                mdlListModel_insertString(pListModel, lvlName.GetWCharCP(), -1);
        }

        mdlDialog_comboBoxSetListModelP(riP, pListModel);
        SyncToolSettingsFromActive();
        break;
    }

    case DITEM_MESSAGE_DESTROY:
    {
        g_lineLevelDialog = nullptr;
        g_lineLevelItemIndex = -1;

        ListModelP pListModel = mdlDialog_comboBoxGetListModelP(riP);
        if (nullptr != pListModel)
            mdlListModel_destroy(pListModel, TRUE);
        break;
    }

    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}

void useByLevel_toggleHook(DialogItemMessage* dimP)
{
    dimP->msgUnderstood = TRUE;

    switch (dimP->messageType)
    {
    case DITEM_MESSAGE_CREATE:
        g_useByLevel = false;
        break;

    case DITEM_MESSAGE_STATECHANGED:
    {
        WString valueString;
        mdlDialog_itemGetStringValue(valueString, dimP->db, dimP->itemIndex);

        g_useByLevel = !valueString.empty() && 0 != wcscmp(valueString.GetWCharCP(), L""0"");
        break;
    }

    case DITEM_MESSAGE_DESTROY:
        g_useByLevel = false;
        break;

    default:
        dimP->msgUnderstood = FALSE;
        break;
    }
}

DialogHookInfo uHooks[] =
{
    { HOOKITEMID_LineColorCombo,      (PFDialogHook)lineColor_comboHook },
    { HOOKITEMID_LineWeightCombo,     (PFDialogHook)lineWeight_comboHook },
    { HOOKITEMID_LineStyleCombo,      (PFDialogHook)lineStyle_comboHook },
    { HOOKITEMID_LineLevelCombo,      (PFDialogHook)lineLevel_comboHook },
    { HOOKITEMID_UseByLevelToggle,    (PFDialogHook)useByLevel_toggleHook },
};
";

            block = NormalizeNewlines(block).TrimStart('\n').Replace("\n", nl);
            text = text.Insert(insertPos, block + nl);

            ReplaceAllText(td, text);
        }

        private void InsertIntoMainExternCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string publishLine = "mdlDialog_hookPublish(sizeof(uHooks) / sizeof(DialogHookInfo), uHooks);";

            if (text.IndexOf(publishLine, StringComparison.Ordinal) >= 0)
                return;

            string anchor = "extern \"C\" DLLEXPORT void MdlMain";
            int mdlMainPos = text.IndexOf(anchor, StringComparison.Ordinal);
            if (mdlMainPos < 0)
                throw new InvalidOperationException("MdlMain entry point not found.");

            int openBrace = text.IndexOf("{", mdlMainPos, StringComparison.Ordinal);
            if (openBrace < 0)
                throw new InvalidOperationException("Opening brace for MdlMain not found.");

            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{')
                    depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                throw new InvalidOperationException("Closing brace for MdlMain not found.");

            // Insert before the line containing the closing brace
            int insertPos = text.LastIndexOf(nl, closeBrace, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? closeBrace : insertPos + nl.Length;

            // Match indentation from the closing brace line or use 4 spaces fallback
            int lineStart = insertPos;
            int indentEnd = lineStart;
            while (indentEnd < text.Length && (text[indentEnd] == ' ' || text[indentEnd] == '\t'))
                indentEnd++;

            string indent = text.Substring(lineStart, indentEnd - lineStart);
            if (indent.Length == 0)
                indent = "    ";

            string insertText = indent + publishLine + nl;

            text = text.Insert(insertPos, insertText);
            ReplaceAllText(td, text);
        }        

        private void InsertIntoCmdFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string cmdDefineName =
                $"#define CMD_{ReplacementsDictionary["$safeprojectname_UPPER$"]}_{ReplacementsDictionary["$safeitemname_UPPER$"]}";

            // Prevent duplicate define name
            if (text.IndexOf(cmdDefineName, StringComparison.Ordinal) >= 0)
                return;

            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int lastCtIndex = -1;
            int lastCmdIndex = -1;
            int maxCmdValue = 9; // so first becomes 10

            for (int i = 0; i < lines.Count; i++)
            {
                string trimmed = lines[i].Trim();

                if (trimmed.StartsWith("#define CT_", StringComparison.Ordinal))
                    lastCtIndex = i;

                if (trimmed.StartsWith("#define CMD_", StringComparison.Ordinal))
                {
                    lastCmdIndex = i;

                    string[] parts = trimmed
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    if (parts.Length >= 3 && int.TryParse(parts[2], out int value))
                    {
                        if (value > maxCmdValue)
                            maxCmdValue = value;
                    }
                }
            }

            int nextCmdValue = maxCmdValue + 1;

            string newDefine = $"{cmdDefineName}  {nextCmdValue}";

            if (lastCmdIndex >= 0)
            {
                // Existing CMD block: insert immediately after the last CMD define, no blank line
                lines.Insert(lastCmdIndex + 1, newDefine);
            }
            else if (lastCtIndex >= 0)
            {
                // No CMD block yet: insert after CT block with one blank line before first CMD define
                int insertIndex = lastCtIndex + 1;

                // If there isn't already a blank line after CT block, add one
                if (insertIndex < lines.Count && !string.IsNullOrWhiteSpace(lines[insertIndex]))
                {
                    lines.Insert(insertIndex, "");
                    insertIndex++;
                }
                else if (insertIndex >= lines.Count)
                {
                    lines.Add("");
                    insertIndex = lines.Count;
                }
                else
                {
                    // already blank line there
                    insertIndex++;
                }

                lines.Insert(insertIndex, newDefine);
            }
            else
            {
                throw new InvalidOperationException("No #define CT_ block found in cmd.r.");
            }

            string newText = string.Join(nl, lines);
            ReplaceAllText(td, newText);
        }

        private void InsertIntoCmdTableFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string projectUpper = ReplacementsDictionary["$safeprojectname_UPPER$"];
            string classUpper = ReplacementsDictionary["$safeitemname_UPPER$"];

            string cmdToken = $"CMD_{projectUpper}_{classUpper}";

            // Prevent duplicate
            if (text.IndexOf(cmdToken, StringComparison.Ordinal) >= 0)
                return;

            string header = "CommandTable CT_COMMANDS";
            int headerPos = text.IndexOf(header, StringComparison.Ordinal);
            if (headerPos < 0)
                throw new InvalidOperationException("CT_COMMANDS table not found.");

            int openBrace = text.IndexOf("{", headerPos, StringComparison.Ordinal);
            if (openBrace < 0)
                return;

            // Find matching closing brace
            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                return;

            string block = text.Substring(openBrace, closeBrace - openBrace);

            var lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int lastEntryIndex = -1;
            int maxId = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("{"))
                {
                    var parts = line
                        .Trim('{', '}', ',', ' ')
                        .Split(',');

                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
                    {
                        if (id >= maxId)
                        {
                            maxId = id;
                            lastEntryIndex = i;
                        }
                    }
                }
            }

            int nextId = maxId + 1;

            // Ensure previous line ends with comma
            if (lastEntryIndex >= 0)
            {
                if (!lines[lastEntryIndex].TrimEnd().EndsWith(","))
                {
                    lines[lastEntryIndex] = lines[lastEntryIndex].TrimEnd() + ",";
                }
            }

            // Capture indentation from last entry
            string indent = "    ";
            if (lastEntryIndex >= 0)
            {
                string original = lines[lastEntryIndex];
                int ws = original.TakeWhile(c => c == ' ' || c == '\t').Count();
                indent = original.Substring(0, ws);
            }

            string newLine =
                $"{indent}{{ {nextId}, {cmdToken}, INHERIT, NONE, \"{classUpper}\", CMDNAME_{ReplacementsDictionary["$safeitemname$"]}, ITEMLISTID_PlacementSettings }}";

            // Insert after last entry
            if (lastEntryIndex >= 0)
                lines.Insert(lastEntryIndex + 1, newLine);
            else
                lines.Insert(1, newLine); // empty table case fallback

            // Rebuild block
            string newBlock = string.Join(nl, lines);

            text = text.Substring(0, openBrace) + newBlock + text.Substring(closeBrace);

            ReplaceAllText(td, text);
        }

        private void UpdatePlacementToolSettingsCppFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveAllDirtyRunningDocuments();

            string safeProjectName = ReplacementsDictionary["$safeprojectname$"];

            PatchRequiredFile("KeyinHandlers.h", InsertIntoKeyinHandlersHeader);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoKeyinHandlersIncludeCpp);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoKeyinHandlersCpp);           
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoCmdTableFile);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoCmdFile);            
            PatchRequiredFile("BentleyApi.cpp", InsertIntoBentleyApiCpp);
            PatchRequiredFile(safeProjectName + ".r", InsertIntoResourceFile);
            PatchRequiredFile(safeProjectName + ".cpp", InsertIntoMainExternCpp);
            PatchRequiredFile(safeProjectName + ".cpp", InsertIntoMainCpp);
            PatchRequiredFile(safeProjectName + "ids.h", InsertIntoIdsFile);
            PatchRequiredFile(safeProjectName + "msg.r", InsertIntoMsgFile);

            try { ActiveProject.Save(); } catch { }
        }

        #endregion

        #region MicroStation Placement Tool

        private void InsertIntoPlacementKeyinHandlersCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string methodSignature = $"void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed)";

            // Prevent duplicate
            if (text.IndexOf(methodSignature, StringComparison.Ordinal) >= 0)
                return;

            string methodText =
                $"{nl}    void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed){nl}" +
                $"    {{{nl}" +
                $"        (void)unparsed;                 // Reserved for future key-in arguments{nl}" +
                $"        start{ClassName}(unparsed);{nl}" +
                $"    }};{nl}";

            string namespaceAnchor = "namespace Commands";
            int nsPos = text.IndexOf(namespaceAnchor, StringComparison.Ordinal);
            if (nsPos < 0)
                throw new InvalidOperationException("namespace Commands not found.");

            int openBrace = text.IndexOf("{", nsPos, StringComparison.Ordinal);
            if (openBrace < 0)
                throw new InvalidOperationException("Opening brace for namespace not found.");

            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                throw new InvalidOperationException("Closing brace for namespace not found.");

            // Insert BEFORE namespace closing brace
            int insertPos = text.LastIndexOf(nl, closeBrace, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? closeBrace : insertPos + nl.Length;

            text = text.Insert(insertPos, methodText);

            ReplaceAllText(td, text);
        }

        private void UpdatePlacementToolCppFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveAllDirtyRunningDocuments();

            string safeProjectName = ReplacementsDictionary["$safeprojectname$"];

            PatchRequiredFile("KeyinHandlers.h", InsertIntoKeyinHandlersHeader);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoKeyinHandlersIncludeCpp);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoPlacementKeyinHandlersCpp);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoSelectionCmdTableFile);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoCmdFile);
            PatchRequiredFile("BentleyApi.cpp", InsertIntoBentleyApiCpp);

            try { ActiveProject.Save(); } catch { }
        }

        #endregion

        #region MicroStation Selection Tool Settings

        private void InsertIntoSelectionKeyinHandlersCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string methodSignature = $"void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed)";

            // Prevent duplicate
            if (text.IndexOf(methodSignature, StringComparison.Ordinal) >= 0)
                return;

            string methodText =
                $"{nl}    void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed){nl}" +
                $"    {{{nl}" +
                $"        (void)unparsed;                 // Reserved for future key-in arguments{nl}" +
                $"        {ClassName}::InstallNewInstance();{nl}" +
                $"    }};{nl}";

            string namespaceAnchor = "namespace Commands";
            int nsPos = text.IndexOf(namespaceAnchor, StringComparison.Ordinal);
            if (nsPos < 0)
                throw new InvalidOperationException("namespace Commands not found.");

            int openBrace = text.IndexOf("{", nsPos, StringComparison.Ordinal);
            if (openBrace < 0)
                throw new InvalidOperationException("Opening brace for namespace not found.");

            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                throw new InvalidOperationException("Closing brace for namespace not found.");

            // Insert BEFORE namespace closing brace
            int insertPos = text.LastIndexOf(nl, closeBrace, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? closeBrace : insertPos + nl.Length;

            text = text.Insert(insertPos, methodText);

            ReplaceAllText(td, text);
        }

        private void InsertIntoSelectionCmdTableFile(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string projectUpper = ReplacementsDictionary["$safeprojectname_UPPER$"];
            string classUpper = ReplacementsDictionary["$safeitemname_UPPER$"];

            string cmdToken = $"CMD_{projectUpper}_{classUpper}";

            // Prevent duplicate
            if (text.IndexOf(cmdToken, StringComparison.Ordinal) >= 0)
                return;

            string header = "CommandTable CT_COMMANDS";
            int headerPos = text.IndexOf(header, StringComparison.Ordinal);
            if (headerPos < 0)
                throw new InvalidOperationException("CT_COMMANDS table not found.");

            int openBrace = text.IndexOf("{", headerPos, StringComparison.Ordinal);
            if (openBrace < 0)
                return;

            // Find matching closing brace
            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                return;

            string block = text.Substring(openBrace, closeBrace - openBrace);

            var lines = block.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            int lastEntryIndex = -1;
            int maxId = 0;

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i].Trim();

                if (line.StartsWith("{"))
                {
                    var parts = line
                        .Trim('{', '}', ',', ' ')
                        .Split(',');

                    if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int id))
                    {
                        if (id >= maxId)
                        {
                            maxId = id;
                            lastEntryIndex = i;
                        }
                    }
                }
            }

            int nextId = maxId + 1;

            // Ensure previous line ends with comma
            if (lastEntryIndex >= 0)
            {
                if (!lines[lastEntryIndex].TrimEnd().EndsWith(","))
                {
                    lines[lastEntryIndex] = lines[lastEntryIndex].TrimEnd() + ",";
                }
            }

            // Capture indentation from last entry
            string indent = "    ";
            if (lastEntryIndex >= 0)
            {
                string original = lines[lastEntryIndex];
                int ws = original.TakeWhile(c => c == ' ' || c == '\t').Count();
                indent = original.Substring(0, ws);
            }

            string newLine =
                $"{indent}{{ {nextId}, {cmdToken}, INHERIT, NONE, \"{classUpper}\" }}";

            // Insert after last entry
            if (lastEntryIndex >= 0)
                lines.Insert(lastEntryIndex + 1, newLine);
            else
                lines.Insert(1, newLine); // empty table case fallback

            // Rebuild block
            string newBlock = string.Join(nl, lines);

            text = text.Substring(0, openBrace) + newBlock + text.Substring(closeBrace);

            ReplaceAllText(td, text);
        }        

        private void UpdateSelectionToolCppFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveAllDirtyRunningDocuments();

            string safeProjectName = ReplacementsDictionary["$safeprojectname$"];

            PatchRequiredFile("KeyinHandlers.h", InsertIntoKeyinHandlersHeader);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoKeyinHandlersIncludeCpp);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoSelectionKeyinHandlersCpp);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoSelectionCmdTableFile);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoCmdFile);
            PatchRequiredFile("BentleyApi.cpp", InsertIntoBentleyApiCpp);            

            try { ActiveProject.Save(); } catch { }
        }

        #endregion

        #region MicroStation Selection Set Tool Settings

        private void InsertIntoSelectionSetKeyinHandlersCpp(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(item);
            if (td == null)
                throw new InvalidOperationException(item.Name + " is not a text document.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            string methodSignature = $"void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed)";

            // Prevent duplicate
            if (text.IndexOf(methodSignature, StringComparison.Ordinal) >= 0)
                return;

            string methodText =
                $"{nl}    void KeyinHandlers::{ClassName}(Bentley::WCharCP unparsed){nl}" +
                $"    {{{nl}" +
                $"        (void)unparsed;                 // Reserved for future key-in arguments{nl}" +
                $"        {ClassName}::Run();{nl}" +
                $"    }};{nl}";

            string namespaceAnchor = "namespace Commands";
            int nsPos = text.IndexOf(namespaceAnchor, StringComparison.Ordinal);
            if (nsPos < 0)
                throw new InvalidOperationException("namespace Commands not found.");

            int openBrace = text.IndexOf("{", nsPos, StringComparison.Ordinal);
            if (openBrace < 0)
                throw new InvalidOperationException("Opening brace for namespace not found.");

            int depth = 0;
            int closeBrace = -1;

            for (int i = openBrace; i < text.Length; i++)
            {
                if (text[i] == '{') depth++;
                else if (text[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                    {
                        closeBrace = i;
                        break;
                    }
                }
            }

            if (closeBrace < 0)
                throw new InvalidOperationException("Closing brace for namespace not found.");

            // Insert BEFORE namespace closing brace
            int insertPos = text.LastIndexOf(nl, closeBrace, StringComparison.Ordinal);
            insertPos = (insertPos < 0) ? closeBrace : insertPos + nl.Length;

            text = text.Insert(insertPos, methodText);

            ReplaceAllText(td, text);
        }

        private void UpdateSelectionSetToolCppFiles()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SaveAllDirtyRunningDocuments();

            string safeProjectName = ReplacementsDictionary["$safeprojectname$"];

            PatchRequiredFile("KeyinHandlers.h", InsertIntoKeyinHandlersHeader);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoKeyinHandlersIncludeCpp);
            PatchRequiredFile("KeyinHandlers.cpp", InsertIntoSelectionSetKeyinHandlersCpp);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoSelectionCmdTableFile);
            PatchRequiredFile(safeProjectName + "cmd.r", InsertIntoCmdFile);
            PatchRequiredFile("BentleyApi.cpp", InsertIntoBentleyApiCpp);

            try { ActiveProject.Save(); } catch { }
        }

        #endregion

        private void PatchRequiredFile(string fileName, Action<ProjectItem> patchAction)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItem item = FindProjectItemRecursive(ActiveProject.ProjectItems, fileName);
            if (item == null)
                throw new WizardCancelledException("Required project file not found: " + fileName);

            EnsureOpen(item);
            SaveProjectItemDocument(item);
            patchAction(item);
            SaveProjectItemDocument(item);
        }

        private ProjectItem FindProjectItemRecursive(ProjectItems items, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (items == null)
                return null;

            foreach (ProjectItem item in items)
            {
                if (string.Equals(item.Name, fileName, StringComparison.OrdinalIgnoreCase))
                    return item;

                ProjectItem child = FindProjectItemRecursive(item.ProjectItems, fileName);
                if (child != null)
                    return child;
            }

            return null;
        }

        public void ReplacementProjectItem(Dictionary<string, string> replacementsDictionary)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ReplacementsDictionary = replacementsDictionary;

            string projectName = null;

            // Prefer built-in if present
            if (ReplacementsDictionary.TryGetValue("$safeprojectname$", out var safeProjectName) &&
                !string.IsNullOrWhiteSpace(safeProjectName))
            {
                projectName = safeProjectName;
            }
            else
            {
                // Add Item fallback: derive from the current project
                Dte = (DTE)ServiceProvider.GlobalProvider.GetService(typeof(DTE));

                Array activeProjects = (Array)Dte.ActiveSolutionProjects;
                if (activeProjects != null && activeProjects.Length > 0)
                {
                    var activeProject = (Project)activeProjects.GetValue(0);
                    projectName = activeProject?.Name;
                }
            }

            if (!string.IsNullOrWhiteSpace(projectName))
            {
                projectName = projectName
                    .Replace(" ", "_")
                    .Replace("-", "_")
                    .Replace("+", "_");

                ReplacementsDictionary["$safeprojectname$"] = projectName;
                ReplacementsDictionary["$safeprojectname_UPPER$"] = projectName.ToUpperInvariant();
            }

            ReplacementsDictionary["$rootnamespace_UPPER$"] =
                ReplacementsDictionary["$rootnamespace$"].ToUpperInvariant();

            ReplacementsDictionary["$safeitemname_UPPER$"] =
                ReplacementsDictionary["$safeitemname$"].ToUpperInvariant();
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (IsNewItem)
            {
                try
                {
                    foreach (ProjectItem item in ActiveProject.ProjectItems)
                    {
                        if (item.Name == filePath)
                            return false;
                    }
                }
                catch { }
            }
            return true;
        }

        #endregion

        #region Helper Methods (existing)

        private void ProcessProjectItem()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string safeItemName = ReplacementsDictionary.ContainsKey("$safeitemname$")
                ? (ReplacementsDictionary["$safeitemname$"] ?? "").ToLowerInvariant()
                : string.Empty;

            string templateName = ProjectFileInfo?.Name?.ToLowerInvariant() ?? string.Empty;

            bool isScanServiceTemplate =
                templateName == "scanservice.vstemplate" ||
                templateName == "scanservice.cpptitem";

            if (isScanServiceTemplate)
            {
                EnsureScanCriteriaHelpersHeaderExists();
            }

            // Singleton: Utilities
            if (templateName == "utilities.vstemplate" ||
                safeItemName.Contains("utilities"))
            {
                if (ProjectContainsFile(ActiveProject.ProjectItems, "Utilities.cpp") ||
                    ProjectContainsFile(ActiveProject.ProjectItems, "Utilities.h"))
                {
                    CancelWizard("Utilities already exists in this project.");
                }

                return;
            }

            // Singleton: ModuleVersion
            if (templateName == "moduleversion.vstemplate" ||
                safeItemName.Contains("moduleversion"))
            {
                if (ProjectContainsFile(ActiveProject.ProjectItems, "ModuleVersion.cpp") ||
                    ProjectContainsFile(ActiveProject.ProjectItems, "ModuleVersion.h"))
                {
                    CancelWizard("Module Version already exists in this project.");
                }

                return;
            }

            // Singleton: UIHelper
            if (templateName == "uihelpers.vstemplate" ||
                safeItemName.Contains("uiHelpers"))
            {
                if (ProjectContainsFile(ActiveProject.ProjectItems, "UIHelpers.h"))
                {
                    CancelWizard("UIHelper already exists in this project.");
                }

                return;
            }

            // Singleton: Scan Criteria Helpers
            if (templateName == "scancriteriahelpers.vstemplate" ||
                safeItemName.Contains("scancriteriahelpers"))
            {
                if (ProjectContainsFile(ActiveProject.ProjectItems, "scancriteriahelpers.h"))
                {
                    CancelWizard("ScanCriteriaHelpers already exists in this project.");
                }

                return;
            }

            // Existing commands.xml / keyin logic
            foreach (ProjectItem projectItem in ActiveProject.ProjectItems)
            {
                if ((projectItem.Name ?? "").ToLowerInvariant() == "commands.xml")
                {
                    if (safeItemName.Contains("keyincommands"))
                    {
                        MessageBox.Show(
                            Properties.Resources.CommandTableError,
                            Properties.Resources.ErrorTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        IsCommandsXmlAllowed = false;
                        throw new WizardCancelledException("KeyinCommands already exists.");
                    }
                    else
                    {
                        if (!safeItemName.Contains("scancriteriaextension"))
                        {
                            DialogResult response = MessageBox.Show(
                                Properties.Resources.AddItem,
                                Properties.Resources.AddItemMessageTitle,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1);

                            if (response == DialogResult.Yes)
                            {
                                IsItemAddedToKeyins = true;
                            }
                        }
                        break;
                    }
                }                

                if ((projectItem.Name ?? "").ToLowerInvariant().Contains("scancriteriaextensions"))
                {
                    if (safeItemName.Contains("scancriteriaextensions"))
                    {
                        MessageBox.Show(
                            Properties.Resources.ScanCriteriaExtensionMsg,
                            Properties.Resources.ErrorTitle,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);

                        IsCommandsXmlAllowed = false;
                        throw new WizardCancelledException("ScanCriteriaExtensions already exists.");
                    }
                    break;
                }                
            }

            //foreach (ProjectItem projectItem in ActiveProject.ProjectItems)
            //{
            //    if ((projectItem.Name ?? "").ToLowerInvariant().Contains("scancriteriaextensions"))
            //    {
            //        if (safeItemName.Contains("scancriteriaextensions"))
            //        {
            //            MessageBox.Show(
            //                Properties.Resources.ScanCriteriaMsg,
            //                Properties.Resources.ErrorTitle,
            //                MessageBoxButtons.OK,
            //                MessageBoxIcon.Error);

            //            IsCommandsXmlAllowed = false;
            //            throw new WizardCancelledException("ScanCriteriaExtensions already exists.");
            //        }
            //        break;
            //    }
            //}
        }

        private void EnsureScanCriteriaHelpersHeaderExists()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (ProjectContainsFile(ActiveProject.ProjectItems, "ScanCriteriaHelpers.h"))
                return;

            string sourceHeader = System.IO.Path.Combine(
                GetAssemblyPath,
                "ItemTemplates",
                "VC",
                "Bentley",
                "Native C++",
                "1033",
                "ScanCriteriaHelpers",
                "ScanCriteriaHelpers.h");

            if (!System.IO.File.Exists(sourceHeader))
            {
                MessageBox.Show(
                    "ScanCriteriaHelpers.h is required, but the source file could not be found.\n\n" +
                    sourceHeader,
                    Properties.Resources.ErrorTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                throw new WizardCancelledException("Required ScanCriteriaHelpers.h not found.");
            }

            string projectDir = System.IO.Path.GetDirectoryName(ActiveProject.FullName);
            if (string.IsNullOrWhiteSpace(projectDir))
                throw new WizardCancelledException("Could not determine project directory.");

            string targetHeader = System.IO.Path.Combine(projectDir, "ScanCriteriaHelpers.h");

            try
            {
                if (!System.IO.File.Exists(targetHeader))
                {
                    string content = System.IO.File.ReadAllText(sourceHeader);

                    content = ApplyReplacements(content);

                    System.IO.File.WriteAllText(targetHeader, content);
                }

                if (!ProjectContainsFile(ActiveProject.ProjectItems, "ScanCriteriaHelpers.h"))
                    ActiveProject.ProjectItems.AddFromFile(targetHeader);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to add ScanCriteriaHelpers.h.\n\n" + ex,
                    "Wizard Debug",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                throw new WizardCancelledException("Failed to add required ScanCriteriaHelpers.h.");
            }
        }

        private string ApplyReplacements(string content)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            foreach (var kvp in ReplacementsDictionary)
            {
                if (!string.IsNullOrEmpty(kvp.Key))
                {
                    content = content.Replace(kvp.Key, kvp.Value ?? string.Empty);
                }
            }

            if (content.Contains("$rootnamespace$"))
                content = content.Replace("$rootnamespace$", RootNamespace ?? string.Empty);

            return content;
        }

        private void CancelWizard(string message)
        {
            MessageBox.Show(
                message,
                Properties.Resources.ErrorTitle,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

            throw new WizardCancelledException(message);
        }

        private bool ProjectContainsFile(ProjectItems items, string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (items == null)
                return false;

            foreach (ProjectItem item in items)
            {
                if (string.Equals(item.Name, fileName, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    if (ProjectContainsFile(item.ProjectItems, fileName))
                        return true;
                }
            }

            return false;
        }
        private void ProcessProject()
        {
            if (ProjectFileInfo.Name == "Addincpp.vstemplate" || ProjectFileInfo.Name == "Keyincpp.vstemplate")
            {
                ProcessResources();
                ProcessEmbeddedResource();
            }
            ReplacementsDictionary["$ToolVersion$"] = Version;            
            ReplacementsDictionary["$AddinAttribute$"] = AddAddinAttribute();
            ReplacementsDictionary["$reference$"] = AddReferences();
        }

        private void ProcessEmbeddedResource()
        {
            string[] embeddedResources = Assembly.GetAssembly(typeof(IWizardImplementation)).GetManifestResourceNames();

            foreach (string resource in embeddedResources)
            {
                string bentleyApp = BentleyApp;
                string result = "";

                if (bentleyApp.Contains("OpenRoads") || bentleyApp.Contains("OpenRail"))
                {
                    if (resource.Contains("{F7AD6FF5-34E0-4FBE-B5CF}"))
                        result = GetSubstituteResult("VSTMC.Wizard.EmbeddedResources.MicroStationUtilities.{F7AD6FF5-34E0-4FBE-B5CF-ORD}.cpp");
                    else if (resource.Contains("{FAD22EAD-82A1-4DB6-9718-B91226D9A42A}"))
                        result = GetSubstituteResult("VSTMC.Wizard.EmbeddedResources.native.{FAD22EAD-82A1-4DB6-9718-B91226D9-ORD}.h");
                }
                else
                {
                    if (resource.Contains("{F7AD6FF5-34E0-4FBE-B5CF}"))
                        result = GetSubstituteResult("VSTMC.Wizard.EmbeddedResources.MicroStationUtilities.{F7AD6FF5-34E0-4FBE-B5CF}.cpp");
                    else if (resource.Contains("{FAD22EAD-82A1-4DB6-9718-B91226D9A42A}"))
                        result = GetSubstituteResult("VSTMC.Wizard.EmbeddedResources.native.{FAD22EAD-82A1-4DB6-9718-B91226D9A42A}.h");
                }

                string[] resourceItemCollection = resource.Split('.');
                foreach (var resourceItem in resourceItemCollection)
                {
                    if (resourceItem.Contains("{"))
                    {
                        var key = $"${resourceItem}$";
                        ReplacementsDictionary[key] = result;
                    }
                }
            }
        }

        private void ProcessResources()
        {                        
            AddFile(GetAssemblyPath + "\\Resources\\KeyinTree.xsd", GetVisualStudioPath + "\\xml\\Schemas\\KeyinTree.xsd");
            AddFile(GetAssemblyPath + "\\Resources\\RibbonDefinitions.xsd", GetVisualStudioPath + "\\xml\\Schemas\\RibbonDefinitions.xsd");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\AECOsim.exe.bat",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\AECOsim.exe.bat");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\MicroStation.exe.bat",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\MicroStation.exe.bat");
            AddFile(GetAssemblyPath + "\\ExampleBatchFiles\\OpenRoadsDesigner.exe.bat",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\OpenRoadsDesigner.exe.bat");
            AddFile(GetAssemblyPath + "\\include\\Declare_ustnTaskId_Import.h",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\include\Declare_ustnTaskId_Import.h");
            AddFile(GetAssemblyPath + "\\include\\std_collection_typedefs.h",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\include\std_collection_typedefs.h");
            AddFile(GetAssemblyPath + "\\include\\tstring_typedef.h",
                Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\VSTMC\include\tstring_typedef.h");
        }

        private string GetSubstituteResult(string resource)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream(resource))
            {
                if (stream == null)
                {
                    string available = string.Join(Environment.NewLine, assembly.GetManifestResourceNames());
                    throw new InvalidOperationException(
                        $"Embedded resource not found: {resource}{Environment.NewLine}{Environment.NewLine}Available resources:{Environment.NewLine}{available}");
                }

                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        #endregion

        #region SAVE + BUFFER-FIRST EDITING

        private static void SaveAllDirtyRunningDocuments()
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();

                var rdt = (IVsRunningDocumentTable)ServiceProvider.GlobalProvider.GetService(typeof(SVsRunningDocumentTable));
                if (rdt == null) return;

                rdt.SaveDocuments(
                    (uint)__VSRDTSAVEOPTIONS.RDTSAVEOPT_SaveIfDirty,
                    pHier: null,
                    itemid: VSConstants.VSITEMID_NIL,
                    docCookie: 0);
            }
            catch
            {
                // swallow in wizard
            }
        }

        private static void SaveProjectItemDocument(ProjectItem item)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                var doc = item?.Document;
                doc?.Save();
            }
            catch { }
        }

        private static void EnsureOpen(ProjectItem item)
        {
            try
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (item != null && !item.IsOpen)
                    item.Open(EnvDTE.Constants.vsViewKindCode);
            }
            catch { }
        }

        private static TextDocument GetTextDocument(ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var doc = item?.Document;
            if (doc == null) return null;

            try
            {
                return (TextDocument)doc.Object("TextDocument");
            }
            catch
            {
                return null;
            }
        }

        private static string GetAllText(TextDocument td)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var start = td.StartPoint.CreateEditPoint();
            return start.GetText(td.EndPoint);
        }

        private static void ReplaceAllText(TextDocument td, string newText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var start = td.StartPoint.CreateEditPoint();
            start.Delete(td.EndPoint);
            start.Insert(newText);
        }

        private static string DetectNewline(string text)
            => text != null && text.Contains("\r\n") ? "\r\n" : "\n";

        private static string NormalizeNewlines(string s)
            => (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n");

        private static string GetIndentOfLine(EditPoint p)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            EditPoint t = p.CreateEditPoint();
            t.StartOfLine();
            string line = t.GetLines(t.Line, t.Line + 1);

            int i = 0;
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) i++;
            return line.Substring(0, i);
        }

        private static void InsertChildLineBeforeCurrentLine(EditPoint p, string lineToInsert, int extraChildTabs)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            string nl = Environment.NewLine;

            // Capture indentation of the current line (closing tag line)
            string closingIndent = GetIndentOfLine(p);

            // Indent child 1 tab deeper (or more)
            string childIndent = closingIndent + new string('\t', Math.Max(0, extraChildTabs));

            // Ensure we insert at start of the closing tag line
            p.StartOfLine();
            p.Insert(childIndent + lineToInsert + nl);
        }

        private static bool BufferContains(TextDocument td, string needle, StringComparison comparison = StringComparison.Ordinal)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string text = GetAllText(td);
            return text?.IndexOf(needle, comparison) >= 0;
        }

        #endregion

        #region AddCommand (UPDATED)

        private void AddCommand()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // 1) Flush dirty buffers FIRST.
            SaveAllDirtyRunningDocuments();

            // 2) Find target files recursively.
            ProjectItem commandsXmlItem = null;
            ProjectItem keyinCsItem = null;
            ProjectItem keyinVbItem = null;
            ProjectItem keyinCppItem = null;

            foreach (ProjectItem item in EnumerateProjectItemsRecursive(ActiveProject.ProjectItems))
            {
                string name = (item.Name ?? "").ToLowerInvariant();
                if (name == "commands.xml") commandsXmlItem = item;
                else if (name == "keyincommands.cs") keyinCsItem = item;
                else if (name == "keyincommands.vb") keyinVbItem = item;
                else if (name == "keyincommands.cpp") keyinCppItem = item;
            }

            // 3) Build method bodies
            BuildKeyinBodies(out string csBody, out string vbBody, out string cppBody);

            // 4) commands.xml edits
            if (commandsXmlItem != null)
            {
                EnsureOpen(commandsXmlItem);
                SaveProjectItemDocument(commandsXmlItem);

                ModifyCommandsXml_BufferFirst(commandsXmlItem);

                SaveProjectItemDocument(commandsXmlItem);
            }

            // 5) KeyinCommands.cs edits (your existing region-based method)
            if (keyinCsItem != null)
            {
                EnsureOpen(keyinCsItem);
                SaveProjectItemDocument(keyinCsItem);

                ModifyKeyinCommandsCs_BufferFirst(keyinCsItem, FunctionName, csBody);
                
                SaveProjectItemDocument(keyinCsItem);
            }

            // 6) KeyinCommands.vb edits (UPDATED to use matching #End Region)
            if (keyinVbItem != null)
            {
                EnsureOpen(keyinVbItem);
                SaveProjectItemDocument(keyinVbItem);

                ModifyKeyinCommandsVb_BufferFirst(keyinVbItem, FunctionName, vbBody);

                SaveProjectItemDocument(keyinVbItem);
            }

            _ = keyinCppItem;
            _ = cppBody; // no-op per your requirement
        }

        #endregion

        #region Commands.xml buffer-first (UPDATED to your markers)

        private void ModifyCommandsXml_BufferFirst(ProjectItem commandsXmlItem)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (!commandsXmlItem.IsOpen)
                commandsXmlItem.Open(EnvDTE.Constants.vsViewKindCode);

            var td = GetTextDocument(commandsXmlItem);
            if (td == null) throw new InvalidOperationException("commands.xml is not a TextDocument.");

            // Keep your current keyin spacing behavior
            string keyin = $"{RootNamespace}  {ClassName}";

            string keywordLine = $"<Keyword CommandWord=\"{ClassName}\"> </Keyword>";

            // ✅ REMOVED Keyin postfix here:
            string handlerLine = $"<KeyinHandler Keyin=\"{keyin}\" Function=\"{RootNamespace}.KeyinCommands.{FunctionName}\" />";

            // Dedupe
            if (!BufferContains(td, $"CommandWord=\"{ClassName}\"", StringComparison.OrdinalIgnoreCase))
            {
                InsertKeywordBeforeKeyinTableClose(commandsXmlItem, "<!-- VSTMC:KEYWORDS_INSERT -->", keywordLine);
            }

            // ✅ Dedupe needle changed to match new Function attribute
            if (!BufferContains(td, $"Function=\"{RootNamespace}.KeyinCommands.{FunctionName}\"", StringComparison.OrdinalIgnoreCase))
            {
                InsertHandlerBeforeKeyinHandlersClose(commandsXmlItem, handlerLine);
            }

            commandsXmlItem.Document?.Save();
        }

        // KEYWORDS:
        // Find marker <!-- VSTMC:KEYWORDS_INSERT -->, then find next </KeyinTable>, insert keyword ABOVE that closing tag.
        private static void InsertKeywordBeforeKeyinTableClose(ProjectItem commandsXmlItem, string marker, string keywordLine)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var doc = commandsXmlItem.Document;
            if (doc == null) throw new InvalidOperationException("commands.xml document not available.");

            var textDoc = (TextDocument)doc.Object("TextDocument");
            EditPoint p = textDoc.StartPoint.CreateEditPoint();

            if (!p.FindPattern(marker, (int)vsFindOptions.vsFindOptionsNone))
                throw new InvalidOperationException($"Marker not found: {marker}");

            if (!p.FindPattern("</KeyinTable>", (int)vsFindOptions.vsFindOptionsNone))
                throw new InvalidOperationException("Could not find </KeyinTable> after keywords marker.");

            InsertChildLineBeforeCurrentLine(p, keywordLine, extraChildTabs: 1);
        }

        private static void InsertHandlerBeforeKeyinHandlersClose(ProjectItem commandsXmlItem, string handlerLine)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var doc = commandsXmlItem.Document;
            if (doc == null) throw new InvalidOperationException("commands.xml document not available.");

            var textDoc = (TextDocument)doc.Object("TextDocument");
            EditPoint p = textDoc.StartPoint.CreateEditPoint();

            if (!p.FindPattern("</KeyinHandlers>", (int)vsFindOptions.vsFindOptionsNone))
                throw new InvalidOperationException("Closing tag not found: </KeyinHandlers>");

            // Insert BEFORE </KeyinHandlers> (indent one level deeper than closing tag)
            InsertChildLineBeforeCurrentLine(p, handlerLine, extraChildTabs: 1);

            doc.Save();
        }


        #endregion

        #region KeyinCommands.cs buffer-first (your existing region append)

        private void ModifyKeyinCommandsCs_BufferFirst(ProjectItem keyinCsItem, string fnName, string body)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(keyinCsItem);
            if (td == null)
                throw new InvalidOperationException("KeyinCommands.cs is not a TextDocument.");

            // ✅ Dedupe WITHOUT Keyin postfix
            string signature = $"public static void {fnName}(string unparsed)";
            if (BufferContains(td, signature, StringComparison.Ordinal))
                return;

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            const string RegionStartLine = "#region VSTMC GENERATED KEYINS (do not remove this region)";
            int insertPos = FindInsertPosBeforeMatchingEndRegion(text, RegionStartLine, nl);

            string indentedBody = NormalizeNewlines(body)
                .Replace("\n", nl + "            ")
                .TrimEnd();

            // ✅ Method WITHOUT Keyin postfix
            string method =
                nl +
                "        public static void " + fnName + "(string unparsed = \"\")" + nl +
                "        {" + nl +
                "            " + indentedBody + nl +
                "        }" + nl;

            text = text.Insert(insertPos, method);
            ReplaceAllText(td, text);
        }

        private static int FindInsertPosBeforeMatchingEndRegion(string text, string regionStartLineContains, string newline)
        {
            int startIdx = text.IndexOf(regionStartLineContains, StringComparison.Ordinal);
            if (startIdx < 0)
                throw new InvalidOperationException($"Region start not found: {regionStartLineContains}");

            int i = text.LastIndexOf(newline, startIdx, StringComparison.Ordinal);
            i = (i < 0) ? 0 : i + newline.Length;

            int depth = 0;
            bool started = false;

            while (i < text.Length)
            {
                int j = text.IndexOf(newline, i, StringComparison.Ordinal);
                if (j < 0) j = text.Length;

                string line = text.Substring(i, j - i).TrimStart();

                if (line.StartsWith("#region", StringComparison.Ordinal))
                {
                    depth++;
                    started = true;
                }
                else if (line.StartsWith("#endregion", StringComparison.Ordinal))
                {
                    if (started)
                    {
                        depth--;
                        if (depth == 0)
                            return i; // insert before this #endregion line
                    }
                }

                i = j + newline.Length;
            }

            throw new InvalidOperationException("Matching #endregion not found for VSTMC region.");
        }

        #endregion

        #region KeyinCommands.vb buffer-first (UPDATED: uses matching #End Region)

        private void ModifyKeyinCommandsVb_BufferFirst(ProjectItem keyinVbItem, string fnName, string body)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var td = GetTextDocument(keyinVbItem);
            if (td == null)
                throw new InvalidOperationException("KeyinCommands.vb is not a TextDocument.");

            string text = GetAllText(td);
            string nl = DetectNewline(text);

            // ✅ Dedupe WITHOUT Keyin postfix
            // (match "Public Shared Sub Class2(")
            if (text.IndexOf($"Public Shared Sub {fnName}(", StringComparison.OrdinalIgnoreCase) >= 0)
                return;

            const string RegionStartLine = "#Region \"VSTMC GENERATED KEYINS (Do Not remove this region)\"";
            int insertPos = FindInsertPosBeforeMatchingEndRegionVb(text, RegionStartLine, nl);

            string indentedBody = NormalizeNewlines(body)
                .Replace("\n", nl + "        ")
                .TrimEnd();

            // ✅ Method WITHOUT Keyin postfix
            string method =
                nl +
                "    Public Shared Sub " + fnName + "(unparsed As String)" + nl +
                "        " + indentedBody + nl +
                "    End Sub" + nl;

            text = text.Insert(insertPos, method);
            ReplaceAllText(td, text);
        }

        private static int FindInsertPosBeforeMatchingEndRegionVb(string text, string regionStartLineContains, string newline)
        {
            int startIdx = text.IndexOf(regionStartLineContains, StringComparison.OrdinalIgnoreCase);
            if (startIdx < 0)
                throw new InvalidOperationException($"Region start not found: {regionStartLineContains}");

            int i = text.LastIndexOf(newline, startIdx, StringComparison.Ordinal);
            i = (i < 0) ? 0 : i + newline.Length;

            int depth = 0;
            bool started = false;

            while (i < text.Length)
            {
                int j = text.IndexOf(newline, i, StringComparison.Ordinal);
                if (j < 0) j = text.Length;

                string rawLine = text.Substring(i, j - i);
                string line = rawLine.TrimStart();

                if (line.StartsWith("#Region", StringComparison.OrdinalIgnoreCase))
                {
                    depth++;
                    started = true;
                }
                else if (line.StartsWith("#End Region", StringComparison.OrdinalIgnoreCase))
                {
                    if (started)
                    {
                        depth--;
                        if (depth == 0)
                            return i; // insert BEFORE this matching "#End Region"
                    }
                }

                i = j + newline.Length;
            }

            throw new InvalidOperationException("Matching #End Region not found for VSTMC region.");
        }

        #endregion

        #region “Missing snippet connections” restored (your existing body builder)

        private void BuildKeyinBodies(out string keyinCommandFunctionCS, out string keyinCommandFunctionVB, out string keyinCommandFunctionCPP)
        {
            keyinCommandFunctionCS = "";
            keyinCommandFunctionVB = "";
            keyinCommandFunctionCPP = "";

            //string className = ReplacementsDictionary["$safeitemname$"];

            switch (GetItemTypeFromTemplate)
            {
                case "MicroStation Selection Tool":
                    keyinCommandFunctionCS =
                        $"{ClassName}.InstallNewInstance(unparsed);";
                    keyinCommandFunctionVB =
                        $"{ClassName}.InstallNewInstance(unparsed)";
                    break;

                case "MicroStation Selection Tool Settings":
                case "MicroStation Selection Tool Settings WPF":
                case "MicroStation Placement Tool Settings":
                case "MicroStation Placement Tool Settings WPF":
                    keyinCommandFunctionCS = AddFunctionCode2(ClassName, "tool", "InstallNewInstance(unparsed)", ProjectLanguage.CS);
                    keyinCommandFunctionVB = AddFunctionCode2(ClassName, "tool", "InstallNewInstance(unparsed)", ProjectLanguage.VB);
                    break;

                case "MicroStation IPrimitive Tool Settings":
                case "MicroStation IPrimitive Tool Settings WPF":
                    keyinCommandFunctionCS = AddFunctionCode(ClassName, "tool", "Run(unparsed)", ProjectLanguage.CS);
                    keyinCommandFunctionVB = AddFunctionCode(ClassName, "tool", "Run(unparsed)", ProjectLanguage.VB);
                    break;

                //case "MicroStation Placement Tool":
                //    keyinCommandFunctionCS =
                //        $"new {FunctionName}(0, 0).InstallNewInstance(unparsed);";
                //    keyinCommandFunctionVB =
                //        AddFunctionCode2(FunctionName, "tool", "InstallNewInstance(unparsed)", ProjectLanguage.VB);
                //    break;

                case "MicroStation Placement Tool":
                    keyinCommandFunctionCS =
                        $"{ClassName}.InstallNewInstance(unparsed);";
                    keyinCommandFunctionVB =
                        $"{ClassName}.InstallNewInstance(unparsed)";
                    break;

                case "MicroStation Tool Settings WPF":
                    keyinCommandFunctionCS = AddFunctionCode(ClassName, "tool", "ShowWindow(Program.Instance, unparsed)", ProjectLanguage.CS);
                    keyinCommandFunctionVB = AddFunctionCode(ClassName, "tool", "ShowWindow(Program.Instance, unparsed)", ProjectLanguage.VB);
                    break;

                case "MicroStation Tool Settings":
                    keyinCommandFunctionCS = AddFunctionCode(ClassName, "tool", "ShowForm(Program.Instance, unparsed)", ProjectLanguage.CS);
                    keyinCommandFunctionVB = AddFunctionCode(ClassName, "tool", "ShowForm(Program.Instance, unparsed)", ProjectLanguage.VB);
                    break;

                case "MicroStation Toolbar WPF":
                    keyinCommandFunctionCS = $"{ClassName}.ShowToolbar(unparsed);"; //AddFunctionCode(ClassName, "toolbar", "ShowToolbar(unparsed)", ProjectLanguage.CS);
                    keyinCommandFunctionVB = AddFunctionCode(ClassName, "toolbar", "ShowToolbar(unparsed)", ProjectLanguage.VB);
                    break;

                case "MicroStation Windows Form":
                    {
                        keyinCommandFunctionCS = $"new {ClassName}().ShowForm(unparsed);"; //AddFunctionCode(FunctionName, "form", "ShowForm(unparsed)", ProjectLanguage.CS);
                        keyinCommandFunctionCPP = FunctionName + "::ShowForm(Program::MSAddin);";
                        keyinCommandFunctionVB = "Dim form As " + ClassName + " = New " + ClassName + "\n" + "form.ShowForm(unparsed)";
                        break;
                    }
                case "MicroStation UserControl (WPF)":
                    {
                        keyinCommandFunctionCS = $"{ClassName}.ShowWindow(unparsed);"; // AddFunctionCode(ClassName, "usercontrol", "ShowWindow(unparsed)", ProjectLanguage.CS);
                        keyinCommandFunctionVB = AddFunctionCode(ClassName, "usercontrol", "ShowWindow(unparsed)", ProjectLanguage.VB);
                        break;
                    }
                case "MicroStation Class":
                    {
                        string varName = ClassName.ToLowerInvariant();

                        keyinCommandFunctionCS =
                            $"new {ClassName}(unparsed);";

                        keyinCommandFunctionVB =
                            $"Dim {varName} As {ClassName} = New {ClassName}(unparsed)";

                        break;
                    }

                default:
                    keyinCommandFunctionCS = "// TODO: implement key-in";
                    keyinCommandFunctionVB = "' TODO: implement key-in";
                    break;
            }
        }

        #endregion

        #region Enumerate items

        private static IEnumerable<ProjectItem> EnumerateProjectItemsRecursive(ProjectItems items)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (items == null) yield break;

            foreach (ProjectItem item in items)
            {
                yield return item;

                if (item.ProjectItems != null && item.ProjectItems.Count > 0)
                {
                    foreach (var child in EnumerateProjectItemsRecursive(item.ProjectItems))
                        yield return child;
                }
            }
        }

        #endregion

        #region Existing formatting helpers (kept)

        //private static string AddFunctionCode(string functionName, string obj, string method, ProjectLanguage lang)
        //{
        //    switch (lang)
        //    {
        //        case ProjectLanguage.CS:
        //            return $"{functionName} {obj} = new();\n{obj}.{method};";
        //        case ProjectLanguage.VB:
        //            return "Dim " + obj + " As " + functionName + " = New " + functionName + "\n" + obj + "." + method;
        //        default:
        //            return functionName + " " + obj + " = new " + functionName + "();\n" + obj + "." + method + ";";
        //    }
        //}

        private static string AddFunctionCode(string functionName, string obj, string method, ProjectLanguage lang)
        {
            switch (lang)
            {
                case ProjectLanguage.CS:
                    return $"new {functionName}().{method};";
                case ProjectLanguage.VB:
                    return "Dim " + obj + " As " + functionName + " = New " + functionName + "\n" + obj + "." + method;
                default:
                    return functionName + " " + obj + " = new " + functionName + "();\n" + obj + "." + method + ";";
            }
        }

        //private static string AddFunctionCode2(string functionName, string obj, string method, ProjectLanguage lang)
        //{
        //    switch (lang)
        //    {
        //        case ProjectLanguage.CS:
        //            return $"{functionName} {obj} = new(0, 0);\n{obj}.{method};";
        //        case ProjectLanguage.VB:
        //            return "Dim " + obj + " As " + functionName + " = New " + functionName + "(0, 0)\n" + obj + "." + method;
        //        default:
        //            return functionName + " " + obj + " = new " + functionName + "(0, 0);\n" + obj + "." + method + ";";
        //    }
        //}

        private static string AddFunctionCode2(string functionName, string obj, string method, ProjectLanguage lang)
        {
            switch (lang)
            {
                case ProjectLanguage.CS:
                    return $"new {functionName}(0, 0).{method};";
                case ProjectLanguage.VB:
                    return "Dim " + obj + " As " + functionName + " = New " + functionName + "(0, 0)\n" + obj + "." + method;
                default:
                    return functionName + " " + obj + " = new " + functionName + "(0, 0);\n" + obj + "." + method + ";";
            }
        }

        #endregion

        #region Existing AddFile

        private void AddFile(string source, string target)
        {
            try
            {
                if (!System.IO.File.Exists(target))
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(target)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(target));

                    System.IO.File.Copy(source, target, true);
                }
            }
            catch { }
        }

        #endregion

        #region KEEP YOUR EXISTING IMPLEMENTATIONS HERE (required for your project)

        /// <summary>
        /// Add addin attribute for project.
        /// </summary>
        /// <returns></returns>
        private string AddAddinAttribute()
        {
            string bentleyApp = BentleyApp;
            string safeprojectname = ReplacementsDictionary["$safeprojectname$"];
            if (IsCSProject)
            {
                if (bentleyApp.Contains("PowerDraft"))
                    return
                        "/// <summary>" +
                        "\r\n/// Main entry point class for this addin application." +
                        "\r\n/// When loading an AddIn MicroStation looks for a class" +
                        "\r\n/// derived from AddIn." +
                        "\r\n///Sample password shown, Request a password at https://www.bentley.com/en/software-developers/bdn-inquiry-form." +
                        "\r\n/// </summary>" +
                        "\r\n[BM.AddIn(MdlTaskID = \"" + safeprojectname + "\"," +
                        "\r\nPassword=\"0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF01234567\")]";
                else
                    return
                        "/// <summary>" +
                        "\r\n/// Main entry point class for this addin application." +
                        "\r\n/// When loading an AddIn MicroStation looks for a class" +
                        "\r\n/// derived from AddIn." +
                        "\r\n/// </summary>" +
                        "\r\n[BM.AddIn(MdlTaskID = \"" + safeprojectname + "\")]";
            }
            else if (IsVBProject)
            {
                if (bentleyApp.Contains("PowerDraft"))
                    return
                        "\r\n" +
                        "\r\n''' <summary>" +
                        "\r\n''' When loading an AddIn MicroStation looks for a class" +
                        "\r\n''' derived from AddIn." +
                        "\r\n''' Sample password shown, Request a password at https://www.bentley.com/en/software-developers/bdn-inquiry-form." +
                        "\r\n''' </summary>" +
                        "\r\n<BM.AddIn(MdlTaskID:=\"" + safeprojectname + "\", _" +
                        "\r\nPassword:=\"0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF01234567\")>";
                else
                    return
                        "\r\n" +
                        "\r\n''' <summary>" +
                        "\r\n''' When loading an AddIn MicroStation looks for a class" +
                        "\r\n''' derived from AddIn." +
                        "\r\n''' </summary>" +
                        "\r\n<BM.AddIn(MdlTaskID:=\"" + safeprojectname + "\")>";
            }
            else if (IsCPPProject)
            {
                if (bentleyApp.Contains("PowerDraft"))
                    return
                        "#define         DLM_PASSWORD(x) \\" +
                        "\r\nBEGIN_EXTERN_C \\" +
                        "\r\n     __declspec(dllexport) void            dlmPassword \\" +
                        "\r\n     ( \\" +
                        "\r\n     char    *passwordP, \\" +
                        "\r\n     WCharCP  appNameP \\" +
                        "\r\n     ) \\" +
                        "\r\n         { \\" +
                        "\r\n         strcpy (passwordP, (x)); \\" +
                        "\r\n         } \\" +
                        "\r\nEND_EXTERN_C";
                else
                    return "";
            }
            else if (IsVCProject)
            {
                if (bentleyApp.Contains("PowerDraft"))
                    return
                        "/// <summary>" +
                        "\r\n/// MicroStation looks for this class that is" +
                        "\r\n/// derived from Bentley.MicroStation.AddIn." +
                        "\r\n/// Sample Password shown. Request a password at https://www.bentley.com/en/software-developers/bdn-inquiry-form." +
                        "\r\n/// </summary>" +
                        "\r\n[Bentley::MstnPlatformNET::AddInAttribute(MdlTaskID = L\"" + safeprojectname + "\"," +
                        "\r\nPassword=L\"0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF01234567\")]";
                else
                    return
                        "/// <summary>" +
                        "\r\n/// MicroStation looks for this class that is" +
                        "\r\n/// derived from Bentley.MicroStation.AddIn." +
                        "\r\n/// </summary>" +
                        "\r\n[Bentley::MstnPlatformNET::AddInAttribute(MdlTaskID = L\"" + safeprojectname + "\")]";
            }
            return "";
        }

        /// <summary>
        /// Add assembly references for specific Bentley product projects.
        /// </summary>
        /// <returns></returns>
        public string AddReferences()
        {
            string bentleyApp = BentleyApp;
            if (bentleyApp.Contains("AECOsim"))
            {
                if (ProjectFileInfo.Name.Contains("Form") || ProjectFileInfo.Name.Contains("WPF"))
                    return
                        "    <Reference Include=\"Bentley.Interop.TFCom\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Building.Api\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Interop.STFCom\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Interop.ATFCom\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>";
                else
                    return
                        "    <Reference Include=\"Bentley.Interop.TFCom\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Building.Api\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Interop.STFCom\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.Interop.ATFCom\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>";
            }
            else if (bentleyApp.Contains("OpenPlant"))
            {
                return ProjectFileInfo.Name.Contains("Form") || ProjectFileInfo.Name.Contains("WPF")
                    ? "    <Reference Include=\"Bentley.OpenPlantModeler.SDK\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>"
                    : "    <Reference Include=\"Bentley.OpenPlantModeler.SDK\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>";
            }
            else if (bentleyApp.Contains("OpenBridge"))
            {
                return ProjectFileInfo.Name.Contains("Form") || ProjectFileInfo.Name.Contains("WPF")
                    ? "    <Reference Include=\"Bentley.ObmNET.Model\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>"
                    : "    <Reference Include=\"Bentley.ObmNET.Model\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>";
            }
            else if (bentleyApp.Contains("OpenRail"))
            {
                return ProjectFileInfo.Name.Contains("Form") || ProjectFileInfo.Name.Contains("WPF")
                    ? "    <Reference Include=\"Bentley.Civil.Rail.GeometryModel\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.CifNET.GeometryModel.SDK.4.0\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>"
                    : "    <Reference Include=\"Bentley.Civil.Rail.GeometryModel\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n\r" +
                        "    <Reference Include=\"Bentley.CifNET.GeometryModel.SDK.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.LinearGeometry.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.Geometry.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.SDK.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.Formatting.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.TerrainModelNET\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>";
            }
            else if (bentleyApp.Contains("OpenRoads"))
            {
                return ProjectFileInfo.Name.Contains("Form") || ProjectFileInfo.Name.Contains("WPF")
                    ? "    <Reference Include=\"Bentley.CifNET.GeometryModel.SDK.4.0\">\n\r" +
                        "      <Private>True</Private>\n\r" +
                        "    </Reference>"
                    : "    <Reference Include=\"Bentley.CifNET.GeometryModel.SDK.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.LinearGeometry.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.Geometry.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.SDK.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.CifNET.Formatting.4.0\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>\n" +
                        "    <Reference Include=\"Bentley.TerrainModelNET\">\n\r" +
                        "      <Private>False</Private>\n\r" +
                        "    </Reference>";
            }
            else
                return "";

        }
 
        #endregion

        #region Properties (as in your file)

        /// <summary>
        /// Determine if the project is a CSharp project. Returns true if project is a CSharp Project
        /// otherwise returns false. (Read only)
        /// </summary>
        private bool IsCSProject => GetLanguageFromTemplate == "CSharp";

        /// <summary>
        /// Determine if the project is a Visual Basic project. Returns true if project is a Visual Basic Project
        /// otherwise returns false. (Read only)
        /// </summary>
        private bool IsVBProject => GetLanguageFromTemplate == "VisualBasic";

        /// <summary>
        /// Determine if the project is a CPP project. Returns true if project is a CPP Project
        /// otherwise returns false. (Read only)
        /// </summary>
        private bool IsCPPProject => GetLanguageFromTemplate == "CPP";

        /// <summary>
        /// Determine if the project is a managed CPP project. Returns true if project is a managed CPP Project
        /// otherwise returns false. (Read only)
        /// </summary>
        private bool IsVCProject => GetLanguageFromTemplate == "VC";

        public string GetLanguageFromTemplate
        {
            get
            {
                if (ReplacementsDictionary.ContainsKey("$language$"))
                {
                    if (string.IsNullOrEmpty(ReplacementsDictionary["$language$"]))
                        return "Language not specified";
                    return ReplacementsDictionary["$language$"];
                }
                else
                    return "Dictionary does not contain language";
            }
        }

        public string GetItemTypeFromTemplate
        {
            get
            {
                if (ReplacementsDictionary.ContainsKey("$item$"))
                {
                    if (string.IsNullOrEmpty(ReplacementsDictionary["$item$"]))
                        return "Item description not specified";
                    return ReplacementsDictionary["$item$"];
                }
                else
                    return "NoKey";
            }
        }

        public System.IO.FileInfo ProjectFileInfo { get; set; }        

        private Project ActiveProject
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return (Project)ActiveSolutionProjects.GetValue(0);
            }
        }

        private Array ActiveSolutionProjects
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return (Array)Dte.ActiveSolutionProjects;
            }
        }   

        private string BentleyApp
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return Dte.get_Properties("Bentley", "MicroStation CONNECT").Item("BentleyApp").Value.ToString();
            }
        }

        private DTE Dte { get; set; }

        private string FunctionName { get; set; }

        private string ClassName { get; set; }

        private string ClassName_Upper { get; set; }

        private string SafeProjectName_Upper { get; set; }

        private bool IsCommandsXmlAllowed { get; set; }

        private bool IsItemAddedToKeyins { get; set; }

        private bool IsNewItem { get; set; }

        public string GetVisualStudioPath
        {
            get {
                ThreadHelper.ThrowIfNotOnUIThread(); 
                return System.IO.Path.GetFullPath(System.IO.Path.Combine(Dte.FullName, @"..\..\..\")); 
            }
        }

        public string GetAssemblyPath
        {
            get
            {
                return System.IO.Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
            }
        }

        public Dictionary<string, string> ReplacementsDictionary { get; set; }

        private string RootNamespace
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return ActiveProject.Properties.Item("DefaultNamespace").Value.ToString();
            }
        }

        private string Version
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                return Dte.Version.ToString();
            }
        }             

        #endregion

        #region Enumerations

        private enum ProjectLanguage
        {
            CS,
            VB,
            CPP,
            VC
        }

        #endregion
    }
}