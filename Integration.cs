﻿using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.RegularExpressions;
using CharacterAI_Discord_Bot.Models.Request;
using CharacterAI_Discord_Bot.Models.Response;

namespace CharacterAI_Discord_Bot.Service
{
    public class Integration : IntegrationService
    {
        public bool audienceMode = false;
        public Character charInfo = new();
        private readonly HttpClient _httpClient = new();
        private readonly string? _userToken;

        public Integration(string userToken)
        {
            _userToken = userToken;
        }

        public async Task<bool> Setup(string charID, bool reset)
        {
            Log("\n" + new string ('>', 50), ConsoleColor.Green);
            Log("\nStarting character setup...\n", ConsoleColor.Yellow);
            Log("(ID: " + charID + ")\n\n", ConsoleColor.DarkMagenta);
            charInfo.CharId = charID;

            if (!await GetInfo() || !(reset ? await CreateNewDialog() : await GetHistory()))
                return FailureLog($"\nSetup has been aborted\n{new string('<', 50)}\n");

            Log("(History ID: " + charInfo.HistoryExternalId + ")\n", ConsoleColor.DarkGray);

            await DownloadAvatar();
            SetupCompleteLog(charInfo);

            return SuccessLog(new string('<', 50) + "\n");
        }

        public async Task<StreamingResponseContent> CallCharacter(string msg, string imgPath, int primaryMsgId = 0, int parentMsgId = 0)
        {
            var requestContentModel = new StreamingRequestContent(charInfo, msg, imgPath);

            if (parentMsgId != 0)
                requestContentModel.ParentMessageId = parentMsgId;
            if (primaryMsgId != 0)
            {
                requestContentModel.PrimaryMessageId = primaryMsgId;
                requestContentModel.SeenMessageIds = new int[] { primaryMsgId };
            }
            
            var requestContent = new ByteArrayContent(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(requestContentModel)));
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            HttpRequestMessage request = new(HttpMethod.Post, "https://beta.character.ai/chat/streaming/");
            request.Content = requestContent;
            request.Headers.Add("accept-encoding", "gzip");
            request = SetHeaders(request);

            // Sending message
            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                FailureLog("\nFailed to send message!\n" +
                        $"Details: { response }\n" +
                        $"Response: { await response.Content.ReadAsStringAsync() }\n" +
                        $"Request: { request?.Content?.ReadAsStringAsync().Result }");

                throw new Exception("⚠️ Failed to send message!");
            }
            request.Dispose();

            // Getting answer
            try 
            { 
                var resp = JsonConvert.DeserializeObject<StreamingResponseContent>(await response.Content.ReadAsStringAsync());
                return resp;
            }
            catch (Exception ex)
            { 
                Console.WriteLine(ex.Message);
                throw new Exception("⚠️ Message has been sent successfully, but something went wrong..."); 
            }
        }

        private async Task<bool> GetInfo()
        {
            Log("Fetching character info... ");

            HttpRequestMessage request = new(HttpMethod.Post, "https://beta.character.ai/chat/character/info/");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "external_id", charInfo.CharId! }
            });
            request = SetHeaders(request);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) return FailureLog("Error!\n Request failed! (https://beta.character.ai/chat/character/info/)");

            var content = await response.Content.ReadAsStringAsync();
            var charParsed = JsonConvert.DeserializeObject<dynamic>(content)?.character;
            if (charParsed is null) return FailureLog("Something went wrong...");   

            charInfo.Name = charParsed.name;
            charInfo.Greeting = charParsed.greeting;
            charInfo.Description = charParsed.description;
            charInfo.Title = charParsed.title;
            charInfo.Tgt = charParsed.participant__user__username;
            charInfo.AvatarUrl = $"https://characterai.io/i/400/static/avatars/{charParsed.avatar_file_name}";

            return SuccessLog("OK");
        }

        private async Task<bool> GetHistory()
        {
            Log("Fetching chat history... ");

            HttpRequestMessage request = new(HttpMethod.Post, "https://beta.character.ai/chat/history/continue/");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> {
                { "character_external_id", charInfo.CharId! }
            });
            request = SetHeaders(request);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return FailureLog("Error!\n Request failed! (https://beta.character.ai/chat/history/continue/)");

            var content = await response.Content.ReadAsStringAsync();
            dynamic historyInfo;

            try { historyInfo = JsonConvert.DeserializeObject<dynamic>(content)!; }
            catch (Exception e) { return FailureLog("Something went wrong...\n" + e.ToString()); }

            // If no status field, then history external_id is provided.
            if (historyInfo.status == null)
                charInfo.HistoryExternalId = historyInfo.external_id;
            // If there's status field, then response is "status: No Such History".
            else
            {
                Log("No chat history found\n", ConsoleColor.Magenta);
                return await CreateNewDialog();
            }

            return SuccessLog("OK");
        }

        private async Task<bool> CreateNewDialog()
        {
            Log(" Creating new dialog... ", ConsoleColor.DarkCyan);

            HttpRequestMessage request = new(HttpMethod.Post, "https://beta.character.ai/chat/history/create/");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { 
                { "character_external_id", charInfo.CharId! },
                { "override_history_set", null! }
            });
            request = SetHeaders(request);

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
                return FailureLog("Error!\n  Request failed! (https://beta.character.ai/chat/history/create/)");

            var content = await response.Content.ReadAsStringAsync();
            try { charInfo.HistoryExternalId = JsonConvert.DeserializeObject<dynamic>(content)!.external_id; }
            catch (Exception e) { return FailureLog("Something went wrong...\n" + e.ToString()); }

            return SuccessLog("OK");
        }

        private async Task<bool> DownloadAvatar()
        {
            Log("Downloading character avatar... ");
            Stream image;

            try { if (File.Exists(avatarPath)) File.Delete(avatarPath); }
            catch (Exception e) { return FailureLog("Something went wrong...\n" + e.ToString()); }

            using var avatar = File.Create(avatarPath);
            using var response = await _httpClient.GetAsync(charInfo.AvatarUrl);

            if (response.IsSuccessStatusCode)
            {
                try { image = await response.Content.ReadAsStreamAsync(); }
                catch (Exception e) { return FailureLog("Something went wrong...\n" + e.ToString()); }
            }
            else
            {
                Log($"Error! Request failed! ({charInfo.AvatarUrl})\n", ConsoleColor.Magenta);
                Log(" Setting default avatar... ", ConsoleColor.DarkCyan);

                try { image = new FileStream(defaultAvatarPath, FileMode.Open); }
                catch { return FailureLog($"Something went wrong.\n   Check if img/defaultAvatar.webp does exist."); }
            }
            image.CopyTo(avatar);
            image.Close();

            return SuccessLog("OK");
        }

        public async Task<string?> UploadImg(byte[] img)
        {
            var image = new ByteArrayContent(img);
            image.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");

            var request = new HttpRequestMessage(HttpMethod.Post, "https://beta.character.ai/chat/upload-image/");
            request.Content = new MultipartFormDataContent { { image, "\"image\"", $"\"image.jpg\"" } };        
            request = SetHeaders(request);

            using var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try { return JsonConvert.DeserializeObject<dynamic>(content)!.value.ToString(); }
                catch { FailureLog("Something went wrong"); return null; }
            }

            FailureLog("\nRequest failed! (https://beta.character.ai/chat/upload-image/)\n");
            return null;
        }

        private HttpRequestMessage SetHeaders(HttpRequestMessage request)
        {
            var headers = new string[]
            {
                "Accept", "application/json, text/plain, */*",
                "Authorization", $"Token {_userToken}",
                "accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7",
                "accept-encoding", "deflate, br",
                "ContentLength", request.Content!.ToString()!.Length.ToString() ?? "0",
                "ContentType", "application/json",
                "dnt", "1",
                "Origin", "https://beta.character.ai",
                "Referer", $"https://beta.character.ai/chat?char={charInfo.CharId}",
                "sec-ch-ua", "\"Not?A_Brand\";v=\"8\", \"Chromium\";v=\"108\", \"Google Chrome\";v=\"108\"",
                "sec-ch-ua-mobile", "?0",
                "sec-ch-ua-platform", "Windows",
                "sec-fetch-dest", "empty",
                "sec-fetch-mode", "cors",
                "sec-fetch-site", "same-origin",
                "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Safari/537.36"
            };
            for (int i = 0; i < headers.Length-1; i+=2)
                request.Headers.Add(headers[i], headers[i+1]);

            return request;
        }
    }
}