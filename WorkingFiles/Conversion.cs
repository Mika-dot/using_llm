using LLama.Common;
using LLM__simple.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace LLM__simple.WorkingFiles
{
    public static class Conversion
    {
        public static List<Story> GetLastNStoriesWithAssistantFirst(List<Story> storList, int N)
        {
            if (storList == null)
                throw new ArgumentNullException(nameof(storList));
            if (N <= 0)
                throw new ArgumentOutOfRangeException(nameof(N), "N must be greater than 0.");

            // Корректируем N, если он больше длины списка
            int count = Math.Min(N, storList.Count);

            // Найти последний элемент с Role.Assistant
            var lastAssistant = storList.LastOrDefault(story => story.Role == Role.System);

            // Получить последние count элементов
            var lastNStories = storList.Skip(Math.Max(0, storList.Count - count)).ToList();

            // Если найден элемент с Role.Assistant и он не в конце списка, переместить его на начало
            if (lastAssistant != null)
            {
                // Удаляем элемент с Role.Assistant из списка
                lastNStories.Remove(lastAssistant);
                // Добавляем его в начало списка
                lastNStories.Insert(0, lastAssistant);
            }

            return lastNStories;
        }

        public static List<Story> MergeStories(List<Story> stories)
        {
            return stories
                .GroupBy(story => story.Role)
                .Select(group => new Story(
                    group.Key,  // Role
                    string.Join(" \t ", group.Select(story => story.Text))
                ))
                .ToList();
        }
        public static ChatHistory StoryToHistory(List<Story> @dialogue)
        {
            ChatHistory chatHistory = new ChatHistory();

            List<Story> Dialogue = MergeStories(@dialogue);

            foreach (var History in Dialogue)
            {
                switch (History.Role)
                {
                    case Role.System:
                        chatHistory.AddMessage(AuthorRole.System, History.Text);
                        break;
                    case Role.User:
                        chatHistory.AddMessage(AuthorRole.User, History.Text);
                        break;
                    case Role.Assistant:
                        chatHistory.AddMessage(AuthorRole.Assistant, History.Text);
                        break;
                    case Role.Unknown:
                        chatHistory.AddMessage(AuthorRole.Unknown, History.Text);
                        break;
                    default:
                        Console.WriteLine($"Вотофак что за роль {History.Role}");
                        break;
                }
            }

            return chatHistory;
        }
    }
}
