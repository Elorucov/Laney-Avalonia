using Avalonia.Collections;
using ColorTextBlock.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace ELOR.Laney.Helpers {
    enum MatchType { User, Group, LinkInText, Mail, Url }

    class MatchInfo {
        public int Start { get; private set; }
        public int Length { get; private set; }
        public MatchType Type { get; private set; }
        public Match Match { get; private set; }

        public MatchInfo(int start, int length, MatchType type, Match match) {
            Start = start;
            Length = length;
            Type = type;
            Match = match;
        }
    }

    public class TextParser {
        #region Internal parsing methods

        private static Tuple<string, string> ParseBracketWord(Match match) {
            return new Tuple<string, string>($"https://vk.com/{match.Groups[1]}{match.Groups[2]}", match.Groups[3].Value);
        }

        private static Tuple<string, string> ParseLinkInBracketWord(Match match) {
            return new Tuple<string, string>(match.Groups[1].Value, match.Groups[2].Value);
        }

        private static List<Tuple<string, string>> GetRaw(string plain, bool dontParseUrls = false) {
            plain = plain.Trim();
            List<Tuple<string, string>> raw = new List<Tuple<string, string>>();
            List<MatchInfo> allMatches = new List<MatchInfo>();

            var userMatches = CompiledRegularExpressions.UserMention().Matches(plain).Cast<Match>().ToList();
            foreach (var m in CollectionsMarshal.AsSpan(userMatches)) allMatches.Add(new MatchInfo(m.Index, m.Length, MatchType.User, m));

            var groupMatches = CompiledRegularExpressions.GroupMention().Matches(plain).Cast<Match>().ToList();
            foreach (var m in CollectionsMarshal.AsSpan(groupMatches)) allMatches.Add(new MatchInfo(m.Index, m.Length, MatchType.Group, m));

            var linkMatches = CompiledRegularExpressions.LinkInText().Matches(plain).Cast<Match>().ToList();
            foreach (var m in CollectionsMarshal.AsSpan(linkMatches)) allMatches.Add(new MatchInfo(m.Index, m.Length, MatchType.LinkInText, m));

            if (!dontParseUrls) {
                var emailMatches = CompiledRegularExpressions.Email().Matches(plain).Cast<Match>().ToList();
                foreach (var m in CollectionsMarshal.AsSpan(emailMatches)) allMatches.Add(new MatchInfo(m.Index, m.Length, MatchType.Mail, m));

                var urlMatches = CompiledRegularExpressions.URL().Matches(plain).Cast<Match>().ToList();
                foreach (var m in CollectionsMarshal.AsSpan(urlMatches)) allMatches.Add(new MatchInfo(m.Index, m.Length, MatchType.Url, m));
            }

            allMatches = allMatches.OrderBy(m => m.Start).ToList();

            string word = String.Empty;
            for (int i = 0; i < plain.Length; i++) {
                var matchInfo = allMatches.Where(m => m.Start == i).FirstOrDefault();
                if (matchInfo != null) {
                    raw.Add(new Tuple<string, string>(null, word));
                    word = String.Empty;

                    Match match = matchInfo.Match;
                    switch (matchInfo.Type) {
                        case MatchType.User:
                        case MatchType.Group: raw.Add(ParseBracketWord(match)); break;
                        case MatchType.LinkInText: raw.Add(ParseLinkInBracketWord(match)); break;
                        case MatchType.Mail: raw.Add(new Tuple<string, string>($"mailto:{match}", match.Value)); break;
                        case MatchType.Url:
                            string url = match.Value;
                            if (!url.StartsWith("https://") && !url.StartsWith("http://")) url = $"https://{url}";
                            raw.Add(new Tuple<string, string>(url, match.Value));
                            break;
                    }

                    i = i + matchInfo.Length - 1;
                } else {
                    word += plain[i];
                }
            }
            raw.Add(new Tuple<string, string>(null, word));

            return raw;
        }

        #endregion

        #region For CTextBlock

        private static CRun BuildCRunForRTBStyle(string text) {
            return new CRun {
                Text = text
            };
        }

        private static CHyperlink BuildCHyperlinkForRTBStyle(string text, string link, CTextBlock rtb, Action<string> clickedCallback) {
            CHyperlink hl = new CHyperlink(new List<CInline> {
                new CRun() {
                    Text = text
                }
            }) {
                CommandParameter = link
            };
            hl.Command = (a) => clickedCallback?.Invoke(a);
            return hl;
        }

        public static void SetText(string plain, CTextBlock rtb, Action<string> linksClickedCallback = null) {
            rtb.Content = new AvaloniaList<CInline>();
            if (string.IsNullOrEmpty(plain)) return;

            foreach (var token in GetRaw(plain)) {
                if (string.IsNullOrEmpty(token.Item1)) {
                    rtb.Content.Add(BuildCRunForRTBStyle(token.Item2));
                } else {
                    CHyperlink h = BuildCHyperlinkForRTBStyle(token.Item2, token.Item1, rtb, linksClickedCallback);
                    rtb.Content.Add(h);
                }
            }
        }

        #endregion

        public static string GetParsedText(string plain) {
            if (string.IsNullOrEmpty(plain)) return string.Empty;
            StringBuilder sb = StringBuilderCache.Acquire(plain.Length);

            foreach (var token in CollectionsMarshal.AsSpan(GetRaw(plain))) {
                sb.Append(token.Item2);
            }
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static long GetMentionId(string plain) {
            var u = CompiledRegularExpressions.UserMention().Match(plain);
            if (u.Success) {
                return long.Parse(u.Groups[2].Value);
            } else {
                var g = CompiledRegularExpressions.GroupMention().Match(plain);
                if (g.Success) return -long.Parse(g.Groups[2].Value);
            }
            return 0;
        }
    }
}