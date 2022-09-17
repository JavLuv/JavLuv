using Common;
using System;
using System.IO;
using System.Linq;
using System.Text;
using NTextCat.Commons;
using static System.Net.Mime.MediaTypeNames;

namespace Subtitles
{
    internal class SubtitleReader
    {
        #region Constructor

        public SubtitleReader(StreamReader reader, string ext)
        {
            FileFormat = ext;
            string[] lines = reader.ReadLines().ToArray();

            // Extract text from known file formats
            switch (ext)
            {
                case ".srt":
                    ReadSRT(lines);
                    break;
                case ".vtt":
                    ReadVTT(lines);
                    break;
                case ".ssa":
                case ".ass":
                    ReadSSA(lines);
                    break;
                case ".smi":
                    ReadSMI(lines);
                    break;
                default:
                    // If we can't extract the text, just analyze the file as-is
                    foreach (string line in lines)
                        AddLine(line);
                    break;
            }
            CheckForError(lines, ext);
        }

        #endregion

        #region Properties

        public string Text { get; private set; }

        public string FileFormat { get; private set; }

        public bool IsValid
        {
            get
            {
                return String.IsNullOrWhiteSpace(Text) == false && Text.Length >= MinimumViableTextLength;
            }
        }

        #endregion

        #region Private Functions

        private void CheckForError(string[] lines, string ext)
        {
            if (IsValid)
                return;
            if (ext != ".srt")
            {
                ReadSRT(lines);
                if (IsValid)
                {
                    FileFormat = ".srt";
                    return;
                }
            }
            if (ext != ".vtt")
            {
                ReadVTT(lines);
                if (IsValid)
                {
                    FileFormat = ".vtt";
                    return;
                }
            }
            if (ext != ".ssa" && ext != ".ass")
            {
                ReadSSA(lines);
                if (IsValid)
                {
                    FileFormat = ".ass";
                    return;
                }
            }
            if (ext != ".smi")
            {
                ReadSMI(lines);
                if (IsValid)
                {
                    FileFormat = ".smi";
                    return;
                }
            }

            // Last try - just copy the text and analyze as-is
            Text = String.Empty;
            FileFormat = ext;
            foreach (string line in lines)
                AddLine(line);
        }

        private void ReadSRT(string[] lines)
        {
            Text = String.Empty;
            SRTState state = SRTState.LineNumber;
            foreach (string line in lines)
            {
                switch (state)
                {
                    case SRTState.LineNumber:
                        int val = 0;
                        if (int.TryParse(line, out val))
                            state = SRTState.Timing;
                        else if (line.Contains("-->"))
                            state = SRTState.Text;
                        break;
                    case SRTState.Timing:
                        state = SRTState.Text;
                        break;
                    case SRTState.Text:
                        if (String.IsNullOrEmpty(line))
                            state = SRTState.LineNumber;
                        else
                        {
                            string text = line.StripTags('{', '}');
                            text = line.StripTags('<', '>');
                            AddLine(line);
                        }
                        break;
                }
            }
            Text = m_text.ToString();
        }

        private void ReadVTT(string[] lines)
        {
            Text = String.Empty;
            VTTState state = VTTState.Initial;
            foreach (string line in lines)
            {
                switch (state)
                {
                    case VTTState.Initial:
                        if (line.StartsWith("WEBVTT"))
                            state = VTTState.Text;
                        break;
                    case VTTState.Timing:
                        state = VTTState.Text;
                        break;
                    case VTTState.Text:
                        if (String.IsNullOrWhiteSpace(line))
                            state = VTTState.Timing;
                        else
                        {
                            string text = line.StripTags('<', '>');
                            AddLine(line);
                        }
                        break;
                }
            }
            Text = m_text.ToString();
        }

        private void ReadSSA(string[] lines)
        {
            Text = String.Empty;
            foreach (string line in lines)
            {
                if (line.StartsWith("Dialogue:") == false)
                    continue;
                int index = line.NthIndexOf(',', 9);
                if (index == -1)
                    continue;
                string text = line.Substring(index + 1);
                text = text.StripTags('{', '}');
                text = text.Replace(@"\N", "\n", StringComparison.Ordinal);
                if (String.IsNullOrWhiteSpace(text) == false)
                    AddLine(text);
            }
            Text = m_text.ToString();
        }

        private void ReadSMI(string[] lines)
        {
            Text = String.Empty;
            SMIState state = SMIState.Header;
            foreach (string line in lines)
            {
                switch (state)
                {
                    case SMIState.Header:
                        if (line.StartsWith("<sync start", StringComparison.OrdinalIgnoreCase) == false)
                            break;
                        state = SMIState.Text;
                        goto case SMIState.Text;
                    case SMIState.Text:
                        string text = line.StripTags('<', '>');
                        text = text.Replace("&nbsp;", "", StringComparison.OrdinalIgnoreCase);
                        if (String.IsNullOrWhiteSpace(text))
                            break;
                        AddLine(text);
                        break;
                }
            }
            Text = m_text.ToString();
        }

        private void AddLine(string text)
        {
            m_text.AppendLine(text);
        }

        #endregion

        #region Private Members

        private enum SRTState
        {
            LineNumber,
            Timing,
            Text,
        }

        private enum VTTState
        {
            Initial,
            Timing,
            Text,
        }

        private enum SMIState
        {
            Header,
            Text,
        }

        private readonly int MinimumViableTextLength = 64;

        private StringBuilder m_text = new StringBuilder(10000);

        #endregion
    }
}
