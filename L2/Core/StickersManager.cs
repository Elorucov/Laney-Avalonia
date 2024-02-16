using ELOR.VKAPILib.Objects;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELOR.Laney.Core {
    public class StickersManager {
        private static Dictionary<string, List<Sticker>> UserStickersForWord = new Dictionary<string, List<Sticker>>();
        private static Dictionary<long, List<string>> WordsForUserSticker = new Dictionary<long, List<string>>();

        private static Dictionary<string, List<Sticker>> PromotedStickersForWord = new Dictionary<string, List<Sticker>>();
        private static Dictionary<long, List<string>> WordsForPromotedSticker = new Dictionary<long, List<string>>();

        public static int WordsCount { get { return UserStickersForWord.Count + PromotedStickersForWord.Count; } }
        public static int StickersCount { get { return WordsForUserSticker.Count + WordsForPromotedSticker.Count; } }
        public static int Chunks { get; private set; } = 0;

        static bool initialized = false;
        public static async void InitKeywords() {
            if (!Settings.SuggestStickers || initialized) return;
            initialized = true;

            UserStickersForWord.Clear();
            WordsForUserSticker.Clear();
            PromotedStickersForWord.Clear();
            WordsForPromotedSticker.Clear();

            int currentChunk = 0;
            string currentHash = String.Empty;
            int totalChunks = 100;
            byte retries = 3;

            await Task.Factory.StartNew(async () => {
                Log.Information($"StickersManager.InitKeywords: now running...");
                do {
                    try {
                        StickersKeywordsResponse skr = await VKSession.Main.API.Store.GetStickersKeywordsAsync(currentChunk, currentHash, false).ConfigureAwait(false);
                        totalChunks = skr.ChunksCount;
                        currentHash = skr.ChunksHash;
                        currentChunk++;
                        AddToDictionary(skr.Dictionary);
                        Log.Information($"StickersManager.InitKeywords: Chunk {currentChunk} of {totalChunks} loaded");
                        await Task.Delay(1000).ConfigureAwait(false);
                    } catch (Exception ex) {
                        Log.Error(ex, $"StickersManager.InitKeywords error!");
                        if (retries > 0) {
                            retries--;
                            await Task.Delay(5000).ConfigureAwait(false);
                        } else {
                            currentChunk = totalChunks;
                        }
                    }
                } while (currentChunk < totalChunks);

                Chunks = currentChunk;
            });
        }

        // ¯\_(ツ)_/¯
        private static void AddToDictionary(List<StickerDictionary> dicts) {
            try {
                Stopwatch sw = Stopwatch.StartNew();
                foreach (var dict in dicts) {
                    // Добавляем слово и подходящие к нему стикеры
                    foreach (string word in dict.Words) {
                        if (dict.UserStickers != null) {
                            if (UserStickersForWord.ContainsKey(word)) {
                                UserStickersForWord[word].AddRange(dict.UserStickers);
                            } else {
                                UserStickersForWord.Add(word, dict.UserStickers);
                            }
                        }
                        if (dict.PromotedStickers != null) {
                            if (PromotedStickersForWord.ContainsKey(word)) {
                                PromotedStickersForWord[word].AddRange(dict.UserStickers);
                            } else {
                                PromotedStickersForWord.Add(word, dict.UserStickers);
                            }
                        }
                    }
                }
                sw.Stop();
                Log.Information($"StickersManager.AddToDictionary: total user words/stickers: {UserStickersForWord.Count}/{WordsForUserSticker.Count}; total promoted words/stickers: {PromotedStickersForWord.Count}/{WordsForPromotedSticker.Count}; elapsed: {sw.ElapsedMilliseconds} ms.");
            } catch (Exception ex) {
                Log.Error(ex, $"StickersManager.AddToDictionary error!");
            }
        }

        public static List<Sticker> GetStickersByWord(string word) {
            if (String.IsNullOrEmpty(word)) return null;
            word = word.ToLower();
            if (UserStickersForWord.ContainsKey(word)) {
                return UserStickersForWord[word];
            }
            return null;
        }

        public static string GetKeywordsForSticker(long stickerId) {
            string wordstr = String.Empty;
            List<string> words = new List<string>();

            if (WordsForUserSticker.ContainsKey(stickerId)) {
                words.AddRange(WordsForUserSticker[stickerId]);
            }
            if (WordsForPromotedSticker.ContainsKey(stickerId)) {
                words.AddRange(WordsForPromotedSticker[stickerId]);
            }
            if (words.Count > 0) wordstr = String.Join(", ", words);

            return wordstr;
        }


        // TODO: фича просмотра подсказок для одного стикера появится в будущем, код скопирован с Laney v1.
        //public static async Task<string> GetKeywordsForStickerFromAPI(long stickerId) {
        //    string wordstr = GetKeywordsForSticker(stickerId);
        //    if (!String.IsNullOrEmpty(wordstr)) return wordstr;

        //    List<string> words = new List<string>();

        //    object resp = await Store.GetStickerKeywords(stickerId);
        //    if (resp is StickersKeywordsResponse skr && skr.Dictionary.Count > 0) {
        //        // Если у юзера есть кастомные подсказки, то они всегда будут возвращаться,
        //        // без разницы, подсказки для какого стикера мы запрашиваем.
        //        // А так как кастомные подсказки уже были добавлены в Init(), 
        //        // то будем игнорить их, так что берём только первый словарь.
        //        var dict = skr.Dictionary[0];
        //        if (dict.UserStickers != null) {
        //            // Надо проверить, есть ли в списке тот стикер, который нам надо
        //            var sticker = dict.UserStickers.Where(s => s.StickerId == stickerId).FirstOrDefault();
        //            if (sticker != null) {
        //                foreach (string word in dict.Words) {
        //                    if (WordsForUserSticker.ContainsKey(stickerId)) {
        //                        if (!WordsForUserSticker[stickerId].Contains(word)) WordsForUserSticker[stickerId].Add(word);
        //                    } else {
        //                        WordsForUserSticker.Add(stickerId, new List<string> { word });
        //                    }
        //                }
        //            }
        //        }
        //        if (dict.PromotedStickers != null) {
        //            var sticker = dict.UserStickers.Where(s => s.StickerId == stickerId).FirstOrDefault();
        //            if (sticker != null) {
        //                foreach (string word in dict.Words) {
        //                    if (WordsForUserSticker.ContainsKey(stickerId)) {
        //                        if (!WordsForUserSticker[stickerId].Contains(word)) WordsForUserSticker[stickerId].Add(word);
        //                    } else {
        //                        WordsForUserSticker.Add(stickerId, new List<string> { word });
        //                    }
        //                }
        //            }
        //        }
        //    } else {
        //        // Show error
        //    }

        //    if (WordsForUserSticker.ContainsKey(stickerId)) {
        //        words.AddRange(WordsForUserSticker[stickerId]);
        //    }
        //    if (WordsForPromotedSticker.ContainsKey(stickerId)) {
        //        words.AddRange(WordsForPromotedSticker[stickerId]);
        //    }
        //    if (words.Count > 0) wordstr = String.Join(", ", words);

        //    return wordstr;
        //}
    }
}