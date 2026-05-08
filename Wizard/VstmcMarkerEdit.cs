using System;
using System.IO;
using System.Text;

namespace VSTMC.Wizard
{
    internal static class VstmcMarkerEdit
    {
        public static void ReplaceBetween(string filePath, string beginMarker, string endMarker, string newContent)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);

            int begin = text.IndexOf(beginMarker, StringComparison.Ordinal);
            int end = text.IndexOf(endMarker, StringComparison.Ordinal);

            if (begin < 0 || end < 0 || end < begin)
                throw new InvalidOperationException($"Markers not found/invalid in {filePath}: {beginMarker} ... {endMarker}");

            int insertAt = begin + beginMarker.Length;

            string before = text.Substring(0, insertAt);
            string after = text.Substring(end);

            if (!before.EndsWith("\n")) before += "\n";

            string content = newContent ?? "";
            content = NormalizeNewlines(content);
            if (content.Length > 0 && !content.EndsWith("\n")) content += "\n";

            if (!after.StartsWith("\n")) content += "\n";

            File.WriteAllText(filePath, before + content + after, Encoding.UTF8);
        }

        public static void UpsertBlockAfterAnchor(
            string filePath,
            string anchorMarker,
            string blockBeginMarker,
            string blockEndMarker,
            string blockContent)
        {
            var text = File.ReadAllText(filePath, Encoding.UTF8);

            int anchorPos = text.IndexOf(anchorMarker, StringComparison.Ordinal);
            if (anchorPos < 0)
                throw new InvalidOperationException($"Anchor marker not found in {filePath}: {anchorMarker}");

            int beginPos = text.IndexOf(blockBeginMarker, StringComparison.Ordinal);
            int endPos = text.IndexOf(blockEndMarker, StringComparison.Ordinal);

            // If block exists, replace its contents
            if (beginPos >= 0 && endPos > beginPos)
            {
                int insertAt = beginPos + blockBeginMarker.Length;
                string before = text.Substring(0, insertAt);
                string after = text.Substring(endPos);

                if (!before.EndsWith("\n")) before += "\n";

                string content = blockContent ?? "";
                content = NormalizeNewlines(content);
                if (content.Length > 0 && !content.EndsWith("\n")) content += "\n";

                if (!after.StartsWith("\n")) content += "\n";

                File.WriteAllText(filePath, before + content + after, Encoding.UTF8);
                return;
            }

            // Otherwise insert a brand new block right after the anchor line
            int lineEnd = text.IndexOf('\n', anchorPos);
            if (lineEnd < 0) lineEnd = text.Length;
            else lineEnd += 1;

            var sb = new StringBuilder();
            sb.Append(blockBeginMarker).Append('\n');

            string body = blockContent ?? "";
            body = NormalizeNewlines(body);
            if (body.Length > 0 && !body.EndsWith("\n")) body += "\n";
            sb.Append(body);

            sb.Append(blockEndMarker).Append('\n');

            text = text.Insert(lineEnd, sb.ToString());
            File.WriteAllText(filePath, text, Encoding.UTF8);
        }

        private static string NormalizeNewlines(string s)
            => (s ?? "").Replace("\r\n", "\n").Replace("\r", "\n");
    }
}